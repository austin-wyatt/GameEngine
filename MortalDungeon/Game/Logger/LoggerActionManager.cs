using Empyrean.Game.Scripting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Empyrean.Game.Logger
{
    public static class LoggerActionManager
    {
        public static Dictionary<string, HashSet<Dictionary<string, object>>> LoggerActions = new Dictionary<string, HashSet<Dictionary<string, object>>>();
        public static object _loggerActionLock = new object();

        private static readonly HashSet<Dictionary<string, object>> _actionsToRemove = new HashSet<Dictionary<string, object>>();
        private static readonly HashSet<Dictionary<string, object>> _actionsToAdd = new HashSet<Dictionary<string, object>>();

        /// <summary>
        /// Evaluate whether a received packet triggers a logger action
        /// </summary>
        public static void ProcessLoggerActions(LoggerPacket packet, string packetType) 
        {
            lock (_loggerActionLock)
            {
                if (LoggerActions.TryGetValue(packetType, out var actions))
                {
                    foreach (Dictionary<string, object> loggerAction in actions)
                    {
                        if (EvaluateActionParameters(packet, loggerAction))
                        {
                            ExecuteAction(packet, loggerAction);
                        }
                    }
                }
            }

            ProcessAdditionsAndRemovals();
        }

        private static bool EvaluateActionParameters(LoggerPacket packet, Dictionary<string, object> loggerAction) 
        {
            //if a loggerAction contains a key of "_x" it will be considered invalid and should not be processed
            if (loggerAction.ContainsKey(_strings[(int)STRINGS.InvalidKey]))
                return false;

            if (loggerAction.ContainsKey(_strings[(int)STRINGS.Parameters]))
            {
                return EvaluateScript<bool>(packet, loggerAction, _strings[(int)STRINGS.Parameters]);
            }

            return false;
        }

        private static void ExecuteAction(LoggerPacket packet, Dictionary<string, object> loggerAction) 
        {
            string outputString;

            if (loggerAction.ContainsKey(_strings[(int)STRINGS.Script]))
            {
                EvaluateScript<bool>(packet, loggerAction, _strings[(int)STRINGS.Script]);
            }

            //Check "persistent" field for value "f". If "f", make the action invalid and add it to _actionsToRemove
            if (loggerAction.TryGetValue(_strings[(int)STRINGS.Persistent], out object value))
            {
                outputString = value as string;
                if(string.Equals(outputString, _strings[(int)STRINGS.False]))
                {
                    RemoveLoggerAction(loggerAction);
                }
            }
        }

        public static T EvaluateScript<T>(LoggerPacket packet, Dictionary<string, object> loggerAction, string scriptKey)
        {
            //format strings:
            //from packet !< >! 
            //from data object @< >@
            //no formatting required for global objects

            string rawScript = loggerAction.GetValueOrDefault(scriptKey) as string;

            if (rawScript == null)
                throw new Exception("Invalid script with id " + loggerAction.GetValueOrDefault(_strings[(int)STRINGS.Id]) + " attempted execution");

            //stores pairs indices in the form (start index, end index, start index, end index)
            //in the case of a malformed format string the system will fail at a later step
            List<int> packetIndices = new List<int>();
            List<int> localIndices = new List<int>();
            
            for(int i = 0; i < rawScript.Length - 1; i++)
            {
                //Check for the packet and local format string indicators 
                if (rawScript[i] == _strings[(int)STRINGS.PacketStart][0] && rawScript[i + 1] == _strings[(int)STRINGS.PacketStart][1]
                    || rawScript[i] == _strings[(int)STRINGS.PacketEnd][0] && rawScript[i + 1] == _strings[(int)STRINGS.PacketEnd][1])
                {
                    packetIndices.Add(i);
                }
                else if (rawScript[i] == _strings[(int)STRINGS.LocalStart][0] && rawScript[i + 1] == _strings[(int)STRINGS.LocalStart][1] 
                         || rawScript[i] == _strings[(int)STRINGS.LocalEnd][0] && rawScript[i + 1] == _strings[(int)STRINGS.LocalEnd][1])
                {
                    localIndices.Add(i);
                }
            }

            //if we have a mismatch of packet and local indices there is no point attempting to execute the string
            if (packetIndices.Count % 2 == 1 || localIndices.Count % 2 == 1)
                throw new Exception("Invalid script with id " + loggerAction.GetValueOrDefault(_strings[(int)STRINGS.Id]) + " attempted execution");

            //use StringBuilder and spans (as opposed to concantenation and substrings) to reduce allocations
            StringBuilder scriptString = new StringBuilder(rawScript.Length);

            int packetIndex = 0;
            int localIndex = 0;
            int len;

            int startIndex;
            int endIndex = 0;
            string formatSubstr;

            List<string> exposedObjects = new List<string>();

            //Build the formatted script string
            for (int i = 0; i < packetIndices.Count + localIndices.Count; i += 2)
            {
                if (packetIndices[packetIndex] < localIndices[localIndex])
                {
                    //append the string between the format indicators
                    len = packetIndices[packetIndex] - endIndex;
                    scriptString.Append(rawScript.AsSpan(endIndex, len));

                    startIndex = packetIndices[packetIndex] + 2; //add the length of the format indicator
                    len = packetIndices[packetIndex + 1] - startIndex;

                    packetIndex += 2;

                    formatSubstr = _strings[(int)STRINGS.PacketPrefix] + rawScript.AsSpan(startIndex, len).ToString();

                    if (packet.TryGetValue(rawScript.AsSpan(startIndex, len).ToString(), out object value))
                    {
                        JSManager.ExposeObject(formatSubstr, value);
                        exposedObjects.Add(formatSubstr);
                    }
                }
                else
                {
                    //append the substring between the format indicators
                    len = localIndices[localIndex] - endIndex;
                    scriptString.Append(rawScript.AsSpan(endIndex, len));

                    startIndex = localIndices[localIndex] + 2; //add the length of the format indicator
                    len = localIndices[localIndex + 1] - startIndex;

                    localIndex += 2;

                    formatSubstr = _strings[(int)STRINGS.LocalPrefix] + rawScript.AsSpan(startIndex, len).ToString();

                    if (loggerAction.TryGetValue(rawScript.AsSpan(startIndex, len).ToString(), out object value))
                    {
                        JSManager.ExposeObject(formatSubstr, value);
                        exposedObjects.Add(formatSubstr);
                    }
                }

                //append the object string without format indicators now that the object has been exposed to the script
                scriptString.Append(formatSubstr);

                endIndex = startIndex + len + 2;
            }

            //append the final unformatted section of the raw script string
            scriptString.Append(rawScript.AsSpan(endIndex, rawScript.Length - endIndex));



            object evaluatedOutput;

            //Evaluate the script and receive the output
            try
            {
                evaluatedOutput = JSManager.ApplyScript(scriptString.ToString());
            }
            catch(Exception ex)
            {
                throw ex;
            }
            finally
            {
                for (int i = 0; i < exposedObjects.Count; i++)
                {
                    JSManager.RemoveObject(exposedObjects[i]);
                }
            }

            //attempt to cast the object returned from the script to T
            return (T)evaluatedOutput;
        }
        

        public static void ProcessAdditionsAndRemovals()
        {
            Monitor.Enter(_actionsToAdd);
            Monitor.Enter(_actionsToRemove);
            Monitor.Enter(_loggerActionLock);

            HashSet<Dictionary<string, object>> storedActions;

            string actionType;

            foreach (var action in _actionsToRemove)
            {
                actionType = GetActionTypeString(action);
                if (LoggerActions.TryGetValue(actionType, out storedActions))
                {
                    storedActions.Remove(action);

                    if(storedActions.Count == 0)
                    {
                        LoggerActions.Remove(actionType);
                    }
                }
            }

            _actionsToRemove.Clear();

            foreach (var action in _actionsToAdd)
            {
                actionType = GetActionTypeString(action);
                if (LoggerActions.TryGetValue(actionType, out storedActions))
                {
                    storedActions.Add(action);
                }
                else
                {
                    storedActions = new HashSet<Dictionary<string, object>>();
                    storedActions.Add(action);

                    LoggerActions.Add(actionType, storedActions);
                }
            }

            _actionsToAdd.Clear();

            Monitor.Exit(_actionsToAdd);
            Monitor.Exit(_actionsToRemove);
            Monitor.Exit(_loggerActionLock);
        }

        public static void RemoveLoggerAction(Dictionary<string, object> loggerAction)
        {
            lock (_actionsToRemove)
            {
                //invalidate the action before slating it for removal
                loggerAction.TryAdd(_strings[(int)STRINGS.InvalidKey], null);
                _actionsToRemove.Add(loggerAction);
            }
        }

        public static void AddLoggerAction(Dictionary<string, object> loggerAction)
        {
            lock (_actionsToAdd)
            {
                _actionsToAdd.Add(loggerAction);
            }
        }

        private static string GetActionTypeString(Dictionary<string, object> loggerAction)
        {
            if (loggerAction.TryGetValue(_strings[(int)STRINGS.Type], out object obj))
            {
                if (obj.GetType() == typeof(long))
                {
                    return ((LoggerEventType)obj).ToString();
                }
                else
                {
                    return (string)obj;
                }
            }

            return null;
        }
        
        private enum STRINGS
        {
            Persistent,
            False,
            InvalidKey,
            Type,
            Id,
            Parameters,
            Script,

            PacketStart,
            PacketEnd,
            LocalStart,
            LocalEnd,
            PacketPrefix,
            LocalPrefix
        }

        //Since this class will be called frequently during gameplay it's worth statically defining common strings to reduce allocations
        private static readonly string[] _strings = new string[]
        {
            "persistent",
            "f",
            "_x",
            "type",
            "id",
            "parameters",
            "script",

            "!<",
            ">!",
            "@<",
            ">@",
            "P_",
            "L_"
        };
    }
}
