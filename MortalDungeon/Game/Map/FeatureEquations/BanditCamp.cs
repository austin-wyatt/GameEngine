using MortalDungeon.Engine_Classes;
using MortalDungeon.Game.Entities;
using MortalDungeon.Game.Ledger;
using MortalDungeon.Game.Structures;
using MortalDungeon.Game.Tiles;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MortalDungeon.Game.Serializers;

namespace MortalDungeon.Game.Map.FeatureEquations
{
    public class BanditCamp : FeatureEquation
    {
        private enum CampFeatures
        {
            Ground = 1,
            Tent = 2,
            Enemy = 4,
            MustExplore = 8,
        }

        private BanditCampParams CampParams;
        private Random NumberGen;

        private List<BuildingSkeleton> TentSkeletons = new List<BuildingSkeleton>();

        public BanditCamp(BanditCampParams @params)
        {
            CampParams = @params;
            NumberGen = new ConsistentRandom((int)HashCoordinates(@params.Origin.X, @params.Origin.Y));

            FeatureID = HashCoordinates(@params.Origin.X, @params.Origin.Y);

            StateIDValuePair killRequirementState = new StateIDValuePair() 
            {
                Type = (int)LedgerUpdateType.Feature,
                StateID = FeatureID,
                ObjectHash = (long)FeatureStateValues.NormalKillRequirements,
                Data = 3
            };

            Ledgers.ApplyStateValue(killRequirementState);

            StateIDValuePair availableToClearState = new StateIDValuePair()
            {
                Type = (int)LedgerUpdateType.Feature,
                StateID = FeatureID,
                ObjectHash = (long)FeatureStateValues.AvailableToClear,
                Data = 1
            };

            Ledgers.ApplyStateValue(availableToClearState);
        }

        public override void ApplyToTile(BaseTile tile, bool freshGeneration = true)
        {
            FeaturePoint affectedPoint = new FeaturePoint(PointToMapCoords(tile.TilePoint));

            if (AffectedPoints.TryGetValue(affectedPoint, out int value))
            {
                if (GetBit(value, 0) && freshGeneration)
                {
                    if (tile.Structure != null && tile.Structure.Type != StructureEnum.Tent)
                        tile.RemoveStructure(tile.Structure);

                    tile.Properties.Type = TileType.Dead_Grass;
                    tile.Update();
                }
                //else if (GetBit(value, 1))
                //{
                //    BuildingSkeleton tentSkeleton = TentSkeletons.Find(t => t.TilePattern.Contains(affectedPoint));

                //    if (tentSkeleton != null && tentSkeleton.Loaded == false)
                //    {
                //        tentSkeleton.Loaded = true;
                //        tentSkeleton._skeletonTouchedThisCycle = true;

                //        Tent tent = new Tent(tile.GetScene());

                //        tent.SkeletonReference = tentSkeleton;
                //        tentSkeleton.Handle = tent;

                //        tent.RotateTilePattern(tentSkeleton.Rotations);

                //        tent.InitializeVisualComponent();

                //        Vector3 tileSize = tile.GetDimensions();
                //        Vector3 posDiff = new Vector3();

                //        if (affectedPoint != tentSkeleton.IdealCenter)
                //        {
                //            posDiff.X = tileSize.X * (tentSkeleton.IdealCenter.X - affectedPoint.X);
                //            posDiff.Y = tileSize.Y * (tentSkeleton.IdealCenter.Y - affectedPoint.Y);

                            

                //            if(affectedPoint.X % 2 == 0) 
                //            {
                //                posDiff.Y -= tileSize.Y / 2;
                //            }
                //            if (affectedPoint.X % 2 == 1)
                //            {
                //                posDiff.Y += tileSize.Y / 2;
                //            }

                //            if (affectedPoint.X < tentSkeleton.IdealCenter.X)
                //            {
                //                posDiff.X -= tileSize.X / 4;
                //            }
                //            else
                //            {
                //                posDiff.X += tileSize.X / 4;
                //            }
                //        }

                //        tent.SetPosition(tile.Position + posDiff + new Vector3(0, 0, 0.2f));

                //        tent.SetTileMapPosition(tile);
                //    }
                //    else if (tentSkeleton != null && tentSkeleton.Handle != null && tentSkeleton.Loaded && !tentSkeleton._skeletonTouchedThisCycle) 
                //    {
                //        tentSkeleton._skeletonTouchedThisCycle = true;

                //        tentSkeleton.Handle.TileAction();
                //        //tentSkeleton.Handle.SetColor(new Vector4(1, 0, 0, 1));
                //    }
                //}


                long pointHash = affectedPoint.GetUniqueHash();

                var interaction = FeatureLedger.GetInteraction(FeatureID, pointHash);
                short featureStateVal = FeatureLedger.GetFeatureStateValue(FeatureID, FeatureStateValues.AvailableToClear);

                if (GetBit(value, 2) && freshGeneration && !tile.GetScene()._units.Exists(u => u.FeatureID == FeatureID && u.ObjectHash == pointHash) 
                    && interaction != FeatureInteraction.Killed && featureStateVal > 0) 
                {
                    Entity enemy = new Entity(EntityParser.ApplyPrefabToUnit(EntityParser.FindPrefab(PrefabType.Unit, "Grave Skele"), tile.GetScene()));
                    EntityManager.AddEntity(enemy);

                    enemy.Handle.pack_name = "bandit camp" + FeatureID;

                    enemy.Handle.FeatureID = FeatureID;
                    enemy.Handle.ObjectHash = pointHash;

                    enemy.DestroyOnUnload = true;

                    enemy.Load(affectedPoint);
                }


                if (GetBit(value, 3) && freshGeneration && interaction != FeatureInteraction.Cleared && interaction != FeatureInteraction.Explored)
                {
                    tile.Properties.MustExplore = true;
                    tile.Update();
                }
            }
        }

