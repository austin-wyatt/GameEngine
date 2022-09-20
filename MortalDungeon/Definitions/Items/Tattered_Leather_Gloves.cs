using Empyrean.Definitions.Abilities;
using Empyrean.Game.Abilities;
using Empyrean.Game.Items;
using Empyrean.Game.Serializers;
using Empyrean.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Definitions.Items
{
    public class Tattered_Leather_Gloves : Item
    {
        public static int ID = 3;

        public Tattered_Leather_Gloves()
        {
            Id = ID;

            ItemType = ItemType.Gloves;

            Tags = ItemTag.Armor_Light;

            Name = new TextInfo(8, 1);
            Description = new TextInfo(9, 1);
        }

        public override void InitializeAbility(bool fromLoad = false, Action initializeCallback = null)
        {
            ItemAbility = new Item_Passive_Ability()
            {
                AnimationSet = AnimationSet,
                BuffEffects = new Dictionary<int, float>()
                    {
                        { (int)BuffEffect.DAMAGE_TYPE_ADDITIVE + (int)DamageType.Piercing, 1 },
                        { (int)BuffEffect.DAMAGE_TYPE_ADDITIVE + (int)DamageType.Slashing, 1 },
                        { (int)BuffEffect.DAMAGE_TYPE_ADDITIVE + (int)DamageType.Blunt, 1 },
                    },
                PassiveName = "Tattered_Leather_Gloves_Passive",
                Name = new TextInfo(8, 1),
                Description = new TextInfo(9, 1)
            };

            base.InitializeAbility(fromLoad, initializeCallback);
        }

        public override void BuildAnimationSet()
        {
            base.BuildAnimationSet();

            AnimationSet = new AnimationSet();

            Animation anim = new Animation();

            anim.Spritesheet = (int)TextureName.ItemSpritesheet_1;
            anim.FrameIndices = new List<int>() { (int)Item_1.Slingshot };

            AnimationSet.Animations.Add(anim);
        }
    }
}
