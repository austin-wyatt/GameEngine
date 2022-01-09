using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Game.Ledger;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using MortalDungeon.Game.Serializers;
using MortalDungeon.Engine_Classes;

namespace MortalDungeon.Game
{
    /// <summary>
    /// Feature interactions that hook into the interaction functionality will specifically be negative values.
    /// </summary>
    public enum FeatureInteraction
    {
        Refreshed = -8,
        Cleared = -7,
        Killed = -6,
        Revived = -5,
        Explored = -4,
        KilledBoss = -3,
        KilledSummon = -2,
        Entered = -1,
        None = 0,
    }

    public static class FeatureLedger
    {
        public static Dictionary<long, FeatureLedgerNode> LedgeredFeatures = new Dictionary<long,FeatureLedgerNode>();

        public static void AddInteraction(StateIDValuePair state)
        {
            FeatureLedgerNode node;

            if (LedgeredFeatures.TryGetValue(state.StateID, out var n))
            {
                node = n;
            }
            else
            {
                node = new FeatureLedgerNode() { ID = state.StateID };
                LedgeredFeatures.Add(state.StateID, node);
            }

            switch ((FeatureInteraction)state.Data)
            {
                case FeatureInteraction.Refreshed:
                    node.SetStateValue(FeatureStateValues.AvailableToClear, 1);
                    node.SetStateValue(FeatureStateValues.TimeCleared, 0);
                    break;
                case FeatureInteraction.Cleared:
                    node.SetStateValue(FeatureStateValues.Cleared, 1);
                    node.SetStateValue(FeatureStateValues.Explored, 1);
                    node.SetStateValue(FeatureStateValues.AvailableToClear, 0);
                    node.SetStateValue(FeatureStateValues.TimeCleared, (short)CombatScene.Days);
                    break;
                case FeatureInteraction.Explored:
                    node.SetStateValue(FeatureStateValues.Explored, 1);
                    break;
                case FeatureInteraction.Entered:
                    //add text to the event log about the area you are in.
                    //To track whether the player is in a feature we can check all loaded features when the player moves
                    //for an affected point that matches the tile that was moved to. Features that the player is in 
                    //could then be stored in a hashset so they aren't being spammed with a message every time 
                    //they move. When a feature is entered it should have its "Discovered" state value set to 1.
                    //(it doesn't matter if this gets set multiple times really)
                    //(remove this entered interaction)
                    //
                    //This won't be that simple actually. One way to do it would be to calculate feature bounds using some 
                    //algorithm when all the affected points are generated and then run a 2D bounds check against those bounds for any
                    //points we care about checking. 
                    break;
                case FeatureInteraction.Killed:
                    if (node.SignificantInteractions.TryGetValue(state.ObjectHash, out var a))
                    {
                        node.SignificantInteractions[state.ObjectHash] = (short)state.Data;
                    }
                    else
                    {
                        node.SignificantInteractions.Add(state.ObjectHash, (short)state.Data);
                    }

                    node.IncrementStateValue(FeatureStateValues.NormalKillCount);
                    break;
                case FeatureInteraction.Revived:
                    if (node.SignificantInteractions.TryGetValue(state.ObjectHash, out var i))
                    {
                        if (i == (short)FeatureInteraction.Killed)
                        {
                            node.SignificantInteractions.Remove(state.ObjectHash);
                        }
                    }
                    break;
            }

            //node.CheckNodeCleared();

            if(state.Data != (int)FeatureInteraction.None)
            {
                StateIDValuePair updatedState = new StateIDValuePair() 
                {
                    Type = (int)LedgerUpdateType.Feature,
                    StateID = state.StateID,
                    ObjectHash = state.ObjectHash,
                    Data = state.Data,
                };

                
                Ledgers.LedgerUpdated(updatedState);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static FeatureInteraction GetInteraction(long featureID, long objectHash)
        {
            if(LedgeredFeatures.TryGetValue(featureID, out var n))
            {
                if(n.SignificantInteractions.TryGetValue(objectHash, out var i))
                {
                    return (FeatureInteraction)i;
                }
            }

            return FeatureInteraction.None;
        }

        public static void SetFeatureStateValue(StateIDValuePair stateValue)
        {
            if(stateValue.Data < 0)
            {
                AddInteraction(stateValue);
                return; //A data of less than 0 will mean that an interaction has occured and the value will be handled there
            }

            if (LedgeredFeatures.TryGetValue(stateValue.StateID, out var n))
            {
                n.SetStateValue((FeatureStateValues)stateValue.ObjectHash, (short)stateValue.Data);
            }
            else
            {
                FeatureLedgerNode node = new FeatureLedgerNode() { ID = stateValue.StateID };
                LedgeredFeatures.Add(stateValue.StateID, node);

                node.SetStateValue((FeatureStateValues)stateValue.ObjectHash, (short)stateValue.Data);
            }
        }

        public static void SetFeatureStateValue(long stateId, FeatureStateValues stateValue, int data)
        {
            if (LedgeredFeatures.TryGetValue(stateId, out var n))
            {
                n.SetStateValue(stateValue, (short)data);
            }
            else
            {
                FeatureLedgerNode node = new FeatureLedgerNode() { ID = stateId };
                LedgeredFeatures.Add(stateId, node);

                node.SetStateValue(stateValue, (short)data);
            }
        }

        public static void IncrementStateValue(long stateId, FeatureStateValues stateValue)
        {
            if (LedgeredFeatures.TryGetValue(stateId, out var n))
            {
                n.IncrementStateValue(stateValue);
            }
            else
            {
                FeatureLedgerNode node = new FeatureLedgerNode() { ID = stateId };
                LedgeredFeatures.Add(stateId, node);

                n.IncrementStateValue(stateValue);
            }
        }

        public static void DecrementStateValue(long stateId, FeatureStateValues stateValue)
        {
            if (LedgeredFeatures.TryGetValue(stateId, out var n))
            {
                n.DecrementStateValue(stateValue);
            }
            else
            {
                FeatureLedgerNode node = new FeatureLedgerNode() { ID = stateId };
                LedgeredFeatures.Add(stateId, node);

                node.DecrementStateValue(stateValue);
            }
        }

        public static void RemoveFeatureStateValue(StateIDValuePair stateValue)
        {
            if (LedgeredFeatures.TryGetValue(stateValue.StateID, out var n))
            {
                n.RemoveStateValue((FeatureStateValues)stateValue.ObjectHash, (short)stateValue.Data);
            }
        }


        public static short GetFeatureStateValue(long featureID, FeatureStateValues objectHash)
        {
            if (LedgeredFeatures.TryGetValue(featureID, out var n))
            {
                return n.GetStateValue(objectHash);
            }

            return 0;
        }


        public static string GetHashData(long featureId, long objectHash, string property)
        {
            if (LedgeredFeatures.TryGetValue(featureId, out var n))
            {
                if(n.HashData.TryGetValue(objectHash, out var data))
                {
                    return data.LazyGet(property);
                }
            }

            return "";
        }

        public static void AddHashData(long featureId, long objectHash, string key, string property)
        {
            FeatureLedgerNode node;

            LedgeredFeatures.GetOrAdd(featureId, out node);
            node.ID = featureId;

            node.HashData.GetOrAdd(objectHash, out var data);
            data.AddOrSet(key, property);

            //check if feature is cleared here
            
            if(FeatureManager.LoadedFeatures.TryGetValue(featureId, out var eq))
            {
                if (eq.CheckCleared())
                {
                    SetFeatureStateValue(featureId, FeatureStateValues.Cleared, 1);
                }
            }
        }

        public static HashBoolean GetHashBoolean(long featureId, long objectHash, string property)
        {
            string value = GetHashData(featureId, objectHash, property);

            switch (value)
            {
                case "t":
                    return HashBoolean.True;
                case "f":
                    return HashBoolean.False;
                default:
                    return HashBoolean.NotSet;
            }
        }

        public static void SetHashBoolean(long featureId, long objectHash, string key, bool property)
        {
            AddHashData(featureId, objectHash, key, property ? "t" : "f");
        }
    }

    public enum HashBoolean
    {
        True,
        False,
        NotSet
    }

    /// <summary>
    /// Pass these values as the objectHash when setting or recieving interaction data
    /// </summary>
    public enum FeatureStateValues : long
    {
        Cleared = long.MaxValue,
        AvailableToClear = long.MaxValue - 1,
        NormalKillRequirements = long.MaxValue - 2,
        BossKillRequirements = long.MaxValue - 3,
        LootInteractionRequirements = long.MaxValue - 4,
        SpecialInteractionRequirements = long.MaxValue - 5,

        Discovered = long.MaxValue - 25,
        Explored = long.MaxValue - 26,
        TimeCleared = long.MaxValue - 27,

        NormalKillCount = long.MaxValue - 50,
        BossKillCount = long.MaxValue - 51,
        LootInteractionCount = long.MaxValue - 52,
        SpecialInteractionCount = long.MaxValue - 53,

        SpecifyFeatureInteraction = long.MaxValue - 100,

        PlayerInside = long.MaxValue - 200,
        PlayerInsideCounter = long.MaxValue - 201,

        FeatureLoaded = long.MaxValue - 500
    }

    public class FeatureLedgerNode
    {
        public long ID;

        /// <summary>
        /// The key will represent the hash of point that the interactable object was on. 
        /// For example, if a skeleton was spawned on point (0, 1) and then killed then 
        /// the hash of (0, 1) and a short casted from the FeatureInteraction enum will be stored <para/>
        /// SignificantInteractions will also encode state data for the feature
        /// </summary>
        public Dictionary<long, short> SignificantInteractions = new Dictionary<long, short>();

        public Dictionary<long, Dictionary<string, string>> HashData = new Dictionary<long, Dictionary<string, string>>();

        public void IncrementStateValue(FeatureStateValues objHash)
        {
            int val = 0;

            if (SignificantInteractions.TryGetValue((long)objHash, out var a))
            {
                val = a + 1;
                SignificantInteractions[(long)objHash] = (short)(val);
            }
            else
            {
                val = 1;
                SignificantInteractions.Add((long)objHash, (short)val);
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

        public void DecrementStateValue(FeatureStateValues objHash)
        {
            int val = 0;

            if (SignificantInteractions.TryGetValue((long)objHash, out var a))
            {
                val = a - 1;
                SignificantInteractions[(long)objHash] = (short)(val);
            }
            else
            {
                val = -1;
                SignificantInteractions.Add((long)objHash, (short)val);
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

        public void SetStateValue(FeatureStateValues objHash, short value)
        {
            if (SignificantInteractions.TryGetValue((long)objHash, out var a))
            {
                SignificantInteractions[(long)objHash] = value;
            }
            else
            {
                SignificantInteractions.Add((long)objHash, value);
            }


            StateIDValuePair updatedState = new StateIDValuePair()
            {
                Type = (int)LedgerUpdateType.Feature,
                StateID = ID,
                ObjectHash = (long)objHash,
                Data = value,
            };

            Ledgers.LedgerUpdated(updatedState);
        }

        public void RemoveStateValue(FeatureStateValues objHash, short value)
        {
            if (SignificantInteractions.TryGetValue((long)objHash, out var a))
            {
                SignificantInteractions.Remove(value);
            }
        }

        public short GetStateValue(FeatureStateValues objHash)
        {
            if (SignificantInteractions.TryGetValue((long)objHash, out var a))
            {
                return a;
            }
            else
            {
                return 0;
            }
        }

        //public void CheckNodeCleared()
        //{
        //    short requiredNormal = GetStateValue(FeatureStateValues.NormalKillRequirements);
        //    short requiredBoss = GetStateValue(FeatureStateValues.BossKillRequirements);
        //    short requiredLoot = GetStateValue(FeatureStateValues.LootInteractionRequirements);
        //    short requiredInteraction = GetStateValue(FeatureStateValues.SpecialInteractionRequirements);

        //    short countNormal = GetStateValue(FeatureStateValues.NormalKillCount);
        //    short countBoss = GetStateValue(FeatureStateValues.BossKillCount);
        //    short countLoot = GetStateValue(FeatureStateValues.LootInteractionCount);
        //    short countInteraction = GetStateValue(FeatureStateValues.SpecialInteractionCount);

        //    if(countNormal >= requiredNormal &&
        //       countBoss >= requiredBoss &&
        //       countLoot >= requiredLoot &&
        //       countInteraction >= requiredInteraction)
        //    {
        //        SetStateValue(FeatureStateValues.Cleared, 1);
        //    }
        //}
    }
}
