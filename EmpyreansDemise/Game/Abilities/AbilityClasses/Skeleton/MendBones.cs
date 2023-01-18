using Empyrean.Engine_Classes.Scenes;
using Empyrean.Game.Tiles;
using Empyrean.Game.Units;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Empyrean.Engine_Classes.UIComponents;
using Empyrean.Objects;
using OpenTK.Mathematics;
using Empyrean.Game.Map;
using System.Diagnostics;
using Empyrean.Engine_Classes.Audio;
using Empyrean.Engine_Classes;
using Empyrean.Game.Serializers;

namespace Empyrean.Game.Abilities
{
    public class MendBones : TemplateRangedSingleTarget
    {
        public MendBones(Unit castingUnit) : base(castingUnit)
        {
            Type = AbilityTypes.Heal;
            DamageType = DamageType.Healing;
            Range = 3;
            CastingUnit = castingUnit;

            CastingMethod |= CastingMethod.Intelligence | CastingMethod.PhysicalDexterity;

            Grade = 1;

            CastRequirements.AddResourceCost(ResF.ActionEnergy, 1, Comparison.GreaterThanOrEqual, ExpendBehavior.Expend);
            ChargeRechargeCost = 50;

            WeightParams.AllyWeight = 1;

            MaxCharges = 3;
            Charges = 3;

            SelectionInfo.CanSelectTiles = false;

            SelectionInfo.UnitTargetParams.Dead = UnitCheckEnum.False;
            SelectionInfo.UnitTargetParams.IsFriendly = UnitCheckEnum.SoftTrue;
            SelectionInfo.UnitTargetParams.IsHostile = UnitCheckEnum.SoftTrue;
            SelectionInfo.UnitTargetParams.IsNeutral = UnitCheckEnum.SoftTrue;
            SelectionInfo.UnitTargetParams.Self = UnitCheckEnum.SoftTrue;

            AbilityClass = AbilityClass.Skeleton;

            Name = TextEntry.GetTextEntry(5); //5  3 
            Description = TextEntry.GetTextEntry(6); //6  3 

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

            for(int i = 0; i < SelectionInfo.SelectedUnits.Count; i++)
            {
                SelectionInfo.SelectedUnits[i].ApplyDamage(new DamageParams(healing) { Ability = this });
            }

            EffectEnded();
        }

        public override DamageInstance GetDamageInstance()
        {
            DamageInstance instance = new DamageInstance();

            int skeletonTypeAbilities = 0;

            if(SelectionInfo.SelectedUnits.Count > 0) 
            {
                SelectionInfo.SelectedUnits[0].Info.Abilities.ForEach(ability =>
                {
                    if (ability.AbilityClass == AbilityClass.Skeleton)
                    {
                        skeletonTypeAbilities++;
                    }
                });
            }

            float healAmount = skeletonTypeAbilities * 10;

            instance.Damage.Add(DamageType, healAmount);

            return instance;
        }
    }
}
