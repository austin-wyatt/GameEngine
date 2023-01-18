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
using Empyrean.Game.Abilities;
using Empyrean.Game.Serializers;
using System.Threading.Tasks;

namespace Empyrean.Game.Abilities
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

            Name = TextEntry.GetTextEntry(7); //7  3
            Description = TextEntry.GetTextEntry(8); //8  3

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

namespace Empyrean.Definitions.Buffs
{
    public class StrongBonesBuff : Buff
    {
        private int PotencyIncrease = 2; //per skeleton ability
        private int DamageBlockIncrease = 2; //per skeleton ability

        public StrongBonesBuff()
        {
            Invisible = false;
        }

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

        private async Task OnAbilitiesUpdated(Unit unit)
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
