using Empyrean.Game.Units;
using Empyrean.Game.Tiles;
using System;
using System.Collections.Generic;
using System.Text;

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

        protected override void FindTargets()
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
                Ability.EnactEffect();
            }
        }

        protected override void CreateVisualIndicators()
        {
            base.CreateVisualIndicators();

            foreach(var unit in TargetedUnits)
            {
                unit.Target();
            }
        }

        protected override void RemoveVisualIndicators()
        {
            base.RemoveVisualIndicators();

            foreach (var unit in TargetedUnits)
            {
                unit.Untarget();
            }
        }
    }
}
