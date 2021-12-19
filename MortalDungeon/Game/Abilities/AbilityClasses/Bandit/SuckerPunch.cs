using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Game.Tiles;
using MortalDungeon.Game.Units;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using MortalDungeon.Engine_Classes.UIComponents;
using MortalDungeon.Objects;
using MortalDungeon.Engine_Classes;

namespace MortalDungeon.Game.Abilities
{
    internal class SuckerPunch : Ability
    {
        internal SuckerPunch(Unit castingUnit, int range = 1, float damage = 10)
        {
            Type = AbilityTypes.MeleeAttack;
            DamageType = DamageType.Blunt;
            Range = range;
            CastingUnit = castingUnit;
            Damage = damage;
            ActionCost = 3;

            CastingMethod |= CastingMethod.BruteForce | CastingMethod.Unarmed;

            Grade = 1;

            MaxCharges = 0;
            Charges = 0;
            ChargeRechargeCost = 0;

            Basic = true;

            Name = "Sucker Punch";

            _description = "Rude";

            Icon = new Icon(Icon.DefaultIconSize, Character.P, Spritesheets.CharacterSheet, true);

            AbilityClass = AbilityClass.Bandit;
        }

        internal override List<BaseTile> GetValidTileTargets(TileMap tileMap, List<Unit> units = default, BaseTile position = null)
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

            TrimTiles(validTiles, units);

            TargetAffectedUnits();

            return validTiles;
        }

        internal override bool UnitInRange(Unit unit, BaseTile position = null)
        {
            if (position == null)
            {
                position = CastingUnit.Info.TileMapPosition;
            }

            GetValidTileTargets(unit.GetTileMap(), new List<Unit> { unit }, position);

            return AffectedUnits.Exists(u => u.ObjectID == unit.ObjectID);
        }

        internal override bool OnUnitClicked(Unit unit)
        {
            if (!base.OnUnitClicked(unit))
                return false;

            if (AffectedTiles.FindIndex(t => t.TilePoint == unit.Info.TileMapPosition) != -1 && UnitTargetParams.CheckUnit(unit, CastingUnit))
            {
                SelectedUnit = unit;
                EnactEffect();
            }

            return true;
        }

        internal override void EnactEffect()
        {
            base.EnactEffect();

            var damageParams = SelectedUnit.ApplyDamage(new Unit.DamageParams(GetDamageInstance()) { Ability = this });

            if (damageParams.ActualDamageDealt >= GetDamage()) 
            {
                SelectedUnit.Info.AddBuff(new StunDebuff(SelectedUnit, 3));
            }

            Casted();
            EffectEnded();
        }

        internal override DamageInstance GetDamageInstance()
        {
            DamageInstance instance = new DamageInstance();


            float damageAmount = GetDamage();

            instance.Damage.Add(DamageType, damageAmount);

            ApplyBuffDamageInstanceModifications(instance);
            return instance;
        }
    }
}
