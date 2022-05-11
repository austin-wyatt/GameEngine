using Empyrean.Definitions.TileEffects;
using Empyrean.Engine_Classes;
using Empyrean.Engine_Classes.MiscOperations;
using Empyrean.Engine_Classes.Scenes;
using Empyrean.Game.Abilities.SelectionTypes;
using Empyrean.Game.Map;
using Empyrean.Game.Tiles;
using Empyrean.Game.Units;
using Empyrean.Objects;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Empyrean.Game.Abilities
{
    public class TemplateRangedAOE : Ability
    {
        /// <summary>
        /// The tile pattern will determine which tiles are a part of the aoe
        /// </summary>
        public TemplateRangedAOE() { }
        public TemplateRangedAOE(Unit castingUnit)
        {
            Type = AbilityTypes.DamagingSpell;
            DamageType = DamageType.Magic;
            Range = 4;
            CastingUnit = castingUnit;
            CastRequirements.AddResourceCost(ResF.ActionEnergy, 1, Comparison.GreaterThanOrEqual, ExpendBehavior.Expend);

            CastingMethod |= CastingMethod.Magic;

            HasHoverEffect = true;

            SelectionInfo = new AOETarget(this, new List<Vector3i>());

            SelectionInfo.CanSelectTiles = true;
            SelectionInfo.UnitTargetParams.IsHostile = UnitCheckEnum.SoftTrue;
            SelectionInfo.UnitTargetParams.IsFriendly = UnitCheckEnum.SoftTrue;
            SelectionInfo.UnitTargetParams.IsNeutral = UnitCheckEnum.SoftTrue;

            Grade = 1;

            MaxCharges = 0;
            Charges = 0;
            ChargeRechargeCost = 0;


            AnimationSet = new Serializers.AnimationSet();
            AnimationSet.Animations.Add(new Serializers.Animation()
            {
                FrameIndices = { (int)Character.A },
                Spritesheet = (int)TextureName.CharacterSpritesheet
            });

            AbilityClass = AbilityClass.Unknown;

            //TilePattern = new List<Vector3i> { new Vector3i(0, 0, 0), new Vector3i(-1, 1, 0), new Vector3i(1, 0, -1), new Vector3i(1, -1, 0), new Vector3i(-1, 0, 1) };
        }

        //protected HashSet<Tile> _affectedTilesHashSet = new HashSet<Tile>();
        //public override void GetValidTileTargets(TileMap tileMap, out List<Tile> affectedTiles, out List<Unit> affectedUnits,
        //    List<Unit> units = default, Tile position = null)
        //{
        //    if (position == null)
        //    {
        //        position = CastingUnit.Info.TileMapPosition;
        //    }

        //    TileMap.TilesInRadiusParameters param = new TileMap.TilesInRadiusParameters(position, Range)
        //    {
        //        Units = units,
        //        CastingUnit = CastingUnit
        //    };

        //    List<Tile> validTiles = tileMap.FindValidTilesInRadius(param);

        //    affectedUnits = new List<Unit>();
        //    foreach (var tile in validTiles)
        //    {
        //        foreach(var unit in UnitPositionManager.GetUnitsOnTilePoint(tile))
        //        {
        //            if (UnitTargetParams.CheckUnit(unit, CastingUnit))
        //            {
        //                affectedUnits.Add(unit);
        //            }
        //        }
        //    }

        //    _affectedTilesHashSet = validTiles.ToHashSet();

        //    affectedTiles = validTiles;
            
        //}

        //protected HashSet<Tile> _selectedTiles = new HashSet<Tile>();
        //public override void OnSelect(CombatScene scene, TileMap currentMap)
        //{
        //    base.OnSelect(scene, currentMap);

        //    if (CanCast())
        //    {
        //        foreach(var tile in AffectedTiles)
        //        {
        //            tile.SelectionColor = _Colors.Blue;
        //            tile.SelectionMixPercent = 0.25f;
        //            scene._tileMapController.SelectTile(tile, TileSelectionType.Full);

        //            _selectedTiles.Add(tile);
        //        }
        //    }
        //}

        //public override void OnAbilityDeselect()
        //{
        //    base.OnAbilityDeselect();

        //    var scene = Scene;
        //    foreach(var tile in _selectedTiles)
        //    {
        //        scene._tileMapController.DeselectTile(tile);
        //    }
        //    _selectedTiles.Clear();

        //    ClearHoveredTiles();

        //    _affectedTilesHashSet.Clear();
        //}


        //public override void OnHover(Tile tile, TileMap map)
        //{
        //    base.OnHover(tile, map);

        //    if (!_affectedTilesHashSet.Contains(tile))
        //    {
        //        ClearHoveredTiles();
        //        return;
        //    }

        //    if(_hoveredTiles.Count > 0 && _hoveredTile == tile)
        //    {
        //        return;
        //    }

        //    ClearHoveredTiles();

        //    _hoveredTile = tile;

        //    var scene = Scene;

        //    Vector3i tileCubeCoords = CubeMethods.OffsetToCube(tile);

        //    foreach(var cubeCoord in TilePattern)
        //    {
        //        Vector3i newTileCube = tileCubeCoords + cubeCoord;
        //        Vector2i offsetCoords = CubeMethods.CubeToOffset(newTileCube);

        //        Tile foundTile = TileMapHelpers.GetTile(new FeaturePoint(offsetCoords.X, offsetCoords.Y));

        //        if (foundTile != null)
        //        {
        //            _hoveredTiles.Add(foundTile);

        //            foundTile.SelectionColor = _Colors.Green;
        //            tile.SelectionMixPercent = 0.5f;
        //            foundTile.CalculateDisplayedColor();
        //            //scene._tileMapController.SelectTile(foundTile, TileSelectionType.Selection);

        //            _hoveredSelectionTiles.Add(foundTile);
        //        }
        //    }
        //}

        //protected void ClearHoveredTiles()
        //{
        //    _hoveredTiles.Clear();

        //    _hoveredTile = null;

        //    foreach (var selectionTile in _hoveredSelectionTiles)
        //    {
        //        selectionTile.SelectionColor = _Colors.Blue;
        //        selectionTile.SelectionMixPercent = 0.25f;
        //        selectionTile.CalculateDisplayedColor();
                
        //    }
        //    _hoveredSelectionTiles.Clear();
        //}

        //public override void OnTileClicked(TileMap map, Tile tile)
        //{
        //    base.OnTileClicked(map, tile);

        //    if (CanTargetGround && _affectedTilesHashSet.Contains(tile))
        //    {
        //        EnactEffect();
        //    }
        //}

        //public override bool OnUnitClicked(Unit unit)
        //{
        //    if (base.OnUnitClicked(unit))
        //    {
        //        if (_affectedTilesHashSet.Contains(unit.Info.TileMapPosition))
        //        {
        //            EnactEffect();
        //        }
        //    }
        //    else
        //    {
        //        return false;
        //    }

        //    return true;
        //}

        public override void EnactEffect()
        {
            BeginEffect();

            //foreach(var tile in _hoveredTiles)
            //{
            //    foreach(var unit in UnitPositionManager.GetUnitsOnTilePoint(tile))
            //    {
            //        //do something
            //    }

            //    TileEffectManager.AddTileEffectToPoint(new WeakSpiderWeb(), tile);
            //}

            //Casted();
            //EffectEnded();
        }
    }
}
