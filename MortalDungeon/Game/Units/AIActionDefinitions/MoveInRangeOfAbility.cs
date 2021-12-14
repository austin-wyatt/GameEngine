using MortalDungeon.Engine_Classes;
using MortalDungeon.Game.Abilities;
using MortalDungeon.Game.Tiles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace MortalDungeon.Game.Units.AI
{
    class MoveInRangeOfAbility : UnitAIAction
    {
        public MoveInRangeOfAbility(Unit castingUnit, Ability ability = null, BaseTile tile = null, Unit unit = null) : base(castingUnit, ability, tile, unit) { }

        public override void EnactEffect()
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();

            CastingUnit.Info._movementAbility.Units = Scene._units;

            List<BaseTile> path = null;

            List<BaseTile> validTiles = CastingUnit.Info._movementAbility.GetValidTileTargets(Map, Scene._units);

            float pathCost = -1;

            validTiles.Randomize();

            //split the range finding into some amount of tasks
            //cut the tasks short in the same way we do now and compare their paths
            //if there are no paths then calculate the closest path and use that

            const int MAX_SAMPLES = 50;
            int sampleCount = 3;
            int sampleSize = validTiles.Count / sampleCount > MAX_SAMPLES ? MAX_SAMPLES : validTiles.Count / sampleCount;


            List<BaseTile>[] presumptivePaths = new List<BaseTile>[sampleCount];
            List<Task> pathTasks = new List<Task>();

            for (int i = 0; i < sampleCount; i++) 
            {
                presumptivePaths[i] = null;
                if (validTiles.Count > i) 
                {
                    int index = i;
                    Task task = new Task(() =>
                    {
                        float presumptivePathCost = -1;
                        for (int j = 0; j < sampleSize; j++) 
                        {
                            if (Ability.UnitInRange(TargetedUnit, validTiles[j + index * sampleSize]))
                            {
                                TileMap.PathToPointParameters param = new TileMap.PathToPointParameters(TilePosition, validTiles[j + index * sampleSize].TilePoint, CastingUnit.Info.Energy / CastingUnit.Info._movementAbility.GetEnergyCost())
                                {
                                    Units = Scene._units,
                                    TraversableTypes = new List<TileClassification>() { TileClassification.Ground },
                                    CastingUnit = CastingUnit,
                                    CheckTileLower = true,
                                    AbilityType = AbilityTypes.Move
                                };

                                List<BaseTile> tempPath = Map.GetPathToPoint(param);
                                float tempPathCost = CastingUnit.AI.GetPathMovementCost(tempPath);


                                if (presumptivePathCost == -1 && tempPath.Count != 0)
                                {
                                    presumptivePaths[index] = tempPath;
                                    presumptivePathCost = tempPathCost;
                                }
                                else if (tempPathCost < pathCost && tempPathCost != 0)
                                {
                                    presumptivePaths[index] = tempPath;
                                    presumptivePathCost = tempPathCost;
                                }

                                if (j > sampleSize / 2)
                                {
                                    break; //if we aren't finding a lot of better paths we can assume the path we have is reasonably efficient
                                }
                            }
                        }
                    });

                    task.Start();
                    pathTasks.Add(task);
                }
            }

            foreach (var task in pathTasks) 
            {
                task.Wait();
            }

            Console.WriteLine($"MoveInRangeOfAbility tasks completed in {timer.ElapsedMilliseconds}ms");

            foreach (List<BaseTile> presumptivePath in presumptivePaths) 
            {
                if (presumptivePath != null) 
                {
                    float tempPathCost = CastingUnit.AI.GetPathMovementCost(presumptivePath);
                    if (pathCost == -1 && presumptivePath.Count != 0)
                    {
                        path = presumptivePath;
                        pathCost = tempPathCost;
                    }
                    else if (tempPathCost < pathCost && tempPathCost != 0)
                    {
                        path = presumptivePath;
                        pathCost = tempPathCost;
                    }
                }
            }


            if (path == null) 
            {
                path = GetClosestPath();
                Console.WriteLine("Using closest path");
            }

            if (path == null || path.Count == 0) 
            {
                CastingUnit.AI.EndTurn();
            }

            Console.WriteLine($"MoveInRangeOfAbility path found in {timer.ElapsedMilliseconds}ms");

            CastingUnit.Info._movementAbility.CurrentTiles = path;
            CastingUnit.Info._movementAbility.EnactEffect();

            CastingUnit.Info._movementAbility.EffectEndedAction = () =>
            {
                CastingUnit.Info._movementAbility.EffectEndedAction = null;

                Console.WriteLine($"MoveInRangeOfAbility effect completed in {timer.ElapsedMilliseconds}ms");
                CastingUnit.AI.BeginNextAction();
            };
        }

        private List<BaseTile> GetClosestPath() 
        {
            List<BaseTile> path;

            bool fullPathToUnit = false;

            TileMap.PathToPointParameters param = new TileMap.PathToPointParameters(TilePosition, TargetedUnit.Info.TileMapPosition.TilePoint, 50)
            {
                IgnoreTargetUnit = true,
                Units = Scene._units,
                TraversableTypes = new List<TileClassification>() { TileClassification.Ground },
                CastingUnit = CastingUnit,
                CheckTileLower = true,
                AbilityType = AbilityTypes.Move
            };

            path = Map.GetPathToPoint(param);

            int pathLength = path.Count;

            path = CastingUnit.AI.GetAffordablePath(path);

            if (path.Count == pathLength)
            {
                fullPathToUnit = true;
            }

            //if (pathLength > 0) 
            //{
            //    path.Insert(0, CastingUnit.Info.TileMapPosition);
            //}

            //path.RemoveAt(path.Count - 1);

            //if (fullPathToUnit && path.Count > Ability.MinRange + 1)
            //{
            //    path.RemoveRange(path.Count - Ability.MinRange + 1, Ability.MinRange - 1);
            //}

            if (path == null || path.Count == 0)
            {
                return null;
            }

            return path;
        }
    }
}
