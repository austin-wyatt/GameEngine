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
using Empyrean.Game.Quests;
using Empyrean.Game.Logger;
using Empyrean.Engine_Classes.Text;

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

            Engine.AddHostType("WindowConstants", typeof(WindowConstants));
            Engine.AddHostType("Window", typeof(Window));
            
            Engine.AddHostType("DataSourceManager", typeof(DataSourceManager));
            Engine.AddHostType("SettingsManager", typeof(SettingsManager));
            Engine.AddHostType("QuestManager", typeof(QuestManager));
            Engine.AddHostType("LoggerActionManager", typeof(LoggerActionManager));
            Engine.AddHostType("GenericStatus", typeof(GenericStatus));
            Engine.AddHostType("EXTENSIONS", typeof(Engine_Classes.Extensions));
            Engine.AddHostType("TextEntry", typeof(TextEntry));

            Engine.AddHostType("DictT", typeof(Dictionary<string, object>));

            Engine.AddHostObject("mscorlib", new HostTypeCollection("mscorlib"));
            Engine.AddHostObject("host", new ExtendedHostFunctions());

            AddTestHostTypes();

            Engine.Evaluate(File.ReadAllText("Game/Scripting/init.js"));

            //Engine.Evaluate("Empyrean.Game.Player.PlayerParty.Inventory.AddGold(500)");
        }

        private static void AddTestHostTypes()
        {
            Engine.AddHostType("GlyphLoader", typeof(GlyphLoader));
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

        public static T EvaluateScript<T>(string rawScript, Dictionary<string, object> localObject = null, Dictionary<string, object> externalObject = null)
        {
            if (rawScript == null)
                throw new Exception("Invalid script attempted execution");

            HashSet<string> exposedObjects = new HashSet<string>();
            string scriptString = ScriptFormat.FormatString(rawScript.AsSpan(), externalObject, localObject, ref exposedObjects);

            object evaluatedOutput;

            //Evaluate the script and receive the output
            try
            {
                evaluatedOutput = ApplyScript(scriptString.ToString());
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
                    RemoveObject(exposedObject);
                }
            }

            //attempt to cast the object returned from the script to T
            return (T)evaluatedOutput;
        }
    }
}
