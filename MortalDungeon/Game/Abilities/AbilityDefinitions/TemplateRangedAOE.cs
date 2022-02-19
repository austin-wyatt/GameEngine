using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.MiscOperations;
using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Game.Tiles;
using MortalDungeon.Game.Units;
using MortalDungeon.Objects;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MortalDungeon.Game.Abilities
{
    public class TemplateRangedAOE : Ability
    {
        /// <summary>
        /// The tile pattern will determine which tiles are a part of the aoe
        /// </summary>
        public List<Vector3i> TilePattern = new List<Vector3i> { new Vector3i() };

        public TemplateRangedAOE() { }
        public TemplateRangedAOE(Unit castingUnit)
        {
            Type = AbilityTypes.DamagingSpell;
            DamageType = DamageType.Magic;
            Range = 4;
            CastingUnit = castingUnit;
            Damage = 0;
            ActionCost = 1;

            CastingMethod |= CastingMethod.Magic;

            HasHoverEffect = true;

            CanTargetGround = true;
            UnitTargetParams.IsHostile = UnitCheckEnum.SoftTrue;
            UnitTargetParams.IsFriendly = UnitCheckEnum.SoftTrue;
            UnitTargetParams.IsNeutral = UnitCheckEnum.SoftTrue;
            CanTargetThroughFog = true;

            Grade = 1;

            MaxCharges = 0;
            Charges = 0;
            ChargeRechargeCost = 0;

            SetIcon(Character.A, Spritesheets.CharacterSheet);

            AbilityClass = AbilityClass.Unknown;

            TilePattern = new List<Vector3i> { new Vector3i(0, 0, 0), new Vector3i(-1, 1, 0), new Vector3i(1, 0, -1), new Vector3i(1, -1, 0), new Vector3i(-1, 0, 1) };
        }

        private HashSet<BaseTile> _affectedTilesHashSet = new HashSet<BaseTile>();
        public override List<BaseTile> GetValidTileTargets(TileMap tileMap, List<Unit> units = default, BaseTile position = null, List<Unit> validUnits = null)
        {
            base.GetValidTileTargets(tileMap);

            if (position == null)
            {
                position = CastingUnit.Info.TileMapPosition;
            }

            TileMap.TilesInRadiusParameters param = new TileMap.TilesInRadiusParameters(position, Range)
            {
                TraversableTypes = TileMapConstants.AllTileClassifications,
                Units = units,
                CastingUnit = CastingUnit
            };

            List<BaseTile> validTiles = tileMap.FindValidTilesInRadius(param);

            TrimTiles(validTiles, units, validUnits: validUnits);

            _affectedTilesHashSet = validTiles.ToHashSet();

            return validTiles;
        }

        private HashSet<BaseTile> _selectedTiles = new HashSet<BaseTile>();
        public override void OnSelect(CombatScene scene, TileMap currentMap)
        {
            base.OnSelect(scene, currentMap);

            if (CanCast())
            {
                foreach(var tile in AffectedTiles)
                {
                    var selectedTile = scene._tileMapController.SelectTile(tile, TileSelectionType.Full);

                    selectedTile.SetColor(_Colors.TranslucentBlue);

                    _selectedTiles.Add(selectedTile); ;
                }
            }
        }

        public override void OnAbilityDeselect()
        {
            base.OnAbilityDeselect();

            var scene = Scene;
            foreach(var tile in _selectedTiles)
            {
                scene._tileMapController.DeselectTile(tile);
            }
            _selectedTiles.Clear();

            ClearHoveredTiles();

            _affectedTilesHashSet.Clear();
        }


        private BaseTile _hoveredTile = null;
        private HashSet<BaseTile> _hoveredTiles = new HashSet<BaseTile>();
        private List<BaseTile> _hoveredSelectionTiles = new List<BaseTile>();
        public override void OnHover(BaseTile tile, TileMap map)
        {
            base.OnHover(tile, map);

            if (!_affectedTilesHashSet.Contains(tile))
            {
                ClearHoveredTiles();
                return;
            }

            if(_hoveredTiles.Count > 0 && _hoveredTile == tile)
            {
                return;
            }

            ClearHoveredTiles();

            _hoveredTile = tile;

            var scene = Scene;

            Vector3i tileCubeCoords = CubeMethods.OffsetToCube(tile);

            foreach(var cubeCoord in TilePattern)
            {
                Vector3i newTileCube = tileCubeCoords + cubeCoord;
                Vector2i offsetCoords = CubeMethods.CubeToOffset(newTileCube);

                BaseTile foundTile = map.GetTile(offsetCoords.X, offsetCoords.Y);

                if(foundTile != null)
                {
                    _hoveredTiles.Add(foundTile);

                    var selectedTile = scene._tileMapController.SelectTile(foundTile, TileSelectionType.Selection);
                    selectedTile.SetColor(_Colors.Green);

                    _hoveredSelectionTiles.Add(selectedTile);
                }
            }
        }

        private void ClearHoveredTiles()
        {
            _hoveredTiles.Clear();

            _hoveredTile = null;

            var scene = Scene;

            foreach (var selectionTile in _hoveredSelectionTiles)
            {
                scene._tileMapController.DeselectTile(selectionTile);
            }
            _hoveredSelectionTiles.Clear();
        }

        public override void OnTileClicked(TileMap map, BaseTile tile)
        {
            base.OnTileClicked(map, tile);

            if (CanTargetGround && _affectedTilesHashSet.Contains(tile))
            {
                EnactEffect();
            }
        }

        public override bool OnUnitClicked(Unit unit)
        {
            if (base.OnUnitClicked(unit))
            {
                if (_affectedTilesHashSet.Contains(unit.Info.TileMapPosition))
                {
                    EnactEffect();
                }
            }
            else
            {
                return false;
            }

            return true;
        }

        public override void EnactEffect()
        {
            base.EnactEffect();

            foreach(var tile in _hoveredTiles)
            {
                foreach(var unit in UnitPositionManager.GetUnitsOnTilePoint(tile))
                {
                    //do something
                }

                TileEffectManager.AddTileEffectToPoint(new TileEffectDefinitions.WeakSpiderWeb(), tile);
            }

            Casted();
            EffectEnded();
        }
    }
}
