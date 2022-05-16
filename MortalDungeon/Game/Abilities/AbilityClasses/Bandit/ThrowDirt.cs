using Empyrean.Definitions.Buffs;
using Empyrean.Engine_Classes;
using Empyrean.Engine_Classes.Audio;
using Empyrean.Game.Abilities.AbilityEffects;
using Empyrean.Game.Abilities.SelectionTypes;
using Empyrean.Game.Particles;
using Empyrean.Game.Serializers;
using Empyrean.Game.Units;
using Empyrean.Objects;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Game.Abilities.AbilityClasses.Bandit
{
    public class ThrowDirt : Ability
    {
        public ThrowDirt(Unit castingUnit)
        {
            Type = AbilityTypes.Utility;
            DamageType = DamageType.NonDamaging;
            Range = 1;
            CastingUnit = castingUnit;
            //CastRequirements.AddResourceCost(ResF.ActionEnergy, 1, Comparison.GreaterThanOrEqual, ExpendBehavior.Expend);

            CastingMethod |= CastingMethod.PhysicalDexterity;

            Grade = 1;

            MaxCharges = 0;
            Charges = 0;
            ChargeRechargeCost = 0;

            AnimationSet = new AnimationSet();
            AnimationSet.Animations.Add(new Serializers.Animation()
            {
                FrameIndices = { (int)Character.D },
                Spritesheet = (int)TextureName.CharacterSpritesheet
            });

            AbilityClass = AbilityClass.Bandit;

            CastRequirements.AddResourceCost(ResF.ActionEnergy, 2, Comparison.GreaterThanOrEqual, ExpendBehavior.Expend);

            List<Vector3i> tilePattern = new List<Vector3i> 
            { 
                new Vector3i(1, -1, 0), new Vector3i(0, -1, 1), 
                new Vector3i(-1, 0, 1), new Vector3i(-2, 0, 2), new Vector3i(-1, -1, 2), 
                new Vector3i(0, -2, 2), new Vector3i(1, -2, 1), new Vector3i(2, -2, 0) 
            };

            DirectionalPattern selectionInfo = new DirectionalPattern(this, tilePattern);
            SelectionInfo = selectionInfo;

            SelectionInfo.CanSelectTiles = true;
            SelectionInfo.CanSelectUnits = false;

            HasHoverEffect = true;


            EffectManager = new EffectManager(this);

            GenericEffectBuff debuff = new GenericEffectBuff();
            debuff.SetDamageAdditive(DamageType.Slashing, -3);
            debuff.SetDamageAdditive(DamageType.Piercing, -3);
            debuff.SetDamageAdditive(DamageType.Blunt, -3);

            debuff.AnimationSet = AnimationSetManager.GetAnimationSet(69);

            debuff.Invisible = false;
            debuff.Duration = 1;
            debuff.BaseDuration = 1;

            debuff.Identifier = "throw_dirt_debuff";

            ApplyBuff damageDecrease = new ApplyBuff(debuff, new TargetInformation(AbilityUnitTarget.SelectedUnits))
            {
                BuffMustBeUnique = true
            };

            EffectManager.Effects.Add(damageDecrease);

            #region animations
            AbilityAnimation dirtAnimation = new AbilityAnimation();
            dirtAnimation.AnimAction = () =>
            {
                Sound sound = new Sound(Sounds.ThrowDirt)
                {
                    Gain = 0.05f
                };
                var pos = CastingUnit.BaseObject.BaseFrame._position;

                sound.SetPosition(pos.X, pos.Y, pos.Z);

                sound.Play();

                Spray.SprayParams sprayParams = Spray.SprayParams.DEFAULT;
                sprayParams.Direction = -MathHelper.DegreesToRadians(GMath.AngleOfDirection(selectionInfo._hoveredDirection)) - MathHelper.PiOver6;
                sprayParams.Speed = new FloatRange(22, 27);
                sprayParams.ParticleCount = 400;
                sprayParams.SweepAngle = MathHelper.PiOver2 + MathHelper.PiOver6;
                sprayParams.Life = new IntRange(24, 27);
                sprayParams.FeedRate = 20;
                sprayParams.MultiplicativeAcceleration = new Vector3(1f, 1f, 1);
                sprayParams.ParticleSize = 0.06f;
                sprayParams.ColorDelta = new Vector4(0, 0, 0, -0.01f);

                var spray = new Spray(CastingUnit._position + new Vector3(0, 0, 0.2f), new Vector4(0.62f, 0.53f, 0.15f, 1f), sprayParams);
                spray.OnFinish = () =>
                {
                    Scene._particleGenerators.Remove(spray);
                    Scene.HighFreqTick -= spray.Tick;
                };

                Scene._particleGenerators.Add(spray);
                Scene.HighFreqTick += spray.Tick;

                dirtAnimation.TaskHandle.SetResult(true);
            };

            damageDecrease.Animation = dirtAnimation;
            #endregion
        }
    }
}
