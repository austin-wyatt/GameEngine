using MortalDungeon.Game.Ledger;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace MortalDungeon.Game.Quests
{
    [Serializable]
    public class Quest
    {
        public int ID = 0;

        [XmlElement("Qsts")]
        public List<QuestState> QuestStates = new List<QuestState>();

        [XmlElement("Qcs")]
        public int CurrentState = 0;
        public Quest() { }

        public void CheckObjectives(LedgerUpdateType type, long id, long data)
        {
            if(CurrentState < QuestStates.Count)
            {
                if (QuestStates[CurrentState].IsStateCompleted(type, id, data))
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

            if(CurrentState >= QuestStates.Count)
            {
                QuestManager.CompleteQuest(this);
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

        public QuestState() { }

        public bool IsStateCompleted(LedgerUpdateType type, long id, long data)
        {
            foreach (var obj in QuestObjectives)
            {
                if (!obj.IsObjectiveCompleted(type, id, data))
                {
                    return false;
                }
            }

            return true;
        }
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
        [XmlElement("QOt")]
        public QuestObjectiveType Type;
        [XmlElement("QOi")]
        public long ItemID;
        [XmlElement("QOdi")]
        public long RelevantDataID;
        [XmlElement("QOev")]
        public int ExpectedValue;
        [XmlElement("QOls")]
        public int LocalStorage;

        public QuestObjective() { }

        public bool IsObjectiveCompleted(LedgerUpdateType type, long id, long data)
        {
            switch (Type)
            {
                case QuestObjectiveType.Feature:
                    if((int)FeatureLedger.GetInteraction(ItemID, RelevantDataID) == ExpectedValue)
                    {
                        return true;
                    }
                    break;
                case QuestObjectiveType.Dialogue:
                    if (DialogueLedger.GetOutcome((int)ItemID, (int)RelevantDataID))
                    {
                        return true;
                    }
                    break;
                case QuestObjectiveType.Quest:
                    if (QuestLedger.GetStateValue((int)ItemID, (int)RelevantDataID))
                    {
                        return true;
                    }
                    break;
                default:
                    return false;
            }

            return false;
        }
    }
}
