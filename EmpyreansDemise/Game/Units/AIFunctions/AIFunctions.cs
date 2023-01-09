using Empyrean.Engine_Classes;
using Empyrean.Engine_Classes.Scenes;
using Empyrean.Game.Abilities;
using Empyrean.Game.Tiles;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Empyrean.Game.Units.AIFunctions
{
    public class PotentialAIAction
    {
        public List<Tile> Path;
        public float PathCost = -1;

        public Unit TargetUnit;

        public float Weight = 0;
    }

    public static class AIFunctions
    {
        //public static bool GetPathToPointInRangeOfAbility(Unit castingUnit, Unit targetUnit, Ability ability, out List<BaseTile> path, out float pathCost)
        //{
        //    CombatScene Scene = castingUnit.Scene;
        //    TileMap Map = castingUnit.GetTileMap();
        //    var tilePosition = castingUnit.Info.TileMapPosition.TilePoint;

        //    castingUnit.Info._movementAbility.Units = Scene._units;

        //    path = null;

        //    List<BaseTile> validTiles = castingUnit.Info._movementAbility.GetValidTileTargets(Map, Scene._units);

        //    validTiles = validTiles.FindAll(tile => TileMap.GetDistanceBetweenPoints(tile, targetUnit.Info.TileMapPosition) <= ability.Range);

        //    float _pathCost = -1;

        //    validTiles.Randomize();

        //    const int MAX_SAMPLES = 50;
        //    //int sampleCount = 3;
        //    int sampleCount = 1;
        //    int sampleSize = validTiles.Count / sampleCount > MAX_SAMPLES ? MAX_SAMPLES : validTiles.Count / sampleCount;

        //    Console.WriteLine("ValidTiles size: " + validTiles.Count);

        //    List<BaseTile>[] presumptivePaths = new List<BaseTile>[sampleCount];
        //    List<Task> pathTasks = new List<Task>();

        //    for (int i = 0; i < sampleCount; i++)
        //    {
        //        presumptivePaths[i] = null;
        //        if (validTiles.Count > i)
        //        {
        //            int index = i;
        //            Task task = new Task(() =>
        //            {
        //                float presumptivePathCost = -1;
        //                for (int j = 0; j < sampleSize; j++)
        //                {
        //                    if (ability.UnitInRange(targetUnit, validTiles[j + index * sampleSize]))
        //                    {
        //                        TileMap.PathToPointParameters param = new TileMap.PathToPointParameters(tilePosition, validTiles[j + index * sampleSize].TilePoint, castingUnit.Info.Energy / castingUnit.Info._movementAbility.GetEnergyCost())
        //                        {
        //                            Units = Scene._units,
        //                            TraversableTypes = new List<TileClassification>() { TileClassification.Ground },
        //                            CastingUnit = castingUnit,
        //                            CheckTileLower = true,
        //                            AbilityType = AbilityTypes.Move
        //                        };

        //                        List<BaseTile> tempPath = Map.GetPathToPoint(param);
        //                        float tempPathCost = castingUnit.AI.GetPathMovementCost(tempPath);


        //                        if (presumptivePathCost == -1 && tempPath.Count != 0)
        //                        {
        //                            presumptivePaths[index] = tempPath;
        //                            presumptivePathCost = tempPathCost;
        //                        }
        //                        else if (tempPathCost < _pathCost && tempPathCost != 0)
        //                        {
        //                            presumptivePaths[index] = tempPath;
        //                            presumptivePathCost = tempPathCost;
        //                        }

        //                        if (j > sampleSize / 2)
        //                        {
        //                            break; //if we aren't finding a lot of better paths we can assume the path we have is reasonably efficient
        //                        }
        //                    }
        //                }
        //            });

        //            task.Start();
        //            pathTasks.Add(task);
        //        }
        //    }

        //    foreach (var task in pathTasks)
        //    {
        //        task.Wait();
        //    }

        //    foreach (List<BaseTile> presumptivePath in presumptivePaths)
        //    {
        //        if (presumptivePath != null)
        //        {
        //            float tempPathCost = castingUnit.AI.GetPathMovementCost(presumptivePath);
        //            if (_pathCost == -1 && presumptivePath.Count != 0)
        //            {
        //                path = presumptivePath;
        //                _pathCost = tempPathCost;
        //            }
        //            else if (tempPathCost < _pathCost && tempPathCost != 0)
        //            {
        //                path = presumptivePath;
        //                _pathCost = tempPathCost;
        //            }
        //        }
        //    }

        //    pathCost = _pathCost;

        //    return path != null;
        //}
    }
}
