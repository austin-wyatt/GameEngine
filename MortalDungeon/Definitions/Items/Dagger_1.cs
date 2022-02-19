using MortalDungeon.Engine_Classes.UIComponents;
using MortalDungeon.Game.Abilities;
using MortalDungeon.Game.Items;
using MortalDungeon.Game.Serializers;
using MortalDungeon.Game.Units;
using MortalDungeon.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Definitions.Items
{
    public class Dagger_1 : Item
    {
        public static int ID = 1;

        public Dagger_1() : base()
        {
            Id = ID;

            if (WindowConstants.GameRunning)
            {
                ItemAbility = new Stab()
                {
                    Icon = new Icon(Icon.DefaultIconSize, AnimationSet.BuildAnimationsFromSet(), true)
                };
            }

            ItemType = ItemType.Weapon;

            Name = new TextInfo(1, 1);
            Description = new TextInfo(3, 1)
            {
                TextReplacementParameters =
                {
                    new TextReplacementParameter()
                    {
                        Key = "damage",
                        Value = () => ItemAbility.Damage.ToString()
                    }
                }
            };
        }

        public Dagger_1(Item item) : base(item) 
        {

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
        public Stab() : base(null, AbilityClass.Item_Normal, 1, 5)
        {
            DamageType = DamageType.Piercing;

            CastingMethod = CastingMethod.Base | CastingMethod.PhysicalDexterity | CastingMethod.Weapon;

            //Name = "Stab";

            //Description = "A template ability for variable range single target abilities.";
        }
    }
}
