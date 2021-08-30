using MortalDungeon.Game.Abilities;
using MortalDungeon.Game.Tiles;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Units.AI
{
    class MoveInRangeOfAbility : UnitAIAction
    {
        public MoveInRangeOfAbility(Unit castingUnit, Ability ability = null, BaseTile tile = null, Unit unit = null) : base(castingUnit, ability, tile, unit) { }

        public override void EnactEffect()
        {
            CastingUnit.Info._movementAbility.Units = Scene._units;

            List<BaseTile> path = null;

            List<BaseTile> validTiles = CastingUnit.Info._movementAbility.GetValidTileTargets(Map, Scene._units);

            float pathCost = -1;


            for (int i = 0; i < validTiles.Count; i++)
            {
                if (Ability.UnitInRange(TargetedUnit, validTiles[i]))
                {
                    TileMap.PathToPointParameters param = new TileMap.PathToPointParameters(TilePosition, validTiles[i].TilePoint, CastingUnit.Info.Energy / CastingUnit.Info._movementAbility.GetEnergyCost())
                    {
                        Units = Scene._units,
                        TraversableTypes = new List<TileClassification>() { TileClassification.Ground },
                        CastingUnit = CastingUnit,
                        CheckTileLower = true,
                        AbilityType = AbilityTypes.Move
                    };

                    List<BaseTile> tempPath = Map.GetPathToPoint(param);
                    float tempPathCost = CastingUnit.AI.GetPathMovementCost(tempPath);

                    if (pathCost == -1 && tempPath.Count != 0)
                    {
                        path = tempPath;
                        pathCost = CastingUnit.AI.GetPathMovementCost(path);
                    }
                    else if (tempPathCost < pathCost && tempPathCost != 0)
                    {
                        path = tempPath;
                        pathCost = tempPathCost;
                    }
                }

                if (i == validTiles.Count - 1 && path == null)
                {
                    path = GetClosestPath();
                    Console.WriteLine("Using closest path");
                }
            }

            if (path == null || path.Count == 0) 
            {
                CastingUnit.AI.EndTurn();
            }

            CastingUnit.Info._movementAbility.CurrentTiles = path;
            CastingUnit.Info._movementAbility.EnactEffect();

            CastingUnit.Info._movementAbility.EffectEndedAction = () =>
            {
                CastingUnit.Info._movementAbility.EffectEndedAction = null;
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

            path.RemoveAt(path.Count - 1);

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
