using Empyrean.Definitions.Buffs;
using Empyrean.Game.Abilities;
using Empyrean.Game.Abilities.AbilityEffects;
using Empyrean.Game.Items;
using Empyrean.Game.Serializers;
using Empyrean.Game.Units;
using Empyrean.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Definitions.Items
{
    public class Blackjack : Item
    {
        public static int ID = 4;

        public Blackjack() : base()
        {
            Id = ID;

            ItemType = ItemType.Weapon;

            Tags = ItemTag.Weapon_OneHanded | ItemTag.Weapon_Melee | ItemTag.Weapon_Concealed | ItemTag.Weapon_Blunt;

            //Name = new TextInfo(1, 1);
            //Description = new TextInfo(3, 1)
            //{
            //    TextReplacementParameters = new TextReplacementParameter[]
            //    {
            //        new TextReplacementParameter()
            //        {
            //            Key = "damage",
            //            Value = () => 1.ToString()
            //        }
            //    }
            //};
        }

        public Blackjack(Item item) : base(item)
        {

        }

        public override void InitializeAbility()
        {
            ItemAbility = new Club(EquipmentHandle.Unit)
            {
                AnimationSet = AnimationSet,
            };

            base.InitializeAbility();
        }

        public override void BuildAnimationSet()
        {
            base.BuildAnimationSet();

            AnimationSet = new AnimationSet();

            Animation anim = new Animation();

            anim.Spritesheet = (int)TextureName.ItemSpritesheet_1;
            anim.FrameIndices = new List<int>() { 3 };

            AnimationSet.Animations.Add(anim);
        }

        private class Club : TemplateRangedSingleTarget
        {
            public Club(Unit unit) : base(null, AbilityClass.Item_Normal, 1)
            {
                CastingUnit = unit;
                CastingMethod = CastingMethod.Base | CastingMethod.BruteForce | CastingMethod.Weapon;

                CastRequirements.AddResourceCost(ResF.ActionEnergy, 1, Comparison.GreaterThanOrEqual, ExpendBehavior.Expend);
                CastRequirements.AddResourceCost(ResI.Stamina, 1, Comparison.GreaterThanOrEqual, ExpendBehavior.Expend);

                EffectManager = new EffectManager(this);

                ApplyDamage initialDamage = new ApplyDamage(new TargetInformation(AbilityUnitTarget.SelectedUnit))
                {
                    CreateDamageInstance = CreateDamageInstance
                };

                StackingDebuff slowDebuff = new StackingDebuff()
                {
                    StackDuration = 3,
                    Duration = -1,
                    OwnerId = CastingUnit.PermanentId,
                    Identifier = "blackjack_slow_debuff",
                    RemoveOnZeroStacks = true,
                    Behavior = StackBehavior.TrackStackDurationSeparately,
                    AnimationSetId = 51,
                    AnimationSet = AnimationSetManager.GetAnimationSet(51)
                };

                slowDebuff.StackingValues.Add(new StackingValue()
                {
                    BuffEffect = BuffEffect.MovementEnergyCostMultiplier,
                    BaseValue = 0,
                    MultiplicativeAmountPerStack = 1.1f
                });

                ApplyBuff stackingSlowDebuff = new ApplyBuff(slowDebuff, new TargetInformation(AbilityUnitTarget.SelectedUnit))
                {
                    StackIfPresent = true,
                };
                

                EffectManager.Effects.Add(initialDamage);
                EffectManager.Effects.Add(stackingSlowDebuff);
            }

            private DamageInstance CreateDamageInstance()
            {
                DamageInstance instance = new DamageInstance();
                instance.Damage.Add(DamageType.Blunt, 4);

                return instance;
            }
        }
    }
}
