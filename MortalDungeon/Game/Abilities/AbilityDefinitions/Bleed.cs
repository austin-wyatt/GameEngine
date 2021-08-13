using MortalDungeon.Engine_Classes.Scenes;
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
    public class Bleed : Ability
    {
        int _bleedDuration;
        float _bleedDamage;

        public Bleed(Unit castingUnit, int range = 1, float bleedDamage = 15f, int duration = 3)
        {
            Type = AbilityTypes.Debuff;
            Range = range;
            CastingUnit = castingUnit;
            EnergyCost = 1;

            _bleedDuration = duration;
            _bleedDamage = bleedDamage;

            Name = "Bleed";

            CanTargetGround = false;

            Icon = new Icon(Icon.DefaultIconSize, Icon.IconSheetIcons.BleedingDagger, Spritesheets.IconSheet, true, Icon.BackgroundType.DebuffBackground);
        }

        public override List<BaseTile> GetValidTileTargets(TileMap tileMap, List<Unit> units = default)
        {
            TileMap.TilesInRadiusParameters param = new TileMap.TilesInRadiusParameters(CastingUnit.TileMapPosition, Range)
            {
                TraversableTypes = TileMapConstants.AllTileClassifications,
                Units = units,
                CastingUnit = CastingUnit
            };


            List<BaseTile> validTiles = tileMap.FindValidTilesInRadius(param);
            TileMap = tileMap;

            TrimTiles(validTiles, units);
            return validTiles;
        }

        public override bool OnUnitClicked(Unit unit)
        {
            if (!base.OnUnitClicked(unit))
                return false;

            if (AffectedTiles.FindIndex(t => t.TilePoint == unit.TileMapPosition) != -1)
            {
                SelectedUnit = unit;
                EnactEffect();
            }

            return true;
        }

        public override void OnCast()
        {
            Scene.EnergyDisplayBar.HoverAmount(0);

            float energyCost = GetEnergyCost();

            //special cases for energy reduction go here

            Scene.EnergyDisplayBar.AddEnergy(-energyCost);

            TileMap.DeselectTiles();

            base.OnCast();
        }

        public override void EnactEffect()
        {
            base.EnactEffect();

            BleedDebuff bleedDebuff = new BleedDebuff(SelectedUnit, _bleedDuration, _bleedDamage);

            OnCast();
        }
    }
}
