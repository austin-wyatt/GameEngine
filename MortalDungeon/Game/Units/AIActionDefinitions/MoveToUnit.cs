using MortalDungeon.Game.Abilities;
using MortalDungeon.Game.Tiles;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Units.AI
{
    class MoveToUnit : UnitAIAction
    {
        public int DistanceFromEnemy = 0;

        public MoveToUnit(Unit castingUnit, Ability ability = null, BaseTile tile = null, Unit unit = null) : base(castingUnit, ability, tile, unit) { }

        public override void EnactEffect()
        {
            CastingUnit.Info._movementAbility.Units = Scene._units;

            List<BaseTile> path = null;
            List<BaseTile> tempPath;

            bool fullPathToUnit = false;

            if (TargetedUnit == null)
            {
                Scene._units.ForEach(u =>
                {
                    if (u.AI.Team == CastingUnit.AI.EnemyTeam)
                    {
                        TileMap.PathToPointParameters param = new TileMap.PathToPointParameters(TilePosition, u.Info.TileMapPosition.TilePoint, CastingUnit.Info.Energy / CastingUnit.Info._movementAbility.GetEnergyCost())
                        {
                            IgnoreTargetUnit = true,
                            Units = Scene._units,
                            TraversableTypes = new List<TileClassification>() { TileClassification.Ground },
                            CastingUnit = CastingUnit,
                            CheckTileLower = true,
                            AbilityType = AbilityTypes.Move
                        };

                        tempPath = Map.GetPathToPoint(param);

                        if (path == null)
                        {
                            path = tempPath;
                        }
                        else
                        {
                            if (CastingUnit.AI.GetPathMovementCost(tempPath) < CastingUnit.AI.GetPathMovementCost(path))
                            {
                                path = tempPath;
                            }
                        }
                    }
                });
            }
            else 
            {
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
            }

            if (path == null || path.Count == 0)
            {
                CastingUnit.AI.BeginNextAction();
                return;
            }

            path.RemoveAt(path.Count - 1);

            if (fullPathToUnit && DistanceFromEnemy > 0 && DistanceFromEnemy < path.Count) 
            {
                path.RemoveRange(path.Count - DistanceFromEnemy, DistanceFromEnemy);
            }

            CastingUnit.Info._movementAbility.CurrentTiles = path;
            CastingUnit.Info._movementAbility.EnactEffect();

            CastingUnit.Info._movementAbility.EffectEndedAction = () =>
            {
                CastingUnit.AI.BeginNextAction();
                CastingUnit.Info._movementAbility.EffectEndedAction = null;
            };
        }
    }
}
