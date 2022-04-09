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
    public class MendBones : TemplateRangedSingleTarget
    {
        public MendBones(Unit castingUnit) : base(castingUnit)
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

            WeightParams.AllyWeight = 1;

            MaxCharges = 3;
            Charges = 3;

            UnitTargetParams.Self = UnitCheckEnum.SoftTrue;
            CanTargetGround = false;

            UnitTargetParams.Dead = UnitCheckEnum.False;
            UnitTargetParams.IsFriendly = UnitCheckEnum.SoftTrue;
            UnitTargetParams.IsHostile = UnitCheckEnum.SoftTrue;
            UnitTargetParams.IsNeutral = UnitCheckEnum.SoftTrue;
            UnitTargetParams.Self = UnitCheckEnum.SoftTrue;

            AbilityClass = AbilityClass.Skeleton;

            Name = new Serializers.TextInfo(5, 3);
            Description = new Serializers.TextInfo(6, 3);

            AnimationSet = new Serializers.AnimationSet();
            AnimationSet.Animations.Add(new Serializers.Animation()
            {
                FrameIndices = { (int)Character.M },
                Spritesheet = (int)TextureName.CharacterSpritesheet
            });
        }

        public override void EnactEffect()
        {
            Sound sound = new Sound(Sounds.Select) { Gain = 0.75f, Pitch = GlobalRandom.NextFloat(0.95f, 1.05f) };
            sound.Play();

            Casted();


            //arrow.SetScale(1 / WindowConstants.AspectRatio, 1, 1);


            DamageInstance healing = GetDamageInstance();

            SelectedUnit.ApplyDamage(new DamageParams(healing) { Ability = this });

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

            return instance;
        }
    }
}
