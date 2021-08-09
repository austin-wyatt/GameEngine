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
    public class Strike : Ability
    {
        public Strike(Unit castingUnit, int range = 1, float damage = 10)
        {
            Type = AbilityTypes.MeleeAttack;
            Range = range;
            CastingUnit = castingUnit;
            Damage = damage;
            EnergyCost = 5;

            Name = "Strike";

            Icon = new Icon(Icon.DefaultIconSize, Icon.IconSheetIcons.CrossedSwords, Spritesheets.IconSheet, true);
        }

        public override List<BaseTile> GetValidTileTargets(TileMap tileMap, List<Unit> units = default)
        {
            List<BaseTile> validTiles = tileMap.FindValidTilesInRadius(CastingUnit.TileMapPosition, Range, TileMapConstants.AllTileClassifications, units, CastingUnit);
            TileMap = tileMap;

            TrimTiles(validTiles, units);
            return validTiles;
        }

        public override bool OnUnitClicked(Unit unit)
        {
            if (!base.OnUnitClicked(unit))
                return false;
            
            if (unit.Team != CastingUnit.Team && AffectedTiles.FindIndex(t => t.TilePoint == unit.TileMapPosition) != -1) 
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

            SelectedUnit.ApplyDamage(GetDamage(), DamageType);

            OnCast();
        }
    }
}
