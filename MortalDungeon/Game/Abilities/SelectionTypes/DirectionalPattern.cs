using Empyrean.Engine_Classes;
using Empyrean.Engine_Classes.MiscOperations;
using Empyrean.Game.Map;
using Empyrean.Game.Tiles;
using Empyrean.Game.Units;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Game.Abilities.SelectionTypes
{
    /// <summary>
    /// Allows selection of a pattern in one of the 6 directions
    /// </summary>
    internal class DirectionalPattern : SelectionInfo
    {
        public List<Vector3i> TilePattern = new List<Vector3i>();
        public DirectionalPattern(Ability ability, List<Vector3i> tilePattern) : base(ability)
        {
            TilePattern = tilePattern;
        }

        protected override void FindTargets()
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

            foreach(var tile in TileBuffer)
            {
                var unitsOnTile = UnitPositionManager.GetUnitsOnTilePoint(tile);

                foreach(var unit in unitsOnTile)
                {
                    SelectedUnits.Add(unit);
                }
            }

            CheckSelectionStatus();

            return false;
        }

        public override bool UnitClicked(Unit clickedUnit)
        {
            return TileClicked(clickedUnit.Info.TileMapPosition);
        }

        protected override void CheckSelectionStatus()
        {
            Ability.EnactEffect();
        }

        protected override void CreateVisualIndicators()
        {
            base.CreateVisualIndicators();

            _hoveredDirection = Direction.None;
            _hoveredTile = null;

            if(Scene._tileMapController._hoveredTile != null)
            {
                TileHovered(Scene._tileMapController._hoveredTile);
            }
        }

        protected override void RemoveVisualIndicators()
        {
            base.RemoveVisualIndicators();

            ClearTargetedTiles();

            _hoveredDirection = Direction.None;
            _hoveredTile = null;
        }

        public Tile _hoveredTile;
        public Direction _hoveredDirection = Direction.None;

        public override void TileHovered(Tile tile)
        {
            //Calculate direction from casting unit position to the hovered tile

            //Create the hovered tile if the current direction is not that direction

            Direction direction = DirectionBetweenTiles(SourceTile, tile);
            
            if (direction == _hoveredDirection)
            {
                return;
            }

            ClearTargetedTiles();

            _hoveredDirection = direction;
            _hoveredTile = tile;

            Vector3i tileCubeCoords = CubeMethods.OffsetToCube(SourceTile);

            foreach (var cubeCoord in TilePattern)
            {
                Vector3i newTileCube = tileCubeCoords + CubeMethods.RotateCube(cubeCoord, GMath.NegMod((int)_hoveredDirection + (int)Direction.North, 6));
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

        private Direction DirectionBetweenTiles(Tile source, Tile destination)
        {
            Vector3 sourcePos = new Vector3(0, 1, 0);
            Vector3 destinationPos = destination._position - source._position;

            sourcePos.Z = 0;
            destinationPos.Z = 0;

            sourcePos.Normalize();
            destinationPos.Normalize();

            float dot = Vector3.Dot(sourcePos, destinationPos);
            //float direction = Vector3.Dot(Vector3.Cross(destinationPos, sourcePos), new Vector3(0, 0, 1));
            float det = sourcePos.X * destinationPos.Y - sourcePos.Y * destinationPos.X;

            float angle = (float)MathHelper.Atan2(det, dot) + MathHelper.Pi;

            const float SIX_OVER_TWO_PI = 6 / MathHelper.TwoPi;
            
            //Add 30 degrees so that each sextant is centered
            angle += MathHelper.PiOver6;

            //convert the range of [0: 2pi] to [0:6]
            angle *= SIX_OVER_TWO_PI;

            //the directions enum advances clockwise so we need to reverse the value
            angle = 6 - angle;

            //bring everything back to the range of [0:6]
            angle = GMath.NegMod(angle, 6);

            return (Direction)angle;
        }
    }
}
