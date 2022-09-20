using Empyrean.Engine_Classes;
using Empyrean.Engine_Classes.UIComponents;
using Empyrean.Game.Abilities.SelectionTypes;
using Empyrean.Game.Map;
using Empyrean.Game.Tiles;
using Empyrean.Game.Units;
using Empyrean.Game.Units.AIFunctions;
using Empyrean.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Empyrean.Game.Abilities
{
    public partial class TemplateRangedSingleTarget : Ability
    {
        public TemplateRangedSingleTarget(Unit castingUnit, AbilityClass abilityClass = AbilityClass.Unknown, int range = 1, float damage = 0)
        {
            Type = AbilityTypes.MeleeAttack;
            DamageType = DamageType.Slashing;
            Range = range;
            CastingUnit = castingUnit;
            //CastRequirements.AddResourceCost(ResF.ActionEnergy, 1, Comparison.GreaterThanOrEqual, ExpendBehavior.Expend);

            CastingMethod |= CastingMethod.BruteForce | CastingMethod.Weapon;

            Grade = 1;

            MaxCharges = 0;
            Charges = 0;
            ChargeRechargeCost = 0;

            AnimationSet = new Serializers.AnimationSet();
            AnimationSet.Animations.Add(new Serializers.Animation()
            {
                FrameIndices = { (int)Character.T },
                Spritesheet = (int)TextureName.CharacterSpritesheet
            });

            AbilityClass = abilityClass;

            SelectionInfo = new SingleTarget(this);

            SelectionInfo.UnitTargetParams = new UnitSearchParams()
            {
                Dead = UnitCheckEnum.False,
                IsFriendly = UnitCheckEnum.SoftTrue,
                IsHostile = UnitCheckEnum.SoftTrue,
                IsNeutral = UnitCheckEnum.SoftTrue,
                Self = UnitCheckEnum.False,
                InVision = UnitCheckEnum.True,
            };
        }

        public override DamageInstance GetDamageInstance()
        {
            DamageInstance instance = new DamageInstance();

            //instance.Damage.Add(DamageType, GetDamage());
            instance.Damage.Add(DamageType, 0);

            return instance;
        }
    }
}
