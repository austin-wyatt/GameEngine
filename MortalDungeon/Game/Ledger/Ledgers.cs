using MortalDungeon.Game.Units;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using MortalDungeon.Game.Serializers;

namespace MortalDungeon.Game.Ledger
{
    public enum LedgerUpdateType
    {
        Dialogue,
        Feature,
        Quest,
        GeneralState,
        Unit
    }

    public static class Ledgers
    {
        public static List<StateIDValuePair> StateSubscribers = new List<StateIDValuePair>();

        public static void LedgerUpdated(StateIDValuePair stateValue)
        {
            List<StateIDValuePair> currentSubscribers = new List<StateIDValuePair>(StateSubscribers);

            for(int i = currentSubscribers.Count - 1; i >= 0; i--)
            {
                if(currentSubscribers[i].Type == stateValue.Type && currentSubscribers[i].StateID == stateValue.StateID
                    && currentSubscribers[i].ObjectHash == stateValue.ObjectHash && stateValue.Data == currentSubscribers[i].Data)
                {
                    if(currentSubscribers[i].Values.Count > 0)
                    {
                        ApplyStateValues(currentSubscribers[i].Values);
                    }

                    if(currentSubscribers[i].Instruction != (short)StateInstructions.PermanentSubscriber)
                    {
                        currentSubscribers.RemoveAt(i);
                    }
                }
            }

            for (int i = QuestManager.Quests.Count - 1; i >= 0; i--)
            {
                QuestManager.Quests[i].CheckObjectives();
            }
        }

        public static void OnUnitKilled(Unit unit)
        {
            //update the feature ledger stating that this unit has died
            if (unit.FeatureID != 0)
            {
                StateIDValuePair killUnitState = new StateIDValuePair()
                {
                    Type = (int)LedgerUpdateType.Feature,
                    StateID = unit.FeatureID,
                    ObjectHash = unit.ObjectHash,
                    Data = (int)FeatureInteraction.Killed,
                };

                SetStateValue(killUnitState);
            }
        }

        public static void OnUnitRevived(Unit unit)
        {
            //update the feature ledger stating that this unit has been revived
            if (unit.FeatureID != 0)
            {
                StateIDValuePair killUnitState = new StateIDValuePair()
                {
                    Type = (int)LedgerUpdateType.Feature,
                    StateID = unit.FeatureID,
                    ObjectHash = unit.ObjectHash,
                    Data = (int)FeatureInteraction.Revived,
                };

                SetStateValue(killUnitState);
            }
        }

        public static void SetStateValue(StateIDValuePair val)
        {
            if(val.Instruction == (int)StateInstructions.Subscribe || val.Instruction == (int)StateInstructions.PermanentSubscriber)
            {
                StateSubscribers.Add(val);
            }
            else if(val.Instruction == (int)StateInstructions.Set)
            {
                switch (val.Type)
                {
                    case (int)LedgerUpdateType.Dialogue:
                        DialogueLedger.SetStateValue(val);
                        break;
                    case (int)LedgerUpdateType.Quest:
                        QuestLedger.ModifyStateValue(val);
                        break;
                    case (int)LedgerUpdateType.Feature:
                        FeatureLedger.SetFeatureStateValue(val);
                        break;
                    case (int)LedgerUpdateType.GeneralState:
                        GeneralLedger.SetStateValue(val);
                        break;
                }
            }
            else if(val.Instruction == (int)StateInstructions.Clear)
            {
                switch (val.Type)
                {
                    case (int)LedgerUpdateType.Dialogue:
                        DialogueLedger.RemoveStateValue(val);
                        break;
                    case (int)LedgerUpdateType.Quest:
                        QuestLedger.ModifyStateValue(val);
                        break;
                    case (int)LedgerUpdateType.Feature:
                        FeatureLedger.RemoveFeatureStateValue(val);
                        break;
                    case (int)LedgerUpdateType.GeneralState:
                        GeneralLedger.RemoveStateValue(val);
                        break;
                }
            }
        }

        public static void ApplyStateValues(List<StateIDValuePair> data)
        {
            foreach (StateIDValuePair val in data)
            {
                SetStateValue(val);
            }
        }
    }

    public enum StateInstructions
    {
        Set,                 //Default, sets the state value to the passed in data

        Subscribe,           //Subscribes to changes in that state value.
                             //If the Values list has data then these will
                             //be evaluated once the subscribed state is hit

        Clear,               //Removes any occurrence of this state value from the
                             //targeted state]

        PermanentSubscriber, //Subscribes to a state value in exactly the same way as 
                             //the subscribe instruction but is not removed when triggered

    }
}
