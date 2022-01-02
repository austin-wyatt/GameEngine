using MortalDungeon.Game.Ledger;
using System;
using System.Collections.Generic;
using System.Text;
using MortalDungeon.Game.Serializers;

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

        public static void ModifyStateValue(StateIDValuePair stateValue)
        {
            #region instructions
            if (stateValue.Data == (long)QuestStates.Start)
            {
                QuestManager.StartQuest((int)stateValue.StateID);
                return;
            }
            #endregion


            QuestLedgerNode node;

            if (LedgeredQuests.TryGetValue((int)stateValue.StateID, out var n))
            {
                node = n;
            }
            else
            {
                node = new QuestLedgerNode() { ID = (int)stateValue.StateID };
                LedgeredQuests.Add((int)stateValue.StateID, node);
            }

            if (stateValue.Instruction == (short)StateInstructions.Set)
            {
                node.QuestState.Add(stateValue.Data);
            }
            else if(stateValue.Instruction == (short)StateInstructions.Clear)
            {
                node.QuestState.Remove(stateValue.Data);
            }

            Ledgers.LedgerUpdated(stateValue);
        }

        public static bool GetStateValue(int questID, int stateValue)
        {
            if (LedgeredQuests.TryGetValue(questID, out var n))
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
        Completed = 99857,
        Start = 99858
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
