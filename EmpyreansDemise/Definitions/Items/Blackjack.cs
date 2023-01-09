using Empyrean.Definitions.Buffs;
using Empyrean.Game;
using Empyrean.Game.Abilities;
using Empyrean.Game.Abilities.AbilityEffects;
using Empyrean.Game.Abilities.SelectionTypes;
using Empyrean.Game.Items;
using Empyrean.Game.Serializers;
using Empyrean.Game.Units;
using Empyrean.Game.Units.AIFunctions;
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

        public override void InitializeAbility(bool fromLoad = false, Action initializeCallback = null)
        {
            ItemAbility = new Club(EquipmentHandle.Unit)
            {
                AnimationSet = AnimationSet,
            };

            base.InitializeAbility(fromLoad, initializeCallback);
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
            private string _buffIdentifier;

            public Club(Unit unit) : base(null, AbilityClass.Item_Normal, 1)
            {
                Name = new TextInfo(4, 1);
                Description = new TextInfo(5, 1);

                CastingUnit = unit;
                CastingMethod = CastingMethod.Base | CastingMethod.BruteForce | CastingMethod.Weapon;

                CastRequirements.AddResourceCost(ResF.ActionEnergy, 1, Comparison.GreaterThanOrEqual, ExpendBehavior.Expend);
                CastRequirements.AddResourceCost(ResI.Stamina, 1, Comparison.GreaterThanOrEqual, ExpendBehavior.Expend);

                EffectManager = new EffectManager(this);

                ApplyDamage initialDamage = new ApplyDamage(new TargetInformation(AbilityUnitTarget.SelectedUnit))
                {
                    CreateDamageInstance = CreateDamageInstance
                };

                _buffIdentifier = "blackjack_slow_debuff";

                StackingDebuff slowDebuff = new StackingDebuff()
                {
                    StackDuration = 3,
                    Duration = -1,
                    OwnerId = CastingUnit.PermanentId,
                    Identifier = _buffIdentifier,
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

                InitializeAIValues();
            }

            private DamageInstance CreateDamageInstance()
            {
                DamageInstance instance = new DamageInstance();
                instance.Damage.Add(DamageType.Blunt, 4);

                return instance;
            }

            private void InitializeAIValues()
            {
                SingleTarget.GenerateDefaultTargetInfoForAbility(this);

                AITargetSelection.EvaluateSimpleWeight = (morsel) =>
                {
                    if (!SelectionInfo.UnitTargetParams.CheckUnit(morsel.Unit, CastingUnit))
                    {
                        return 0;
                    }

                    var relation = morsel.Team.GetRelation(CastingUnit.AI.GetTeam());
                    float weight = 1.5f;
                    //To avoid a hardcoded weight value, an ability summary class should be created that gets manually filled with 
                    //the effects of the ability and provides general numbers based on intrinsic weights. Ie doing 1 damage is 0.1 weight,
                    //adding a slow would be 0.05 weight * duration * intensity, etc

                    //Then costs would add negative weight. Stamina would be major, action points would be minor, and movement energy would be negligible

                    //This more in-depth weight evaluation should happen in the feasibility check since that will always happen assuming targets are valid

                    switch (relation)
                    {
                        case Relation.Hostile:
                            break;
                        default:
                            return 0;
                    }

                    Buff foundBuff = morsel.Unit.Info.BuffManager.Buffs.Find(b => b.Identifier == _buffIdentifier);

                    if (foundBuff != null)
                    {
                        weight += foundBuff.Stacks * 0.05f;
                    }

                    weight *= 1 + CastingUnit.AI.Feelings.GetFeelingValue(FeelingType.Bloodthirst, morsel);

                    return weight;
                };
            }
        }
    }
}
