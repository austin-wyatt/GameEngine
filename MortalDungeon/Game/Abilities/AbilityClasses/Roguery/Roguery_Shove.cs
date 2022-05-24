using Empyrean.Definitions.Buffs;
using Empyrean.Engine_Classes;
using Empyrean.Game.Abilities.AbilityEffects;
using Empyrean.Game.Abilities.SelectionTypes;
using Empyrean.Game.Movement;
using Empyrean.Game.Serializers;
using Empyrean.Game.Units;
using Empyrean.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Game.Abilities.AbilityClasses.Roguery
{
    public class Roguery_Shove : Ability
    {
        public Roguery_Shove(Unit castingUnit) : base(castingUnit)
        {
            Type = AbilityTypes.Repositioning;
            DamageType = DamageType.NonDamaging;
            Range = 1;
            CastingUnit = castingUnit;

            CastRequirements.AddResourceCost(ResF.ActionEnergy, 1, Comparison.GreaterThanOrEqual, ExpendBehavior.Expend);
            CastRequirements.AddResourceCost(ResI.Stamina, 1, Comparison.GreaterThanOrEqual, ExpendBehavior.Expend);

            CastingMethod |= CastingMethod.BruteForce | CastingMethod.Unarmed;

            Grade = 1;

            MaxCharges = 0;
            Charges = 0;
            ChargeRechargeCost = 0;

            Name = new TextInfo(23, 3);
            Description = new TextInfo(24, 3);


            //Name = new Serializers.TextInfo(9, 3);
            //Description = new Serializers.TextInfo(10, 3);

            AnimationSet = new Serializers.AnimationSet();
            AnimationSet.Animations.Add(new Serializers.Animation()
            {
                FrameIndices = { (int)Character.H },
                Spritesheet = (int)TextureName.CharacterSpritesheet
            });

            AbilityClass = AbilityClass.Roguery;
            

            #region Selection info
            HasHoverEffect = true;
            MultiSelectionType multi = new MultiSelectionType(this);
            SelectionInfo = multi;

            SingleTarget singleTarget = new SingleTarget(this);
            singleTarget.UnitTargetParams = new UnitSearchParams()
            {
                Dead = UnitCheckEnum.False,
                IsFriendly = UnitCheckEnum.SoftTrue,
                IsHostile = UnitCheckEnum.SoftTrue,
                IsNeutral = UnitCheckEnum.SoftTrue,
                Self = UnitCheckEnum.False,
                InVision = UnitCheckEnum.True,
            };

            multi.AddChainedSelectionInfo(singleTarget);

            RadialSelection radial = new RadialSelection(this)
            {
                MinMagnitude = 1f,
                MaxMagnitude = 1f,
                CapMagnitude = true
            };

            radial.Selected += () =>
            {
                radial.SourceTile = singleTarget.SelectedUnits[0].Info.TileMapPosition;
                radial.SelectedUnits.Add(singleTarget.SelectedUnits[0]);

                radial.Direction = GMath.AngleOfPoints(radial.SourceTile._position, CastingUnit.Info.TileMapPosition._position);
            };

            multi.AddChainedSelectionInfo(radial);
            #endregion


            #region Effect
            EffectManager = new EffectManager(this);

            ChainCondition staminaCheck = new ChainCondition();
            staminaCheck.ConditionFunc = (effectResults) =>
            {
                return SelectionInfo.SelectedUnit.GetResI(ResI.Stamina) <= CastingUnit.GetResI(ResI.Stamina);
            };

            TargetInformation targetInfo = new TargetInformation(AbilityUnitTarget.SelectedUnit);

            MoveEffect forcedMoveEffect = new MoveEffect(targetInfo);

            forcedMoveEffect.EffectEnacted += () =>
            {
                radial.RemoveVisualIndicators();
            };

            forcedMoveEffect.GetMoveContract = () =>
            {
                return MovementHelper.CalculateForcedMovement(radial.SourceTile, radial.CurrAngle, radial.CurrMagnitude);
            };

            staminaCheck.ChainedEffect = forcedMoveEffect;

            EffectManager.ChainConditions.Add(staminaCheck);

            const int STAMINA_REDUCTION = 1;
            ModifyResI reduceStaminaEffect = new ModifyResI(ResOperation.Subtract, ResI.Stamina, targetInfo,
                () => STAMINA_REDUCTION);
            forcedMoveEffect.AddAdjacentEffect(reduceStaminaEffect);

            GenericEffectBuff shoveNegativeEffects = new GenericEffectBuff()
            {
                Duration = 1,
                Invisible = false
            };
            shoveNegativeEffects.AnimationSet = AnimationSetManager.GetAnimationSet(66);

            shoveNegativeEffects.SetBuffEffect(BuffEffect.MaxMovementEnergyAdditive, -3);
            shoveNegativeEffects.Identifier = "Roguery_Shove_Debuff";

            ApplyBuff buffEffect = new ApplyBuff(shoveNegativeEffects, targetInfo)
            {
                BuffMustBeUnique = true,
            };
            forcedMoveEffect.AddAdjacentEffect(buffEffect);
            #endregion
        }
    }
}
