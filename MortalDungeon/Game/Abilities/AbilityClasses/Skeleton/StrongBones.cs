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
    internal class StrongBones : Ability
    {
        internal StrongBones(Unit castingUnit)
        {
            Type = AbilityTypes.Passive;
            DamageType = DamageType.NonDamaging;
            CastingUnit = castingUnit;

            CastingMethod |= CastingMethod.Passive | CastingMethod.BruteForce;

            Grade = 1;

            Name = "Strong Bones";

            _description = "Sufficient calcium has been achieved.";

            Icon = new Icon(Icon.DefaultIconSize, Character.s, Spritesheets.CharacterSheet, true);

            AbilityClass = AbilityClass.Skeleton;

            CastingUnit.Info.AddBuff(new StrongBonesBuff(castingUnit));
        }
    }

    internal class StrongBonesBuff : Buff
    {
        private int PotencyIncrease = 2; //per skeleton ability
        private int DamageBlockIncrease = 2; //per skeleton ability

        internal StrongBonesBuff(Unit affected) : base(affected)
        {
            Name = "StrongBonesBuff";
            BuffType = BuffType.Buff;

            Icon = new Icon(Icon.DefaultIconSize, IconSheetIcons.QuestionMark, Spritesheets.IconSheet);

            IndefiniteDuration = true;
            Hidden = true;
        }

        internal override void ModifyDamageInstance(DamageInstance instance, Ability ability)
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

        internal override float ModifyShieldBlockAdditive(Unit unit)
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
