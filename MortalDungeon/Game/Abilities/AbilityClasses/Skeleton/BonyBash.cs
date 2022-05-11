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
using Empyrean.Game.Units.AIFunctions;

namespace Empyrean.Game.Abilities
{
    public class BonyBash : TemplateRangedSingleTarget
    {
        public BonyBash(Unit castingUnit) : base(castingUnit)
        {
            Type = AbilityTypes.MeleeAttack;
            DamageType = DamageType.Blunt;
            Range = 1;
            CastingUnit = castingUnit;
            //Damage = 3;
            CastRequirements.AddResourceCost(ResF.ActionEnergy, 3, Comparison.GreaterThanOrEqual, ExpendBehavior.Expend);

            CastingMethod |= CastingMethod.BruteForce | CastingMethod.Weapon;

            WeightParams.EnemyWeight = 1;

            Grade = 1;

            MaxCharges = 0;
            Charges = 0;
            ChargeRechargeCost = 0;

            Name = new Serializers.TextInfo(3, 3);
            Description = new Serializers.TextInfo(4, 3);

            AnimationSet = new Serializers.AnimationSet();
            AnimationSet.Animations.Add(new Serializers.Animation()
            {
                FrameIndices = { (int)Character.B },
                Spritesheet = (int)TextureName.CharacterSpritesheet
            });

            AbilityClass = AbilityClass.Skeleton;
        }

        public override DamageInstance GetDamageInstance()
        {
            DamageInstance instance = new DamageInstance();

            int skeletonTypeAbilities = 0;

            CastingUnit.Info.Abilities.ForEach(ability =>
            {
                if (ability.AbilityClass == AbilityClass.Skeleton)
                {
                    skeletonTypeAbilities++;
                }
            });

            //float damageAmount = GetDamage() * skeletonTypeAbilities;
            float damageAmount = skeletonTypeAbilities;

            instance.Damage.Add(DamageType, damageAmount);

            return instance;
        }
        
    }
}
