using Empyrean.Definitions.Buffs;
using Empyrean.Game.Abilities;
using Empyrean.Game.Serializers;
using Empyrean.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Definitions.Abilities
{
    public class Item_Passive_Ability : Ability
    {
        public Dictionary<int, float> BuffEffects = new Dictionary<int, float>();
        public string PassiveName = "";

        public Item_Passive_Ability()
        {
            Type = AbilityTypes.Passive;
            DamageType = DamageType.NonDamaging;

            CastingMethod |= CastingMethod.Equipped;

            Grade = 1;

            //Name = new Serializers.TextInfo(7, 3);
            //Description = new Serializers.TextInfo(8, 3);

            AnimationSet = new AnimationSet();

            Animation anim = new Animation();

            anim.Spritesheet = (int)TextureName.ItemSpritesheet_1;
            anim.FrameIndices = new List<int>() { (int)Item_1.Slingshot };

            AnimationSet.Animations.Add(anim);

            AbilityClass = AbilityClass.Item_Normal;
        }

        private Buff _itemBuff = null;

        public override void ApplyPassives()
        {
            base.ApplyPassives();

            _itemBuff = new GenericEffectBuff();
            _itemBuff.BuffEffects = BuffEffects;
            _itemBuff.Identifier = PassiveName;

            CastingUnit.Info.AddBuff(_itemBuff);
        }

        public override void RemovePassives()
        {
            base.RemovePassives();

            CastingUnit.Info.RemoveBuff(_itemBuff);
        }
    }
}
