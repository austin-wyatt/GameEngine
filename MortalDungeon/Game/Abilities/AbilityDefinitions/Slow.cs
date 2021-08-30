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
    public class Slow : Ability
    {
        float _slowMultiplier;
        int _slowDuration;

        public Slow(Unit castingUnit, int range = 1, float slowAmount = 0.25f, int duration = 3)
        {
            Type = AbilityTypes.Debuff;
            Range = range;
            CastingUnit = castingUnit;
            EnergyCost = 1;

            _slowDuration = duration;
            _slowMultiplier = 1 + slowAmount;

            Name = "Slow";

            CanTargetGround = false;

            Icon = new Icon(Icon.DefaultIconSize, Icon.IconSheetIcons.SpiderWeb, Spritesheets.IconSheet, true, Icon.BackgroundType.DebuffBackground);
        }

        public override List<BaseTile> GetValidTileTargets(TileMap tileMap, List<Unit> units = default, BaseTile position = null)
        {
            TileMap.TilesInRadiusParameters param = new TileMap.TilesInRadiusParameters(CastingUnit.Info.TileMapPosition, Range)
            {
                TraversableTypes = TileMapConstants.AllTileClassifications,
                Units = units,
                CastingUnit = CastingUnit
            };

            List<BaseTile> validTiles = tileMap.FindValidTilesInRadius(param);

            TrimTiles(validTiles, units);

            TargetAffectedUnits();

            return validTiles;
        }

        public override bool OnUnitClicked(Unit unit)
        {
            if (!base.OnUnitClicked(unit))
                return false;

            if (AffectedTiles.FindIndex(t => t.TilePoint == unit.Info.TileMapPosition) != -1)
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

            SlowDebuff slowDebuff = new SlowDebuff(SelectedUnit, _slowDuration, _slowMultiplier);

            Casted();
        }
    }
}
