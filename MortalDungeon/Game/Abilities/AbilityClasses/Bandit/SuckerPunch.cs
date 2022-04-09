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
using MortalDungeon.Definitions.Buffs;

namespace MortalDungeon.Game.Abilities
{
    public class SuckerPunch : TemplateRangedSingleTarget
    {
        public SuckerPunch(Unit castingUnit) : base(castingUnit)
        {
            Type = AbilityTypes.MeleeAttack;
            DamageType = DamageType.Blunt;
            Range = 1;
            CastingUnit = castingUnit;
            Damage = 10;
            ActionCost = 3;

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
        }

        public override void EnactEffect()
        {
            BeginEffect();

            var damageParams = SelectedUnit.ApplyDamage(new DamageParams(GetDamageInstance()) { Ability = this });

            if (damageParams.ActualDamageDealt >= GetDamage()) 
            {
                SelectedUnit.Info.AddBuff(new StunDebuff() { Duration = 3 });
            }

            Casted();
            EffectEnded();
        }

        public override DamageInstance GetDamageInstance()
        {
            DamageInstance instance = new DamageInstance();


            float damageAmount = GetDamage();

            instance.Damage.Add(DamageType, damageAmount);

            return instance;
        }
    }
}
