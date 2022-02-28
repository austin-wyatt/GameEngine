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
using MortalDungeon.Game.Abilities;
using MortalDungeon.Game.Serializers;

namespace MortalDungeon.Game.Abilities
{
    public class StrongBones : Ability
    {
        public StrongBones(Unit castingUnit)
        {
            Type = AbilityTypes.Passive;
            DamageType = DamageType.NonDamaging;
            CastingUnit = castingUnit;

            CastingMethod |= CastingMethod.Passive | CastingMethod.BruteForce;

            Grade = 1;

            Name = new Serializers.TextInfo(7, 3);
            Description = new Serializers.TextInfo(8, 3);

            AnimationSet = new Serializers.AnimationSet();
            AnimationSet.Animations.Add(new Serializers.Animation()
            {
                FrameIndices = { (int)Character.s },
                Spritesheet = (int)TextureName.CharacterSpritesheet
            });

            AbilityClass = AbilityClass.Skeleton;
        }

        private StrongBonesBuff _strongBonesBuff = null;

        public override void ApplyPassives()
        {
            base.ApplyPassives();

            _strongBonesBuff = new StrongBonesBuff();

            CastingUnit.Info.AddBuff(_strongBonesBuff);
        }

        public override void RemovePassives()
        {
            base.RemovePassives();

            CastingUnit.Info.RemoveBuff(_strongBonesBuff);
        }
    }
}

namespace MortalDungeon.Definitions.Buffs
{
    public class StrongBonesBuff : Buff
    {
        private int PotencyIncrease = 2; //per skeleton ability
        private int DamageBlockIncrease = 2; //per skeleton ability

        public StrongBonesBuff()
        {
            Invisible = false;
        }
        public StrongBonesBuff(Buff buff) : base(buff) { }

        protected override void AssignAnimationSet()
        {
            base.AssignAnimationSet();

            AnimationSet = AnimationSetManager.GetAnimationSet(54);
        }

        public override void AddEventListeners()
        {
            base.AddEventListeners();

            Unit.PreDamageInstanceAppliedSource += ModifyDamageInstance;
            Unit.AbilitiesUpdated += OnAbilitiesUpdated;
        }

        public override void RemoveEventListeners()
        {
            base.RemoveEventListeners();

            Unit.PreDamageInstanceAppliedSource -= ModifyDamageInstance;
            Unit.AbilitiesUpdated -= OnAbilitiesUpdated;
        }

        private void ModifyDamageInstance(Unit unit, DamageInstance instance)
        {
            int skeletonTypeAbilities = 0;

            unit.Info.Abilities.ForEach(ability =>
            {
                if (ability.AbilityClass == AbilityClass.Skeleton)
                {
                    skeletonTypeAbilities++;
                }
            });

            if (instance.AbilityClass == AbilityClass.Skeleton)
            {
                var keys = instance.Damage.Keys.ToArray();

                for (int i = 0; i < keys.Length; i++)
                {
                    if (instance.Damage[keys[i]] > 0)
                    {
                        instance.Damage[keys[i]] += PotencyIncrease * skeletonTypeAbilities;
                    }
                }
            }
        }

        private void OnAbilitiesUpdated(Unit unit)
        {
            int skeletonTypeAbilities = 0;

            unit.Info.Abilities.ForEach(ability =>
            {
                if (ability.AbilityClass == AbilityClass.Skeleton)
                {
                    skeletonTypeAbilities++;
                }
            });

            SetBuffEffect(BuffEffect.ShieldBlockAdditive, skeletonTypeAbilities * DamageBlockIncrease);
        }
    }
}
