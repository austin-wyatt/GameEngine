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

namespace Empyrean.Game.Scripting
{
    public static class JSManager
    {
        public static V8ScriptEngine Engine = new V8ScriptEngine();

        public static void Initialize()
        {
            Engine.ExposeHostObjectStaticMembers = true;

            Engine.AddHostType("Ledgers", typeof(Ledgers));
            Engine.AddHostType("QuestLedger", typeof(QuestLedger));
            Engine.AddHostType("QuestManager", typeof(QuestManager));
            Engine.AddHostType("StateIDValuePair", typeof(StateIDValuePair));
            Engine.AddHostType("StateSubscriber", typeof(StateSubscriber));
            Engine.AddHostType("PlayerParty", typeof(PlayerParty));

            //temp
            //Engine.AddHostType("UnitInfo", typeof(UnitInfo));

            Engine.AddHostObject("mscorlib", new HostTypeCollection("mscorlib"));

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
    }
}
