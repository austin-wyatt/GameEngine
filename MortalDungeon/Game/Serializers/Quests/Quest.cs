using MortalDungeon.Game.Ledger;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Xml.Serialization;

namespace MortalDungeon.Game.Serializers
{
    [Serializable]
    public class Quest : ISerializable
    {
        public int ID = 0;

        [XmlElement("Qsts")]
        public List<QuestState> QuestStates = new List<QuestState>();

        [XmlElement("Qcs")]
        public int CurrentState = 0;

        [XmlElement("Qsn")]
        public string Name = "";
        [XmlElement("QsHe")]
        public TextInfo Title = new TextInfo();

        [XmlElement("QsBo")]
        public TextInfo Body = new TextInfo();

        [XmlElement("Qsrew")]
        public QuestReward QuestReward = new QuestReward();

        [XmlElement("QsS")]
        public double Scale = 1;

        [XmlElement("QsPs")]
        public Vector2 _position = new Vector2();

        public Quest() { }

        public void CheckObjectives()
        {
            if(CurrentState < QuestStates.Count)
            {
                if (QuestStates[CurrentState].IsStateCompleted())
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

        public void PrepareForSerialization()
        {
            QuestReward.PrepareForSerialization();
        }

        public void CompleteDeserialization()
        {
            QuestReward.CompleteDeserialization();

            int i = 0;

            foreach (var state in QuestStates)
            {
                int j = 0;

                state.Parent = this;
                state._stateIndex = i;

                foreach (var obj in state.QuestObjectives)
                {
                    obj.Parent = state;
                    obj._objectiveIndex = j;

                    j++;
                }

                i++;
            }
        }
    }

    [XmlType(TypeName = "QST")]
    public class QuestState
    {
        [XmlElement("QSo")]
        public List<QuestObjective> QuestObjectives = new List<QuestObjective>();

        [XmlElement("QSt")]
        public string DescriptiveName = "";

        [XmlElement("QSte")]
        public TextInfo TextInfo = new TextInfo();

        [XmlIgnore]
        public Quest Parent;

        [XmlIgnore]
        public int _stateIndex;

        [XmlElement("QSps")]
        public Vector2 _position = new Vector2();

        public QuestState() { }

        public bool IsStateCompleted()
        {
            int count = 0;
            foreach (var obj in QuestObjectives)
            {
                if (!obj.IsCompleted())
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
        public string DescriptiveName;

        [XmlElement("QOte")]
        public TextInfo TextInfo = new TextInfo();

        [XmlIgnore]
        public QuestState Parent;

        [XmlIgnore]
        public int _objectiveIndex;

        [XmlElement("QOps")]
        public Vector2 _position = new Vector2();

        public QuestObjective() { }

        public bool IsCompleted()
        {
            return QuestLedger.GetStateValue(Parent.Parent.ID, (int)QuestStates.State0 + (int)QuestStates.StateOffset * Parent._stateIndex + _objectiveIndex + 1);
        }
    }
}
