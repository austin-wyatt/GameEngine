using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.ClearScript.V8;
using Microsoft.ClearScript;
using Empyrean.Game.Ledger;
using Empyrean.Game.Serializers;
using Empyrean.Game.Player;
using Empyrean.Game.Units;
using DataObjects;
using Empyrean.Game.Settings;

namespace Empyrean.Game.Scripting
{
    public static class JSManager
    {
        public static V8ScriptEngine Engine = new V8ScriptEngine();

        public static void Initialize()
        {
            Engine.ExposeHostObjectStaticMembers = true;

            Engine.AddHostType("StateIDValuePair", typeof(StateIDValuePair));
            Engine.AddHostType("PlayerParty", typeof(PlayerParty));

            Engine.AddHostType("DataSearchRequest", typeof(DataSearchRequest));
            Engine.AddHostType("DataObjectEntry", typeof(DataObjectEntry));
            Engine.AddHostType("object", typeof(object));

            Engine.AddHostType("DataSourceManager", typeof(DataSourceManager));
            Engine.AddHostType("SettingsManager", typeof(SettingsManager));

            Engine.AddHostObject("mscorlib", new HostTypeCollection("mscorlib"));
            Engine.AddHostObject("host", new ExtendedHostFunctions());

            Engine.Evaluate(File.ReadAllText("Game/Scripting/init.js"));

            

            //Engine.Evaluate("Empyrean.Game.Player.PlayerParty.Inventory.AddGold(500)");
        }

        public static object ApplyScript(string script)
        {
            return Engine.Evaluate(script);
        }

        /// <summary>
        /// Expose object to the script environment with the given name
        /// </summary>
        public static void ExposeObject(string name, object obj)
        {
            Engine.Script[name] = obj;
        }

        /// <summary>
        /// Remove a previously exposed object from the script environment
        /// </summary>
        public static void RemoveObject(string name)
        {
            Engine.Execute("delete " + name);
        }

        public static T EvaluateScript<T>(string rawScript)
        {
            if (rawScript == null)
                throw new Exception("Invalid script attempted execution");

            HashSet<string> exposedObjects = new HashSet<string>();
            string scriptString = ScriptFormat.FormatString(rawScript.AsSpan(), null, null, ref exposedObjects);

            object evaluatedOutput;

            //Evaluate the script and receive the output
            try
            {
                evaluatedOutput = JSManager.ApplyScript(scriptString.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return default(T);
            }
            finally
            {
                foreach (string exposedObject in exposedObjects)
                {
                    JSManager.RemoveObject(exposedObject);
                }
            }

            //attempt to cast the object returned from the script to T
            return (T)evaluatedOutput;
        }
    }
}
