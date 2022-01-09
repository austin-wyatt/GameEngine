using MortalDungeon.Game.Ledger;
using MortalDungeon.Game.LuaHandling;
using MortalDungeon.Game.Map;
using MortalDungeon.Game.Units;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MortalDungeon.Game.Serializers
{
    /// <summary>
    /// Stores all of the possible feature origin points and load radii in memory. <para/>
    /// When maps are loaded this list will be checked. If any of the points are within 
    /// the load radius then that feature will be added to the tile map controller's loaded features. <para/>
    /// Similarly, if a loaded feature is outside of the load radius then it will be unloaded.
    /// </summary>
    public static class FeatureManager
    {
        public static FeatureList AllFeatures = new FeatureList();

        public static Dictionary<long, FeatureEquation> LoadedFeatures = new Dictionary<long, FeatureEquation>();

        public static HashSet<FeatureEquation> SubscribedBounds = new HashSet<FeatureEquation>();

        private static object _featureLock = new object();

        public static void Initialize()
        {
            LoadedFeatures = new Dictionary<long, FeatureEquation>();
            AllFeatures = FeatureSerializer.LoadFeatureListFile();
        }
        //when loading a feature, apply any passed state values as well.
        public static void EvaluateLoadedFeatures(FeaturePoint currPosition, int layer)
        {
            List<long> featuresToRemove = new List<long>();
            lock (_featureLock)
            {
                foreach (var eq in LoadedFeatures)
                {
                    int loadRadius = eq.Value.LoadRadius < 150 ? 150 : eq.Value.LoadRadius;

                    loadRadius += 200; //we don't want to unload the feature as soon as we leave the load radius since that could
                                       //cause some issues if the user walks back and forth between 2 maps repeatedly.

                    if (layer != eq.Value.Layer || !CheckDistance(eq.Value.Origin, currPosition, loadRadius))
                    {
                        featuresToRemove.Add(eq.Value.FeatureID);
                    }
                }

                foreach (var eq in featuresToRemove)
                {
                    SubscribedBounds.Remove(LoadedFeatures[eq]);

                    FeatureLedger.SetFeatureStateValue(LoadedFeatures[eq].FeatureID, FeatureStateValues.FeatureLoaded, 0);

                    LoadedFeatures.Remove(eq);
                }


                for (int i = 0; i < AllFeatures.Features.Count; i++)
                {
                    if (LoadedFeatures.ContainsKey(AllFeatures.Features[i].Id))
                    {
                        continue;
                    }

                    int loadRadius = AllFeatures.Features[i].LoadRadius < 150 ? 150 : AllFeatures.Features[i].LoadRadius;

                    if (layer == AllFeatures.Features[i].Layer && CheckDistance(AllFeatures.Features[i].Origin, currPosition, loadRadius))
                    {
                        var feature = FeatureSerializer.LoadFeatureFromFile(AllFeatures.Features[i].Id);

                        if (feature != null)
                        {
                            var equation = feature.CreateFeatureEquation();

                            if (equation.Instructions.Count > 0)
                            {
                                Ledgers.EvaluateInstructions(equation.Instructions);
                            }

                            LoadedFeatures.Add(AllFeatures.Features[i].Id, equation);

                            FeatureLedger.SetFeatureStateValue(equation.FeatureID, FeatureStateValues.FeatureLoaded, 1);

                            foreach (var bound in equation.BoundingPoints)
                            {
                                if (bound.SubscribeToEntrance)
                                {
                                    SubscribedBounds.Add(equation);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }


        public static bool CheckDistance(FeaturePoint start, FeaturePoint end, int targetDistance) 
        {
            return ((start.X - end.X) * (start.X - end.X) + (start.Y - end.Y) * (start.Y - end.Y)) < targetDistance * targetDistance;
        }

        public delegate void FeatureMovementEventHandler(FeatureEquation feature, Unit unit);
        public static FeatureMovementEventHandler FeatureEnter;
        public static FeatureMovementEventHandler FeatureExit;


        public static void OnUnitMoved(Unit unit)
        {
            var unitPos = FeatureEquation.PointToMapCoords(unit.Info.TileMapPosition);

            lock (_featureLock)
            {
                foreach (var eq in SubscribedBounds)
                {
                    foreach (var bound in eq.BoundingPoints)
                    {
                        if (!bound.SubscribeToEntrance)
                            continue;

                        if (bound.Parameters.TryGetValue("type", out string type))
                        {
                            //evaluate different types of bounds checks here.
                            //the default case is checking for a player unit passing a feature threshold
                            continue;
                        }

                        //first check if the unit is controlled by the player and the player is not in combat
                        if (unit.AI.ControlType == ControlType.Controlled && !unit.Scene.InCombat)
                        {
                            bool unitIsInside = FeaturePoint.PointInPolygon(bound.OffsetPoints, unitPos);

                            int testValue = 3;

                            short val = FeatureLedger.GetFeatureStateValue(eq.FeatureID, FeatureStateValues.PlayerInsideCounter);

                            if (unitIsInside && val < testValue)
                            {
                                FeatureLedger.IncrementStateValue(eq.FeatureID, FeatureStateValues.PlayerInsideCounter);
                            }
                            else if (!unitIsInside && val > -testValue)
                            {
                                FeatureLedger.DecrementStateValue(eq.FeatureID, FeatureStateValues.PlayerInsideCounter);
                            }

                            bool alreadyInside = FeatureLedger.GetFeatureStateValue(eq.FeatureID, FeatureStateValues.PlayerInside) == 1;

                            if(val >= testValue && !alreadyInside)
                            {
                                FeatureEnter?.Invoke(eq, unit);

                                FeatureLedger.SetFeatureStateValue(eq.FeatureID, FeatureStateValues.PlayerInside, 1);
                                FeatureLedger.SetFeatureStateValue(eq.FeatureID, FeatureStateValues.Explored, 1);

                                if(bound.Parameters.TryGetValue("enter_script", out var script))
                                {
                                    LuaManager.ApplyScript(script);
                                }
                            }
                            else if(val <= -testValue && alreadyInside)
                            {
                                FeatureExit?.Invoke(eq, unit);

                                FeatureLedger.SetFeatureStateValue(eq.FeatureID, FeatureStateValues.PlayerInside, 0);

                                if (bound.Parameters.TryGetValue("exit_script", out var script))
                                {
                                    LuaManager.ApplyScript(script);
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void OnUnitKilled(Unit unit)
        {
            FeatureLedger.SetHashBoolean(unit.FeatureID, unit.ObjectHash, "alive", false);
        }
    }
}
