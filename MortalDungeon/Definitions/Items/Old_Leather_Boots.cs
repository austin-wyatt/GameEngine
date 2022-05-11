using Empyrean.Definitions.Abilities;
using Empyrean.Definitions.Buffs;
using Empyrean.Game.Abilities;
using Empyrean.Game.Items;
using Empyrean.Game.Serializers;
using Empyrean.Game.Units;
using Empyrean.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Definitions.Items
{
    public class Old_Leather_Boots : Item
    {
        public static int ID = 2;

        public Old_Leather_Boots()
        {
            Id = ID;


            ItemType = ItemType.Boots;

            Tags = ItemTag.Armor_Light;
        }

        public override void InitializeAbility()
        {
            ItemAbility = new Item_Passive_Ability()
            {
                AnimationSet = AnimationSet,
                BuffEffects = new Dictionary<int, float>()
                    {
                        { (int)BuffEffect.MaxMovementEnergyAdditive, 1 }
                    },
                PassiveName = "Old_Leather_Boots_Passive"
            };

            base.InitializeAbility();
        }

        public override void BuildAnimationSet()
        {
            base.BuildAnimationSet();

            AnimationSet = new AnimationSet();

            Animation anim = new Animation();

            anim.Spritesheet = (int)TextureName.ItemSpritesheet_1;
            anim.FrameIndices = new List<int>() { (int)Item_1.Boots };

            AnimationSet.Animations.Add(anim);
        }
    }
}
