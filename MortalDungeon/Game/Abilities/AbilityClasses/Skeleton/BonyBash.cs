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
    public class BonyBash : Ability
    {
        public BonyBash(Unit castingUnit, int range = 1, float damage = 3)
        {
            Type = AbilityTypes.MeleeAttack;
            DamageType = DamageType.Blunt;
            Range = range;
            CastingUnit = castingUnit;
            Damage = damage;
            ActionCost = 3;

            CastingMethod |= CastingMethod.BruteForce | CastingMethod.Weapon;

            Grade = 1;

            MaxCharges = 0;
            Charges = 0;
            ChargeRechargeCost = 0;

            BasicAbility = true;

            Name = "Bony Bash";

            _description = "Bop.";

            Icon = new Icon(Icon.DefaultIconSize, Character.B, Spritesheets.CharacterSheet, true);

            AbilityClass = AbilityClass.Skeleton;
        }

        public override List<BaseTile> GetValidTileTargets(TileMap tileMap, List<Unit> units = default, BaseTile position = null)
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

        public override bool UnitInRange(Unit unit, BaseTile position = null)
        {
            if (position == null)
            {
                position = CastingUnit.Info.TileMapPosition;
            }

            GetValidTileTargets(unit.GetTileMap(), new List<Unit> { unit }, position);

            return AffectedUnits.Exists(u => u.ObjectID == unit.ObjectID);
        }

        public override bool OnUnitClicked(Unit unit)
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

        public override void EnactEffect()
        {
            base.EnactEffect();

            SelectedUnit.ApplyDamage(new Unit.DamageParams(GetDamageInstance()) { Ability = this });

            Casted();
            EffectEnded();
        }

        public override DamageInstance GetDamageInstance()
        {
            DamageInstance instance = new DamageInstance();

            int skeletonTypeAbilities = 0;

            CastingUnit.Info.Abilities.ForEach(ability =>
            {
                if (ability.AbilityClass == AbilityClass.Skeleton)
                {
                    skeletonTypeAbilities++;
                }
            });

            float damageAmount = GetDamage() * skeletonTypeAbilities;

            instance.Damage.Add(DamageType, damageAmount);

            ApplyBuffDamageInstanceModifications(instance);
            return instance;
        }
    }
}
