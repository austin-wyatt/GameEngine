using MortalDungeon.Game.Ledger;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game
{
    public enum StateModificationOptions
    {
        Add,
        Remove
    }

    public static class QuestLedger
    {
        public static Dictionary<int, QuestLedgerNode> LedgeredQuests = new Dictionary<int, QuestLedgerNode>();

        public static void ModifyStateValue(int questID, int state, StateModificationOptions option = StateModificationOptions.Add)
        {
            QuestLedgerNode node;

            if (LedgeredQuests.TryGetValue(questID, out var n))
            {
                node = n;
            }
            else
            {
                node = new QuestLedgerNode() { ID = questID };
                LedgeredQuests.Add(questID, node);
            }

            if (option == StateModificationOptions.Add)
            {
                node.QuestState.Add(state);
            }
            else if(option == StateModificationOptions.Remove)
            {
                node.QuestState.Remove(state);
            }

            Ledgers.LedgerUpdated(LedgerUpdateType.Quest, questID, state);
        }

        public static bool GetStateValue(int dialogueID, int stateValue)
        {
            if (LedgeredQuests.TryGetValue(dialogueID, out var n))
            {
                if (n.QuestState.TryGetValue(stateValue, out var i))
                {
                    return true;
                }
            }

            return false;
        }
    }

    public enum QuestStates
    {
        Completed = 99857
    }

    public class QuestLedgerNode
    {
        public int ID;

        /// <summary>
        /// Stores quest states. <para/>
        /// Example: You want to train your magic at a magic trainer but he requires that you've read 
        /// some specific magic words before. These words are recorded in dozens of areas throughout the game
        /// but checking each specific one would be too much work. So instead we can apply a random int to track 
        /// this value to the quest's quest state. <para/>
        /// </summary>
        public HashSet<int> QuestState = new HashSet<int>();
    }
}
