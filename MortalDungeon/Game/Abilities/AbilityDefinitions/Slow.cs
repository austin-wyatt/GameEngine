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
    internal class Slow : Ability
    {
        float _slowMultiplier;
        int _slowDuration;

        internal Slow(Unit castingUnit, int range = 1, float slowAmount = 0.25f, int duration = 3)
        {
            Type = AbilityTypes.Debuff;
            Range = range;
            CastingUnit = castingUnit;

            _slowDuration = duration;
            _slowMultiplier = 1 + slowAmount;

            Name = "Slow";

            CanTargetGround = false;

            Icon = new Icon(Icon.DefaultIconSize, IconSheetIcons.SpiderWeb, Spritesheets.IconSheet, true, Icon.BackgroundType.DebuffBackground);
        }

        internal override List<BaseTile> GetValidTileTargets(TileMap tileMap, List<Unit> units = default, BaseTile position = null)
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

            TargetAffectedUnits();

            return validTiles;
        }

        internal override bool OnUnitClicked(Unit unit)
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

        internal override void OnCast()
        {
            Scene.EnergyDisplayBar.HoverAmount(0);
            Scene.ActionEnergyBar.HoverAmount(0);

            float energyCost = GetEnergyCost();
            float actionEnergyCost = GetActionEnergyCost();

            //special cases for energy reduction go here

            Scene.EnergyDisplayBar.AddEnergy(-energyCost);
            Scene.ActionEnergyBar.AddEnergy(-actionEnergyCost);

            TileMap.DeselectTiles();

            base.OnCast();
        }

        internal override void EnactEffect()
        {
            base.EnactEffect();

            SlowDebuff slowDebuff = new SlowDebuff(SelectedUnit, _slowDuration, _slowMultiplier);

            Casted();
            EffectEnded();
        }
    }
}
