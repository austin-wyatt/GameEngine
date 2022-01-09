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



        public static void CompleteQuestObjective(int questId, int stateIndex, int objectiveIndex)
        {
            StateIDValuePair state = new StateIDValuePair();

            state.Type = (int)LedgerUpdateType.Quest;
            state.StateID = questId;
            state.Data = (int)QuestStates.State0 + (int)QuestStates.StateOffset * stateIndex + objectiveIndex + 1;

            Ledgers.ApplyStateValue(state);
        }

        public static void StartQuest(int questId)
        {
            StateIDValuePair state = new StateIDValuePair();
            state.Type = (int)LedgerUpdateType.Quest;
            state.StateID = questId;
            state.Data = (int)QuestStates.Start;

            Ledgers.ApplyStateValue(state);
        }
    }

    public enum QuestStates
    {
        Completed = 99857,
        Start = 99858,

        StateOffset = 10000,
        State0 = 100000, 
        Objective0 = 100001,
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
