using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.ClearScript.V8;
using Microsoft.ClearScript;
using Empyrean.Game.Ledger;
using Empyrean.Game.Serializers;
using Empyrean.Game.Player;

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

            Engine.AddHostObject("mscorlib", new HostTypeCollection("mscorlib"));

            Engine.Evaluate(File.ReadAllText("Game/Scripting/init.js"));


            //Engine.Evaluate("Empyrean.Game.Player.PlayerParty.Inventory.AddGold(500)");
        }

        public static object ApplyScript(string script)
        {
            return Engine.Evaluate(script);
        }
    }
}
