using Empyrean.Definitions.Buffs;
using Empyrean.Engine_Classes.UIComponents;
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
    public class Dagger_1 : Item
    {
        public static int ID = 1;

        public Dagger_1() : base()
        {
            Id = ID;

            ItemType = ItemType.Weapon;

            Tags = ItemTag.Weapon_OneHanded | ItemTag.Weapon_Melee | ItemTag.Weapon_Concealed | ItemTag.Weapon_Slashing | ItemTag.Weapon_Thrusting;

            Name = new TextInfo(1, 1);
            Description = new TextInfo(3, 1)
            {
                TextReplacementParameters = new TextReplacementParameter[]
                {
                    new TextReplacementParameter()
                    {
                        Key = "damage",
                        Value = () => 1.ToString()
                    }
                }
            };
        }

        public Dagger_1(Item item) : base(item) 
        {

        }

        public override void InitializeAbility()
        {
            ItemAbility = new Stab(EquipmentHandle.Unit)
            {
                AnimationSet = AnimationSet
            };

            base.InitializeAbility();
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
        public Stab(Unit unit) : base(null, AbilityClass.Item_Normal, 1)
        {
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

            Dagger_CoupDeGraceDebuff debuff = new Dagger_CoupDeGraceDebuff()
            {
                Identifier = "dagger_coup_de_grace_" + CastingUnit.PermanentId.Id,
            };

            ApplyBuff coupDeGraceBuff = new ApplyBuff(debuff, new TargetInformation(AbilityUnitTarget.SelectedUnit)) 
            {
                StackIfPresent = true,
            };

            //ChainCondition secondaryDamageCheck = new ChainCondition("({TargetUnit ResI Shields} < 0) || ({TargetUnit Species} == 5)");

            //secondaryDamageCheck.ChainedEffect = new ModifyResI(ResOperation.Subtract, AbilityUnitTarget.TargetUnit, ResI.Stamina, GetResourceValue);

            //initialDamage.AddChainCondition(secondaryDamageCheck);
            

            EffectManager.Effects.Add(initialDamage);
            EffectManager.Effects.Add(coupDeGraceBuff);
        }

        private DamageInstance CreateDamageInstance()
        {
            DamageInstance instance = new DamageInstance();
            instance.Damage.Add(DamageType.Piercing, 1);
            instance.PiercingPercent = 1;

            return instance;
        }

        private int GetResourceValue()
        {
            return 1;
        }
    }
}
