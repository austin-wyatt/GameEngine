using Empyrean.Definitions.Buffs;
using Empyrean.Engine_Classes;
using Empyrean.Engine_Classes.MiscOperations;
using Empyrean.Engine_Classes.UIComponents;
using Empyrean.Game;
using Empyrean.Game.Abilities;
using Empyrean.Game.Abilities.AbilityEffects;
using Empyrean.Game.Abilities.SelectionTypes;
using Empyrean.Game.Combat;
using Empyrean.Game.Items;
using Empyrean.Game.Serializers;
using Empyrean.Game.Units;
using Empyrean.Game.Units.AIFunctions;
using Empyrean.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Animation = Empyrean.Game.Serializers.Animation;

namespace Empyrean.Definitions.Items
{
    public class Dagger_1 : Item
    {
        public static int ID = 1;

        public Dagger_1() : base()
        {
            Id = ID;

            ItemType = ItemType.Weapon;

            Tags = ItemTag.Weapon_OneHanded | ItemTag.Weapon_Melee | ItemTag.Weapon_Concealed | ItemTag.Weapon_Slashing | ItemTag.Weapon_Thrusting;

            Name = TextEntry.GetTextEntry(1); //1, 1
            Description = TextEntry.GetTextEntry(2, getUnique: true); //3, 1
            Description.AddFunctionFormatString(() => 1.ToString(), insertIndex: 0);
        }

        public Dagger_1(Item item) : base(item) 
        {

        }

        public override void InitializeAbility(bool fromLoad = false, Action initializeCallback = null)
        {
            ItemAbility = new Stab(EquipmentHandle.Unit)
            {
                AnimationSet = AnimationSet
            };

            base.InitializeAbility(fromLoad, initializeCallback);
        }

        public override void BuildAnimationSet()
        {
            base.BuildAnimationSet();

            AnimationSet = new AnimationSet();

            Animation anim = new Animation();

            anim.Spritesheet = (int)TextureName.ItemSpritesheet_1;
            anim.FrameIndices = new List<int>() { (int)Item_1.Dagger_1 };

            AnimationSet.Animations.Add(anim);
        }
    }

    public class Stab : TemplateRangedSingleTarget
    {
        private string _buffIdentifier;

        public Stab(Unit unit) : base(null, AbilityClass.Item_Normal, 1)
        {
            Name = TextEntry.GetTextEntry(3); //27, 3
            Description = TextEntry.GetTextEntry(4); //28, 3

            CastingUnit = unit;

            DamageType = DamageType.Piercing;

            CastingMethod = CastingMethod.Base | CastingMethod.PhysicalDexterity | CastingMethod.Weapon;

            VariableResourceCost resourceCost = new VariableResourceCost();
            resourceCost.ResourceCosts.Add(new CombinedResourceCost()
            {
                ResourceType = ResourceType.ResI,
                Field = ResI.Stamina,
                ResourceCost = new ResourceCost(1, Comparison.GreaterThanOrEqual, ExpendBehavior.Expend)
            });
            resourceCost.ResourceCosts.Add(new CombinedResourceCost()
            {
                ResourceType = ResourceType.ResF,
                Field = ResF.ActionEnergy,
                ResourceCost = new ResourceCost(1, Comparison.GreaterThanOrEqual, ExpendBehavior.Expend)
            });

            //The ability costs 1 stamina or 1 action point
            CastRequirements.VariableResourceCosts.Add(resourceCost);


            EffectManager = new EffectManager(this);

            ApplyDamage initialDamage = new ApplyDamage(new TargetInformation(AbilityUnitTarget.SelectedUnit))
            {
                CreateDamageInstance = CreateDamageInstance
            };

            _buffIdentifier = "dagger_coup_de_grace_" + CastingUnit.PermanentId.Id;

            Dagger_CoupDeGraceDebuff debuff = new Dagger_CoupDeGraceDebuff()
            {
                Identifier = _buffIdentifier,
                OwnerId = CastingUnit.PermanentId
            };

            ApplyBuff coupDeGraceBuff = new ApplyBuff(debuff, new TargetInformation(AbilityUnitTarget.SelectedUnit)) 
            {
                StackIfPresent = true,
            };
 
            
            EffectManager.Effects.Add(initialDamage);
            EffectManager.Effects.Add(coupDeGraceBuff);


            InitializeAIValues();
        }

        private DamageInstance CreateDamageInstance()
        {
            DamageInstance instance = new DamageInstance();
            instance.Damage.Add(DamageType.Piercing, 1);
            instance.PiercingPercent = 1;

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
                    weight += foundBuff.Stacks * 0.1f;
                }

                weight *= 1 + CastingUnit.AI.Feelings.GetFeelingValue(FeelingType.Bloodthirst, morsel);

                return weight;
            };
        }
    }
}
