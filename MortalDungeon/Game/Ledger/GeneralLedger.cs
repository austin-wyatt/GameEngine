using Empyrean.Engine_Classes.Scenes;
using Empyrean.Game.Ledger;
using Empyrean.Game.Save;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using Empyrean.Game.Serializers;
using System.Numerics;

namespace Empyrean.Game
{

    [XmlType(TypeName = "GenLedg")]
    public static class GeneralLedger
    {
        [XmlIgnore]
        public static Dictionary<BigInteger, GeneralLedgerNode> LedgeredGeneralState = new Dictionary<BigInteger, GeneralLedgerNode>();

        /// <summary>
        /// 
        /// </summary>
        public static int GetInteraction(BigInteger featureID, BigInteger objectHash)
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

        public static void SetStateValue(BigInteger id, BigInteger hash, int data)
        {
            if (data < 0)
            {
                return; //put instructions here
            }

            if (LedgeredGeneralState.TryGetValue(id, out var n))
            {
                n.SetStateValue(hash, data);
            }
            else
            {
                GeneralLedgerNode node = new GeneralLedgerNode() { ID = id };
                LedgeredGeneralState.Add(id, node);

                node.SetStateValue(hash, data);
            }
        }

        public static void RemoveStateValue(StateIDValuePair stateValue)
        {
            if (LedgeredGeneralState.TryGetValue(stateValue.StateID, out var n))
            {
                n.RemoveStateValue(stateValue.ObjectHash, stateValue.Data);
            }
        }


        public static int GetStateValue(BigInteger featureID, BigInteger objectHash)
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
        public BigInteger ID;

        /// <summary>
        /// The object hash is the BigInteger component and the data is the int component (from the StateIdValuePairs)
        /// </summary>
        [XmlIgnore]
        public Dictionary<BigInteger, int> StateValues = new Dictionary<BigInteger, int>();

        [XmlElement("Glns", Namespace = "GenLN")]
        public DeserializableDictionary<BigInteger, int> _stateValues = new DeserializableDictionary<BigInteger, int>();

        public void IncrementStateValue(BigInteger objHash)
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
                Type = (int)LedgerUpdateType.GeneralState,
                StateID = ID,
                ObjectHash = (long)objHash,
                Data = val,
            };

            Ledgers.LedgerUpdated(updatedState);
        }

        public void SetStateValue(BigInteger objHash, int value)
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

        public void RemoveStateValue(BigInteger objHash, int value)
        {
            if (StateValues.TryGetValue(objHash, out var a))
            {
                StateValues.Remove(value);
            }
        }

        public int GetStateValue(BigInteger objHash)
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
