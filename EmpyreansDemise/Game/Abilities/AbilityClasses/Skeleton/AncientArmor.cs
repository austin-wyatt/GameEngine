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

namespace Empyrean.Game.Abilities
{
    public class AncientArmor : TemplateRangedSingleTarget
    {
        private int ShieldsGained = 1;
        public AncientArmor(Unit castingUnit) : base(castingUnit)
        {
            Type = AbilityTypes.BuffDefensive;
            CastingUnit = castingUnit;

            Grade = 2;

            CastRequirements.AddResourceCost(ResF.ActionEnergy, 1, Comparison.GreaterThanOrEqual, ExpendBehavior.Expend);
            ChargeRechargeCost = 30;

            MaxCharges = 2;
            Charges = 2;

            WeightParams.AllyWeight = 1;

            SelectionInfo.UnitTargetParams.Self = UnitCheckEnum.SoftTrue;
            SelectionInfo.CanSelectTiles = false;

            SelectionInfo.UnitTargetParams.Dead = UnitCheckEnum.False;
            SelectionInfo.UnitTargetParams.IsFriendly = UnitCheckEnum.False;
            SelectionInfo.UnitTargetParams.IsHostile = UnitCheckEnum.False;
            SelectionInfo.UnitTargetParams.IsNeutral = UnitCheckEnum.False;

            AbilityClass = AbilityClass.Skeleton;

            Name = new Serializers.TextInfo(1, 3);
            Description = new Serializers.TextInfo(2, 3);

            AnimationSet = new Serializers.AnimationSet();
            AnimationSet.Animations.Add(new Serializers.Animation()
            {
                FrameIndices = { (int)Character.A },
                Spritesheet = (int)TextureName.CharacterSpritesheet
            });
        }

        public override void EnactEffect()
        {
            BeginEffect();

            Sound sound = new Sound(Sounds.Select) { Gain = 0.5f, Pitch = GlobalRandom.NextFloat(0.75f, 0.8f) };
            sound.Play();

            Casted();

            if (CastingUnit.GetResI(ResI.Shields) > 0)
            {
                CastingUnit.SetShields(CastingUnit.GetResI(ResI.Shields) + ShieldsGained);
            }
            else 
            {
                CastingUnit.SetShields(ShieldsGained);
            }

            EffectEnded();
        }
    }
}
