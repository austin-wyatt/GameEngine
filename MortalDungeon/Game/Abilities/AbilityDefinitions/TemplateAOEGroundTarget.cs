﻿using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Game.Tiles;
using MortalDungeon.Game.Units;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using MortalDungeon.Engine_Classes.UIComponents;
using MortalDungeon.Objects;

namespace MortalDungeon.Game.Abilities
{
    public class TemplateAOEGroundTarget : Ability
    {
        public TemplateAOEGroundTarget(Unit castingUnit, int range = 3)
        {
            Type = AbilityTypes.Empty;
            DamageType = DamageType.NonDamaging;
            Range = range;
            CastingUnit = castingUnit;

            CanTargetGround = true;
            UnitTargetParams.IsHostile = UnitCheckEnum.False;
            UnitTargetParams.IsFriendly = UnitCheckEnum.False;
            UnitTargetParams.IsNeutral = UnitCheckEnum.False;

            //Name = "AOE Ground Target";

            SetIcon(IconSheetIcons.QuestionMark, Spritesheets.IconSheet);
        }

        public override List<BaseTile> GetValidTileTargets(TileMap tileMap, List<Unit> units = default, BaseTile position = null, List<Unit> validUnits = null)
        {
            base.GetValidTileTargets(tileMap);

            TileMap.TilesInRadiusParameters param = new TileMap.TilesInRadiusParameters(CastingUnit.Info.TileMapPosition, Range)
            {
                TraversableTypes = TileMapConstants.AllTileClassifications,
                Units = units,
                CastingUnit = CastingUnit
            };

            List<BaseTile> validTiles = tileMap.FindValidTilesInRadius(param);

            TrimTiles(validTiles, units);

            if (CastingUnit.AI.ControlType == ControlType.Controlled && CanTargetGround)
            {
                validTiles.ForEach(tile =>
                {
                    tile.TilePoint.ParentTileMap.Controller.SelectTile(tile);
                });
            }

            return validTiles;
        }

        public override void OnTileClicked(TileMap map, BaseTile tile)
        {
            if (AffectedTiles.Exists(t => t == tile))
            {
                SelectedTile = tile;
                EnactEffect();
                Scene._selectedAbility = null;

                map.Controller.DeselectTiles();
            }
        }


        public override void OnCast()
        {
            ClearSelectedTiles();

            base.OnCast();
        }

        public override void OnAICast()
        {
            base.OnAICast();
        }

        public override void EnactEffect()
        {
            base.EnactEffect();

            //create skeleton unit
            Console.WriteLine("Effect");

            Casted();
            EffectEnded();
        }

        public override void OnAbilityDeselect()
        {
            ClearSelectedTiles();

            base.OnAbilityDeselect();

            SelectedTile = null;
        }

        public void ClearSelectedTiles()
        {
            lock (AffectedTiles)
                AffectedTiles.ForEach(tile =>
                {
                    tile.TilePoint.ParentTileMap.Controller.DeselectTiles();
                });
        }
    }
}
