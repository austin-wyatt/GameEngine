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
            ProcessAdditionsAndRemovals();

            lock (_loggerActionLock)
            {
                if (LoggerActions.TryGetValue(packetType, out var actions))
                {
                    foreach (Dictionary<string, object> loggerAction in actions)
                    {
                        //evaluate the "parameters" script of the logger action to see if the packet applies
                        if (EvaluateActionParameters(packet, loggerAction))
                        {
                            ExecuteAction(packet, loggerAction);
                        }
                    }
                }
            }
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
                EvaluateScript<object>(packet, loggerAction, _strings[(int)STRINGS.Script]);
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
            string rawScript = loggerAction.GetValueOrDefault(scriptKey) as string;

            if (rawScript == null)
                throw new Exception("Invalid script with id " + loggerAction.GetValueOrDefault(_strings[(int)STRINGS.Id]) + " attempted execution");

            HashSet<string> exposedObjects = new HashSet<string>();
            string scriptString = ScriptFormat.FormatString(rawScript.AsSpan(), packet, loggerAction, ref exposedObjects);

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
                foreach(string exposedObject in exposedObjects)
                {
                    JSManager.RemoveObject(exposedObject);
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

            if (!(_actionsToAdd.Count > 0 || _actionsToRemove.Count > 0))
                return;

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

        private class ScriptFormatIndex
        {
            public FormatType Type;
            public int Start;
            public int End;
            public bool ExposeObject = true;

            public ScriptFormatIndex(FormatType type, int start)
            {
                Type = type;
                Start = start;
            }
        }

        private enum FormatType
        {
            Packet,
            Local,
            DataObject,
            DataObjectInt,
            DataObjectFloat,
            DataObjectString,
            DataObjectDouble,
            DataObjectLong,
            DataObjectBool
        }

        private enum FormatState
        {
            Clear,
            PacketStart,
            LocalStart,
            DOStart,
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
            DOStart,
            DOEnd
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
            "@@",
            "@@"
        };
    }
}
