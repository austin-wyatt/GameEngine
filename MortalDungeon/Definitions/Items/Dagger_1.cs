using MortalDungeon.Engine_Classes.UIComponents;
using MortalDungeon.Game.Abilities;
using MortalDungeon.Game.Items;
using MortalDungeon.Game.Units;
using MortalDungeon.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Definitions.Items
{
    public class Dagger_1 : Item
    {
        public Dagger_1()
        {
            Id = 1;

            ItemAbility = new Stab();

            ValidEquipmentSlots = EquipmentSlot.Weapon_1;
        }

        public Dagger_1(Item item) : base(item) 
        {
            
        }
    }

    public class Stab : TemplateRangedSingleTarget
    {
        public Stab() : base(null, AbilityClass.Item_Normal, 1, 5)
        {
            DamageType = DamageType.Piercing;

            CastingMethod = CastingMethod.Base | CastingMethod.PhysicalDexterity | CastingMethod.Weapon;

            Name = "Stab";

            _description = "A template ability for variable range single target abilities.";

            Icon = new Icon(Icon.DefaultIconSize, IconSheetIcons.BleedingDagger, Spritesheets.IconSheet, true);
        }
    }
}
