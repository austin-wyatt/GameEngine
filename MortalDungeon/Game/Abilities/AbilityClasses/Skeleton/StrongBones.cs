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

            SetIcon(Character.s, Spritesheets.CharacterSheet);

            AbilityClass = AbilityClass.Skeleton;
        }

        private StrongBonesBuff _strongBonesBuff = null;

        public override void ApplyPassives()
        {
            base.ApplyPassives();

            _strongBonesBuff = new StrongBonesBuff(CastingUnit);

            CastingUnit.Info.AddBuff(_strongBonesBuff);
        }

        public override void RemovePassives()
        {
            base.RemovePassives();

            CastingUnit.Info.RemoveBuff(_strongBonesBuff);
        }
    }

    public class StrongBonesBuff : Buff
    {
        private int PotencyIncrease = 2; //per skeleton ability
        private int DamageBlockIncrease = 2; //per skeleton ability

        public StrongBonesBuff(Unit affected) : base(affected)
        {
            Name = "StrongBonesBuff";
            BuffType = BuffType.Buff;

            Icon = new Icon(Icon.DefaultIconSize, IconSheetIcons.QuestionMark, Spritesheets.IconSheet);

            IndefiniteDuration = true;
            Hidden = true;
        }

        public override void ModifyDamageInstance(DamageInstance instance, Ability ability)
        {
            int skeletonTypeAbilities = 0;

            AffectedUnit.Info.Abilities.ForEach(ability =>
            {
                if (ability.AbilityClass == AbilityClass.Skeleton)
                {
                    skeletonTypeAbilities++;
                }
            });

            if (ability.AbilityClass == AbilityClass.Skeleton) 
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

        public override float ModifyShieldBlockAdditive(Unit unit)
        {
            int skeletonTypeAbilities = 0;

            AffectedUnit.Info.Abilities.ForEach(ability =>
            {
                if (ability.AbilityClass == AbilityClass.Skeleton)
                {
                    skeletonTypeAbilities++;
                }
            });

            return skeletonTypeAbilities * DamageBlockIncrease;
        }
    }
}
