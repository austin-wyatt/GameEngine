using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Game.Tiles;
using MortalDungeon.Game.Units;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using MortalDungeon.Engine_Classes.UIComponents;
using MortalDungeon.Objects;
using OpenTK.Mathematics;
using MortalDungeon.Game.Map;
using System.Diagnostics;
using MortalDungeon.Engine_Classes.Audio;
using MortalDungeon.Engine_Classes;

namespace MortalDungeon.Game.Abilities
{
    public class MendBones : Ability
    {
        public MendBones(Unit castingUnit)
        {
            Type = AbilityTypes.Heal;
            DamageType = DamageType.Healing;
            Range = 3;
            CastingUnit = castingUnit;
            Damage = 10;

            CastingMethod |= CastingMethod.Intelligence | CastingMethod.PhysicalDexterity;

            Grade = 1;

            ActionCost = 1;
            ChargeRechargeCost = 50;

            MaxCharges = 3;
            Charges = 3;

            CanTargetSelf = true;
            CanTargetGround = false;

            UnitTargetParams.Dead = UnitCheckEnum.False;
            UnitTargetParams.IsFriendly = UnitCheckEnum.SoftTrue;
            UnitTargetParams.IsHostile = UnitCheckEnum.SoftTrue;
            UnitTargetParams.IsNeutral = UnitCheckEnum.SoftTrue;
            UnitTargetParams.Self = UnitCheckEnum.SoftTrue;

            AbilityClass = AbilityClass.Skeleton;

            Name = new Serializers.TextInfo(5, 3);
            Description = new Serializers.TextInfo(6, 3);

            SetIcon(Character.M, Spritesheets.CharacterSheet);
        }

        public override List<BaseTile> GetValidTileTargets(TileMap tileMap, List<Unit> units = default, BaseTile position = null, List<Unit> validUnits = null)
        {
            base.GetValidTileTargets(tileMap);

            TilePoint point = position == null ? CastingUnit.Info.TileMapPosition.TilePoint : position.TilePoint;

            //List <BaseTile> validTiles = tileMap.GetTargetsInRadius(point, (int)Range, new List<TileClassification>(), units);
            List<Unit> unitsInCastRadius = VisionHelpers.GetUnitsInRadius(CastingUnit, units, (int)Range, Scene);

            TrimUnits(unitsInCastRadius, false, MinRange);

            TargetAffectedUnits();

            return new List<BaseTile>();
        }

        public override bool UnitInRange(Unit unit, BaseTile position = null)
        {
            TilePoint point = position == null ? CastingUnit.Info.TileMapPosition.TilePoint : position.TilePoint;

            if (CanTargetSelf && unit == CastingUnit)
                return true;

            bool inRange = false;

            if (TileMap.GetDistanceBetweenPoints(point, unit.Info.TileMapPosition) <= MinRange)
            {
                return false;
            }

            var tempVision = new TemporaryVisionParams()
            {
                Unit = CastingUnit,
                TemporaryPosition = point
            };

            if (VisionHelpers.PointInVision(unit.Info.TileMapPosition, CastingUnit.AI.Team, new List<TemporaryVisionParams> { tempVision }))
            {
                return true;
            }

            return inRange;
        }

        public override bool OnUnitClicked(Unit unit)
        {
            if (!base.OnUnitClicked(unit))
                return false;

            if (AffectedUnits.Contains(unit))
            {
                SelectedUnit = unit;
                EnactEffect();
            }

            return true;
        }

        public override void EnactEffect()
        {
            base.EnactEffect();

            Sound sound = new Sound(Sounds.Select) { Gain = 0.75f, Pitch = GlobalRandom.NextFloat(0.95f, 1.05f) };
            sound.Play();

            Casted();


            //arrow.SetScale(1 / WindowConstants.AspectRatio, 1, 1);


            DamageInstance healing = GetDamageInstance();

            SelectedUnit.ApplyDamage(new Unit.DamageParams(healing) { Ability = this });

            EffectEnded();
        }

        public override DamageInstance GetDamageInstance()
        {
            DamageInstance instance = new DamageInstance();

            int skeletonTypeAbilities = 0;

            if(SelectedUnit != null) 
            {
                SelectedUnit.Info.Abilities.ForEach(ability =>
                {
                    if (ability.AbilityClass == AbilityClass.Skeleton)
                    {
                        skeletonTypeAbilities++;
                    }
                });
            }

            float healAmount = skeletonTypeAbilities * Damage;

            instance.Damage.Add(DamageType, healAmount);

            ApplyBuffDamageInstanceModifications(instance);
            return instance;
        }
    }
}
