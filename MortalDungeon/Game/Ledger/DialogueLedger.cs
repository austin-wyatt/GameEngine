using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Game.Ledger;
using MortalDungeon.Game.Serializers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace MortalDungeon.Game
{
    public static class DialogueLedger
    {
        public static Dictionary<int, DialogueLedgerNode> LedgeredDialogues = new Dictionary<int, DialogueLedgerNode>();

        public static void SetStateValue(StateIDValuePair stateValue)
        {
            DialogueLedgerNode node;

            #region instructions
            if (stateValue.Data == (int)DialogueStates.CreateDialogue)
            {
                //create dialogue here (maybe through a dialogue manager or something).
                //as it is we don't have a good way to define which units should be present in the dialogue
                //but that isn't a huge deal.
                return; //this is an instruction so we don't want to actually set the value
            }
            #endregion


            if (LedgeredDialogues.TryGetValue((int)stateValue.StateID, out var n))
            {
                node = n;
            }
            else
            {
                node = new DialogueLedgerNode() { ID = (int)stateValue.StateID };
                LedgeredDialogues.Add((int)stateValue.StateID, node);
            }

            if(stateValue.Data > 0)
            {
                node.RecievedOutcomes.Add(stateValue.Data);

                Ledgers.LedgerUpdated(stateValue);
            }
        }

        public static void RemoveStateValue(StateIDValuePair stateValue)
        {
            DialogueLedgerNode node = null;

            if (LedgeredDialogues.TryGetValue((int)stateValue.StateID, out var n))
            {
                node = n;
            }

            if (node != null)
            {
                node.RecievedOutcomes.Remove(stateValue.Data);
            }
        }

        /// <summary>
        /// Returns true if the outcome has been achieved and false otherwise
        /// </summary>
        public static bool GetStateValue(int dialogueID, int outcome)
        {
            if (LedgeredDialogues.TryGetValue(dialogueID, out var n))
            {
                if (n.RecievedOutcomes.TryGetValue(outcome, out var i))
                {
                    return true;
                }
            }

            return false;
        }
    }

    public class DialogueLedgerNode
    {
        public int ID;

        /// <summary>
        /// Records all recieved outcomes of any given dialogue. <para/>
        /// Example, if a quest required the dialogue ID of 50 to have recieved an 
        /// outcome of 5 then it can be easily checked.
        /// </summary>
        public HashSet<int> RecievedOutcomes = new HashSet<int>();
    }
}
