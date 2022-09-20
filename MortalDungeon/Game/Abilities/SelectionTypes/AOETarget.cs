using Empyrean.Game.Units;
using Empyrean.Game.Tiles;
using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Mathematics;
using Empyrean.Engine_Classes;
using Empyrean.Engine_Classes.MiscOperations;
using Empyrean.Game.Map;

namespace Empyrean.Game.Abilities.SelectionTypes
{
    /// <summary>
    /// Allows selection of an area of tiles
    /// </summary>
    public class AOETarget : SelectionInfo
    {
        public List<Vector3i> TilePattern = new List<Vector3i>();
        public AOETarget(Ability ability, List<Vector3i> tilePattern) : base(ability)
        {
            TilePattern = tilePattern;
        }

        public override void FindTargets()
        {
            TargetedTiles.Clear();
            TargetedUnits.Clear();

            if (SourceTile == null)
            {
                SourceTile = Ability.CastingUnit.Info.TileMapPosition;
            }

            TileMap.TilesInRadiusParameters param = new TileMap.TilesInRadiusParameters(SourceTile, Ability.Range)
            {
                Units = Scene._units,
                CastingUnit = Ability.CastingUnit
            };

            List<Tile> validTiles = SourceTile.TileMap.FindValidTilesInRadius(param);

            foreach (var tile in validTiles)
            {
                if (CanSelectTiles)
                {
                    TargetedTiles.Add(tile);
                }

                foreach (var unit in UnitPositionManager.GetUnitsOnTilePoint(tile))
                {
                    if (CanSelectUnits && UnitTargetParams.CheckUnit(unit, Ability.CastingUnit))
                    {
                        TargetedUnits.Add(unit);
                    }
                }
            }
        }

        public override bool TileClicked(Tile clickedTile)
        {
            if (!CanSelectTiles)
                return false;

            if (TargetedTiles.Contains(clickedTile))
            {
                for(int i = 0; i < TileBuffer.Count; i++)
                {
                    SelectedTiles.Add(TileBuffer[i]);
                }

                CheckSelectionStatus();

                return true;
            }

            return false;
        }

        protected override void CheckSelectionStatus()
        {
            if (SelectedUnits.Count > 0 || SelectedTiles.Count > 0)
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

            foreach (var unit in TargetedUnits)
            {
                unit.Target();
            }

            foreach(var tile in TargetedTiles)
            {
                tile.SelectionColor = _Colors.Blue;
                Scene._tileMapController.SelectTile(tile);
            }
        }

        public override void RemoveVisualIndicators()
        {
            base.RemoveVisualIndicators();

            foreach (var unit in TargetedUnits)
            {
                unit.Untarget();
            }

            Scene._tileMapController.DeselectTiles(TargetedTiles);

            ClearTargetedTiles();
        }

        public Tile _hoveredTile;

        public override void TileHovered(Tile tile)
        {
            if (!TargetedTiles.Contains(tile))
            {
                ClearTargetedTiles();

                return;
            }

            if (TileBuffer.Count > 0 && _hoveredTile == tile)
            {
                return;
            }

            ClearTargetedTiles();

            _hoveredTile = tile;

            Vector3i tileCubeCoords = CubeMethods.OffsetToCube(tile);

            foreach (var cubeCoord in TilePattern)
            {
                Vector3i newTileCube = tileCubeCoords + cubeCoord;
                Vector2i offsetCoords = CubeMethods.CubeToOffset(newTileCube);

                Tile foundTile = TileMapHelpers.GetTile(new FeaturePoint(offsetCoords.X, offsetCoords.Y));

                if (foundTile != null)
                {
                    TileBuffer.Add(foundTile);
                    var indicator = Scene._tileMapController.TargetTile(foundTile);
                    indicator.Color = _Colors.Tan;
                }
            }
        }

        private void ClearTargetedTiles()
        {
            for (int i = TileBuffer.Count - 1; i >= 0; i--)
            {
                Scene._tileMapController.UntargetTile(TileBuffer[i]);
                TileBuffer.RemoveAt(i);
            }
        }
    }
}
