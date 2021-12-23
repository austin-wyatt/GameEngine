using MortalDungeon.Game.Ledger;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace MortalDungeon.Game
{
    public static class DialogueLedger
    {
        public static Dictionary<int, DialogueLedgerNode> LedgeredDialogues = new Dictionary<int, DialogueLedgerNode>();

        public static void AddOutcome(int dialogueID, int outcome)
        {
            DialogueLedgerNode node;

            if (LedgeredDialogues.TryGetValue(dialogueID, out var n))
            {
                node = n;
            }
            else
            {
                node = new DialogueLedgerNode() { ID = dialogueID };
                LedgeredDialogues.Add(dialogueID, node);
            }

            if(outcome > 0)
            {
                node.RecievedOutcomes.Add(outcome);

                Ledgers.LedgerUpdated(LedgerUpdateType.Dialogue, dialogueID, outcome);
            }
        }

        /// <summary>
        /// Returns true if the outcome has been achieved and false otherwise
        /// </summary>
        public static bool GetOutcome(int dialogueID, int outcome)
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
