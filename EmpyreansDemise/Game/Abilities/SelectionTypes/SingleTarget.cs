using Empyrean.Game.Units;
using Empyrean.Game.Tiles;
using System;
using System.Collections.Generic;
using System.Text;
using Empyrean.Game.Combat;
using System.Threading.Tasks;
using Empyrean.Engine_Classes.MiscOperations;
using Empyrean.Engine_Classes;

namespace Empyrean.Game.Abilities.SelectionTypes
{

    /// <summary>
    /// Allows selection of only 1 target before enacting the ability's effect
    /// </summary>
    public class SingleTarget : SelectionInfo
    {
        public SingleTarget(Ability ability) : base(ability)
        {
        }

        public override void FindTargets()
        {
            TargetedTiles.Clear();
            TargetedUnits.Clear();

            if (SourceTile == null)
            {
                SourceTile = Ability.CastingUnit.Info.TileMapPosition;
            }

            if (CanSelectUnits)
            {
                var units = UnitPositionManager.GetUnitsInRadius((int)Ability.Range, SourceTile.ToFeaturePoint());

                foreach (var unit in units)
                {
                    if (UnitTargetParams.CheckUnit(unit, Ability.CastingUnit))
                    {
                        if (Context.GetFlag(SelectionInfoContext.LineRequiredToTarget))
                        {
                            List<Tile> lineOfTiles = SourceTile.TileMap.GetLineOfTiles(SourceTile, unit.Info.TileMapPosition);

                            bool valid = true;
                            for (int i = 0; i < lineOfTiles.Count; i++)
                            {
                                if (lineOfTiles[i].BlocksType(BlockingType.Abilities))
                                {
                                    valid = false;
                                    break;
                                }
                            }
                            if(valid)
                                TargetedUnits.Add(unit);
                        }
                        else
                        {
                            TargetedUnits.Add(unit);
                        }
                    }
                }
            }

            if (CanSelectTiles)
            {
                var tiles = SourceTile.TileMap.FindValidTilesInRadius(new TileMap.TilesInRadiusParameters()
                {
                    StartingPoint = SourceTile,
                    Radius = Ability.Range,
                    CheckTileHigher = true,
                    CheckTileLower = true
                });

                for(int i = 0; i < tiles.Count; i++)
                {
                    TargetedTiles.Add(tiles[i]);
                }
            }
        }

        protected override void CheckSelectionStatus()
        {
            if(SelectedUnits.Count > 0 || SelectedTiles.Count > 0)
            {
                OnConditionsMet();

                if (UseAbility)
                {
                    Ability.EnactEffect();
                }
            }
        }

        public override void CreateVisualIndicators()
        {
            base.CreateVisualIndicators();

            foreach(var unit in TargetedUnits)
            {
                unit.Target();
            }
        }

        public override void RemoveVisualIndicators()
        {
            base.RemoveVisualIndicators();

            foreach (var unit in TargetedUnits)
            {
                unit.Untarget();
            }
        }

        public static void GenerateDefaultTargetInfoForAbility(Ability ability)
        {
            Dictionary<Unit, NavTileWithParent> potentialMoves = new Dictionary<Unit, NavTileWithParent>();

            ability.AITargetSelection.GenerateAction = (morsel) =>
            {
                Func<Task<bool>> action = async () =>
                {
                    if (potentialMoves.TryGetValue(morsel.Unit, out var potentialMove))
                    {
                        ability.CastingUnit.Info._movementAbility._path.Clear();

                        ability.CastingUnit.Info._movementAbility._path.Add(potentialMove.NavTile.Tile);
                        while (potentialMove.Parent != null)
                        {
                            potentialMove = potentialMove.Parent;
                            ability.CastingUnit.Info._movementAbility._path.Add(potentialMove.NavTile.Tile);
                        }
                        ability.CastingUnit.Info._movementAbility._path.Reverse();

                        ability.CastingUnit.Info._movementAbility.EnactEffect();
                        await ability.CastingUnit.Info._movementAbility.EffectEndedAsync.Task;
                    }

                    ability.SelectionInfo.SelectedUnits.Add(morsel.Unit);
                    ability.SelectionInfo.SelectAbilityAI();

                    bool successful = false;

                    ability.SelectionInfo.FindTargets();
                    if (ability.SelectionInfo.TargetedUnits.Contains(morsel.Unit))
                    {
                        await ability.EffectManager.EnactEffect();
                        successful = true;
                    }

                    ability.SelectionInfo.DeselectAbilityAI();
                    potentialMoves.Clear();

                    return successful;
                };

                return action;
            };

            ability.AITargetSelection.GenerateFeasibilityCheck = (availableMovePaths) =>
            {
                bool valid = false;

                int distance = CubeMethods.GetDistanceBetweenPoints(ability.CastingUnit.Info.TileMapPosition, availableMovePaths.AssociatedMorsel.Unit.Info.TileMapPosition);

                if (distance <= ability.Range)
                    valid = true;


                for (int i = (int)ability.Range; i >= ability.MinRange && !valid; i--)
                {
                    if (!valid && availableMovePaths.UnionedTilesByDistanceFromUnit.TryGetValue(i, out var foundTiles))
                    {
                        foreach (var tile in foundTiles)
                        {
                            potentialMoves.AddOrSet(availableMovePaths.AssociatedMorsel.Unit, tile);
                            valid = true;
                            break;
                        }
                    }
                }
                

                Func<bool> action = () =>
                {
                    return valid;
                };

                return action;
            };
        }
    }
}
