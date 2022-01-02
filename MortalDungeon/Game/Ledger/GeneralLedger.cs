using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Game.Ledger;
using MortalDungeon.Game.Save;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using MortalDungeon.Game.Serializers;

namespace MortalDungeon.Game
{

    [XmlType(TypeName = "GenLedg")]
    public static class GeneralLedger
    {
        [XmlIgnore]
        public static Dictionary<long, GeneralLedgerNode> LedgeredGeneralState = new Dictionary<long, GeneralLedgerNode>();


        /// <summary>
        /// 
        /// </summary>
        public static int GetInteraction(long featureID, long objectHash)
        {
            if (LedgeredGeneralState.TryGetValue(featureID, out var n))
            {
                if (n.StateValues.TryGetValue(objectHash, out var i))
                {
                    return i;
                }
            }

            return 0;
        }

        public static void SetStateValue(StateIDValuePair stateValue)
        {
            if (stateValue.Data < 0)
            {
                return; //put instructions here
            }

            if (LedgeredGeneralState.TryGetValue(stateValue.StateID, out var n))
            {
                n.SetStateValue(stateValue.ObjectHash, stateValue.Data);
            }
            else
            {
                GeneralLedgerNode node = new GeneralLedgerNode() { ID = stateValue.StateID };
                LedgeredGeneralState.Add(stateValue.StateID, node);

                node.SetStateValue(stateValue.ObjectHash, stateValue.Data);
            }
        }

        public static void RemoveStateValue(StateIDValuePair stateValue)
        {
            if (LedgeredGeneralState.TryGetValue(stateValue.StateID, out var n))
            {
                n.RemoveStateValue(stateValue.ObjectHash, stateValue.Data);
            }
        }


        public static int GetStateValue(long featureID, long objectHash)
        {
            if (LedgeredGeneralState.TryGetValue(featureID, out var n))
            {
                return n.GetStateValue(objectHash);
            }

            return 0;
        }
    }

    [XmlType(TypeName = "GenLN")]
    [Serializable]
    public class GeneralLedgerNode
    {
        public long ID;

        /// <summary>
        /// The object hash is the long component and the data is the int component (from the StateIdValuePairs)
        /// </summary>
        [XmlIgnore]
        public Dictionary<long, int> StateValues = new Dictionary<long, int>();

        [XmlElement("Glns", Namespace = "GenLN")]
        public DeserializableDictionary<long, int> _stateValues = new DeserializableDictionary<long, int>();

        public void IncrementStateValue(long objHash)
        {
            int val = 0;

            if (StateValues.TryGetValue((long)objHash, out var a))
            {
                val = a + 1;
                StateValues[(long)objHash] = (short)(val);
            }
            else
            {
                val = 1;
                StateValues.Add((long)objHash, (short)val);
            }

            StateIDValuePair updatedState = new StateIDValuePair()
            {
                Type = (int)LedgerUpdateType.Feature,
                StateID = ID,
                ObjectHash = (long)objHash,
                Data = val,
            };

            Ledgers.LedgerUpdated(updatedState);
        }

        public void SetStateValue(long objHash, int value)
        {
            if (StateValues.TryGetValue(objHash, out var a))
            {
                StateValues[objHash] = value;
            }
            else
            {
                StateValues.Add(objHash, value);
            }

            StateIDValuePair updatedState = new StateIDValuePair()
            {
                Type = (int)LedgerUpdateType.GeneralState,
                StateID = ID,
                ObjectHash = objHash,
                Data = value,
            };

            Ledgers.LedgerUpdated(updatedState);
        }

        public void RemoveStateValue(long objHash, int value)
        {
            if (StateValues.TryGetValue(objHash, out var a))
            {
                StateValues.Remove(value);
            }
        }

        public int GetStateValue(long objHash)
        {
            if (StateValues.TryGetValue(objHash, out var a))
            {
                return a;
            }
            else
            {
                return 0;
            }
        }
    }
}
