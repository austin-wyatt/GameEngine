using MortalDungeon.Game.Ledger;
using MortalDungeon.Game.Serializers;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Events
{
    public abstract class EventAction
    {
        public string EventTrigger;

        public List<string> StateFlags = new List<string>();

        /// <summary>
        /// Convert object parameters into what the action requires
        /// </summary>
        public abstract void BuildEvent(List<dynamic> parameters);

        public abstract void Invoke(params dynamic[] parameters);

        public bool CheckStateFlags()
        {
            bool stateFlag = true;

            foreach(string flag in StateFlags)
            {
                var split = flag.Split(':');

                string strippedFlag = split[0];

                string[] parameters = new string[0];

                if(split.Length > 1)
                {
                    parameters = split[1].Split(',');
                }

                switch (strippedFlag)
                {
                    case "in_combat":
                        stateFlag = EventManager.Scene.InCombat;
                        break;
                    case "out_combat":
                        stateFlag = !EventManager.Scene.InCombat;
                        break;
                    case "quest_completed":
                        stateFlag = QuestManager.GetQuestCompleted(int.Parse(parameters[0]));
                        break;
                    case "state_value":
                        stateFlag = Ledgers.GetStateValue((LedgerUpdateType)int.Parse(parameters[0]), int.Parse(parameters[1]), int.Parse(parameters[2]), int.Parse(parameters[3])) == int.Parse(parameters[4]);
                        break;
                }

                if(!stateFlag)
                    return false;
            }

            return stateFlag;
        }
    }
}