        public override void GenerateFeature()
        {
            ClearAffectedPoints();

            FeaturePoint startPoint = CampParams.Origin;

            AddAffectedPoint(startPoint, (int)CampFeatures.Ground);

            List<FeaturePoint> tentLocations = new List<FeaturePoint>();
            List<int> tentRotations = new List<int>();

            tentLocations.Add(new FeaturePoint(startPoint.X, startPoint.Y));
            tentRotations.Add(0);


            tentLocations.Add(new FeaturePoint(startPoint.X + 6, startPoint.Y - 3));
            tentRotations.Add(1);

            tentLocations.Add(new FeaturePoint(startPoint.X + 6, startPoint.Y + 3));
            tentRotations.Add(2);

            for (int i = 0; i < tentLocations.Count; i++)
            {
                Tent tent1 = new Tent();

                tent1.RotateTilePattern(tentRotations[i]);

                List<FeaturePoint> tentPoints = tent1.GetPatternFeaturePoints(tentLocations[i]);

                foreach (FeaturePoint point in tentPoints)
                {
                    AddAffectedPoint(point, (int)CampFeatures.Tent);
                }

                //TentSkeletons.Add(new BuildingSkeleton() { IdealCenter = tentLocations[i], TilePattern = tentPoints.ToHashSet(), Rotations = tent1.Rotations });
            }

            int count = 0;
            for(int i = 0; i <= 2; i++) 
            {
                List<FeaturePoint> points = new List<FeaturePoint>();

                if (i == 0)
                    points.Add(new FeaturePoint(startPoint.X + 4, startPoint.Y));
                else
                    GetRingOfTiles(new FeaturePoint(startPoint.X + 4, startPoint.Y), points, i);

                foreach (FeaturePoint point in points)
                {
                    if (AffectedPoints.TryGetValue(point, out int val))
                    {
                        AffectedPoints[point] = val | (int)CampFeatures.Ground;

                        if (i == 2) 
                        {
                            if ((count + 1) % 4 == 0) 
                            {
                                AffectedPoints[point] = val | (int)CampFeatures.Enemy;
                            }
                            count++;
                        }

                        AffectedMaps.Add(FeaturePointToTileMapCoords(point));
                    }
                    else
                    {
                        AddAffectedPoint(point, (int)CampFeatures.Ground);

                        if (i == 2)
                        {
                            if ((count + 1) % 4 == 0)
                            {
                                AffectedPoints[point] = (int)CampFeatures.Ground | (int)CampFeatures.Enemy;
                            }
                            count++;
                        }
                    }
                }
            }

            for (int i = 0; i <= 6; i++)
            {
                List<FeaturePoint> points = new List<FeaturePoint>();

                if (i == 0)
                    points.Add(new FeaturePoint(startPoint.X + 4, startPoint.Y));
                else
                    GetRingOfTiles(new FeaturePoint(startPoint.X + 4, startPoint.Y), points, i);

                foreach (FeaturePoint point in points)
                {
                    if (AffectedPoints.TryGetValue(point, out int val))
                    {
                        AffectedPoints[point] = val | (int)CampFeatures.MustExplore;

                        AffectedMaps.Add(FeaturePointToTileMapCoords(point));
                    }
                    else
                    {
                        AddAffectedPoint(point, (int)CampFeatures.MustExplore);
                    }
                }
            }
        }

        public override void OnAppliedToMaps()
        {
            base.OnAppliedToMaps();

            foreach (var tentSkeletons in TentSkeletons) 
            {
                tentSkeletons._skeletonTouchedThisCycle = false;
            }
        }
    }


    public struct BanditCampParams
    {
        public FeaturePoint Origin;

        public BanditCampParams(FeaturePoint origin)
        {
            Origin = origin;
        }
    }
}
