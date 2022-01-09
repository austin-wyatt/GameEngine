using MortalDungeon.Game.Ledger;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace MortalDungeon.Game.Serializers
{
    [Serializable]
    public class Quest
    {
        public int ID = 0;

        [XmlElement("Qsts")]
        public List<QuestState> QuestStates = new List<QuestState>();

        [XmlElement("Qcs")]
        public int CurrentState = 0;

        [XmlElement("Qsn")]
        public string Name = "";
        [XmlElement("QsHe")]
        public int Title = 0;

        [XmlElement("QsBo")]
        public int Body = 0;

        public Quest() { }

        public void CheckObjectives()
        {
            if(CurrentState < QuestStates.Count)
            {
                if (QuestStates[CurrentState].IsStateCompleted(ID, CurrentState))
                {
                    AdvanceQuestState();
                }
            }
            else
            {
                QuestManager.CompleteQuest(this);
            }
        }

        /// <summary>
        /// When all quest objectives for the current quest state are completed then we advance to the next state
        /// </summary>
        public void AdvanceQuestState()
        {
            CurrentState++;

            if (CurrentState >= QuestStates.Count)
            {
                QuestManager.CompleteQuest(this);
            }
            else
            {
                AddCurrentStateObjectivesToSubscriber();
            }
        }

        public void AddCurrentStateObjectivesToSubscriber()
        {
            foreach(var obj in QuestStates[CurrentState].QuestObjectives)
            {
                Ledgers.EvaluateInstruction(obj.Instruction);
            }
        }
    }

    [XmlType(TypeName = "QST")]
    public class QuestState
    {
        [XmlElement("QSo")]
        public List<QuestObjective> QuestObjectives = new List<QuestObjective>();

        [XmlElement("QSt")]
        public string StateText = "";

        [XmlElement("QSte")]
        public int TextEntry = 0;

        public QuestState() { }

        public bool IsStateCompleted(int questID, int stateIndex)
        {
            int count = 0;
            foreach (var obj in QuestObjectives)
            {
                if (!QuestLedger.GetStateValue(questID, (int)QuestStates.State0 + (int)QuestStates.StateOffset * stateIndex + count + 1))
                {
                    return false;
                }

                count++;
            }

            return true;
        }

        //public void ClearState(int questID)
        //{
        //    for(int i = 0; i < QuestObjectives.Count; i++)
        //    {
        //        StateIDValuePair clearValue = new StateIDValuePair()
        //        {
        //            Type = (int)LedgerUpdateType.Quest,
        //            StateID = questID,
        //            ObjectHash = 0,
        //            Data = (int)QuestStates.CompleteObjective0 + i,
        //            Instruction = (short)StateInstructions.Clear
        //        };

        //        Ledgers.ApplyStateValue(clearValue);
        //    }
        //}
    }

    public enum QuestObjectiveType
    {
        Dialogue, //Checks if a specific outcome was achieved in a dialogue. For very general dialogue options a quest state value should be used.
        Feature, //Checks whether an interaction of a certain type (specified by ExpectedValue) has occurred on a point (specified by RelevantDataID). 
        Quest, //Checks a state value (identified by the RelevantDataID) for any given quest (identified by ItemID).
        GeneralState,
        /// <summary>
        /// 
        /// </summary>
        Kill,
        /// <summary>
        /// Will check the passed type, id, and data values. This could be useful for quests that are general like "complete 5 quests"
        /// </summary>
        CheckPassedData

    }

    [XmlType(TypeName = "QO")]
    public class QuestObjective
    {
        public Instructions Instruction = new Instructions();

        [XmlElement("QOn")]
        public string Name;

        [XmlElement("QOte")]
        public int TextEntry = 0;

        public QuestObjective() { }
    }
}
