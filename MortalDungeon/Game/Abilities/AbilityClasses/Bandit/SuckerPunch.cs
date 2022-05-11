using Empyrean.Engine_Classes.Scenes;
using Empyrean.Game.Tiles;
using Empyrean.Game.Units;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Empyrean.Engine_Classes.UIComponents;
using Empyrean.Objects;
using Empyrean.Engine_Classes;
using Empyrean.Definitions.Buffs;
using Empyrean.Game.Abilities.AbilityEffects;

namespace Empyrean.Game.Abilities
{
    public class SuckerPunch : TemplateRangedSingleTarget
    {
        public SuckerPunch(Unit castingUnit) : base(castingUnit)
        {
            Type = AbilityTypes.MeleeAttack;
            DamageType = DamageType.Blunt;
            Range = 1;
            CastingUnit = castingUnit;

            CastRequirements.AddResourceCost(ResF.ActionEnergy, 2, Comparison.GreaterThanOrEqual, ExpendBehavior.Expend);

            CastingMethod |= CastingMethod.BruteForce | CastingMethod.Unarmed;

            Grade = 1;

            MaxCharges = 0;
            Charges = 0;
            ChargeRechargeCost = 0;

            WeightParams.EnemyWeight = 1;


            Name = new Serializers.TextInfo(9, 3);
            Description = new Serializers.TextInfo(10, 3);

            AnimationSet = new Serializers.AnimationSet();
            AnimationSet.Animations.Add(new Serializers.Animation()
            {
                FrameIndices = { (int)Character.P },
                Spritesheet = (int)TextureName.CharacterSpritesheet
            });

            AbilityClass = AbilityClass.Bandit;

            WeightParams.WeightModifications.Add((weight, ability, morsel) =>
            {
                if(morsel.Shields <= 0 && !morsel.Unit.Info.StatusManager.CheckCondition(StatusCondition.Stunned))
                {
                    weight *= 2;
                }
                else
                {
                    weight *= 0.75f;
                }

                return weight;
            });

            EffectManager = new EffectManager(this);

            ChainCondition shieldsCheck = new ChainCondition("{TargetUnit[0] ResI Shields} <= 0");

            StunDebuff stunDebuff = new StunDebuff(2);

            shieldsCheck.ChainedEffect = new ApplyBuff(stunDebuff, new TargetInformation(AbilityUnitTarget.SelectedUnit));

            EffectManager.ChainConditions.Add(shieldsCheck);
        }
    }
}
