using Empyrean.Engine_Classes;
using Empyrean.Engine_Classes.UIComponents;
using Empyrean.Game.Abilities;
using Empyrean.Game.Objects;
using Empyrean.Game.Player;
using Empyrean.Game.Serializers;
using Empyrean.Game.Units;
using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Game.Items
{
    public enum ItemLocation
    {
        None,
        Inventory,
        Equipment
    }

    public enum ItemStackError
    {
        None,
        MaximumStackSizeReached
    }

    public enum ItemUseEnum
    {
        OutsideCombat,
        InsideCombat,
        InUI
    }

    /// <summary>
    /// Denotes an item's usages
    /// </summary>
    [Flags]
    public enum ItemTag : long
    {
        None = 0,
        Weapon_Melee = 1 << 0,
        Weapon_Slashing = (1 << 1) | Weapon_Melee,
        Weapon_Blunt = (1 << 2) | Weapon_Melee,
        Weapon_Thrusting = (1 << 3) | Weapon_Melee,
        Weapon_OneHanded = 1 << 4,
        Weapon_TwoHanded = 1 << 5,
        Weapon_Concealed = 1 << 6, //Concealed weapons can't be seen by other teams when equipped in the secondary weapon slot
        Weapon_Shafted = (1 << 7) | Weapon_Melee,

        Weapon_Ranged = 1 << 8,
        Weapon_Throwing = (1 << 9) | Weapon_Ranged,
        Weapon_Shooting = (1 << 10) | Weapon_Ranged,

        //Different classifications of magic weapons.
        //Weak magic would be a simple enchantment while strong magic would be akin to a legendary item
        Weapon_WeakMagic = 1 << 11,
        Weapon_Magic = (1 << 12) | Weapon_WeakMagic,
        Weapon_StrongMagic = (1 << 13) | Weapon_Magic,

        //Denotes an item that can be used as a focus for magic energy. Think wand, staff, talisman, etc.
        Weapon_MagicFocus = 1 << 14,

        //Denotes an item that will be consumed on use but has charges that can be replenished
        Consumable_Charged = 1 << 15,
        //Denotes an infinite use consumable
        Consumable_Infinite = 1 << 16,
        //Denotes a consumable that will be deleted from the unit's inventory upon use
        Consumable_DestroyOnUse = 1 << 17,

        Consumable_Food = 1L << 18,
        Consumable_Potion = 1L << 19,
        Consumable_Magic = 1L << 20,
        Consumable_Explosive = 1L << 21,

        Armor_Heavy = 1L << 22,
        Armor_Medium = 1L << 23,
        Armor_Light = 1L << 24,
        Armor_Clothing = 1L << 25

    }

    public enum Affinity
    {

    }

    public class Item
    {
        public int Id;
        public TextEntry Name;
        public TextEntry Description;

        /// <summary>
        /// The modifier tag will allow items to have alternate stats/functions depending on the value of the tag
        /// </summary>
        public int Modifier = 0;

        public bool Stackable = false;
        public bool Unique = false;

        public bool Consumable = false;
        public bool UsableOutsideCombat = true;
        public bool UsableInCombat = true;

        public bool PlayerItem = true;

        public bool Sellable = false;
        public int SellPrice = 0;

        public int StackSize = 0;
        public int Charges = 0;

        public int MaxCharges;

        public int MaxEquipmentStack = 2;
        public int MaxInventoryStack = 999;

        public ItemLocation Location = ItemLocation.Inventory;
        public ItemType ItemType = ItemType.BasicItem;

        public ItemTag Tags = ItemTag.None;

        public Ability ItemAbility;

        public Equipment EquipmentHandle;

        public AnimationSet AnimationSet;


        public Item() 
        {
            BuildAnimationSet();
        }

        public Item(Item item)
        {
            Id = item.Id;
            Name = item.Name;
            Description = item.Description;
            Stackable = item.Stackable;
            Consumable = item.Consumable;
            UsableOutsideCombat = item.UsableOutsideCombat;
            UsableInCombat = item.UsableInCombat;
            StackSize = item.StackSize;
            Charges = item.Charges;
            MaxEquipmentStack = item.MaxEquipmentStack;
            MaxInventoryStack = item.MaxInventoryStack;
            Unique = item.Unique;
            Sellable = item.Sellable;
            SellPrice = item.SellPrice;
            MaxCharges = item.MaxCharges;

            SetModifier(item.Modifier);
            BuildAnimationSet();
        }

        public ItemStackError AddToStack(int amount)
        {
            int maxStackSize = Location == ItemLocation.Inventory ? MaxInventoryStack : MaxEquipmentStack;

            if(amount + StackSize > maxStackSize)
            {
                int overflowAmount = amount + StackSize - maxStackSize;

                StackSize = maxStackSize;

                if (PlayerItem)
                {
                    var item = Activator.CreateInstance(GetType(), new object[] {this}) as Item;

                    item.StackSize = overflowAmount;

                    PlayerParty.Inventory.AddItemToInventory(item);
                    return ItemStackError.MaximumStackSizeReached;
                }
            }
            else
            {
                StackSize += amount;
            }

            return ItemStackError.None;
        }

        public virtual void Use(ItemUseEnum useLocation)
        {
            switch (useLocation)
            {
                case ItemUseEnum.OutsideCombat:
                    if (UsableOutsideCombat)
                    {
                        UseAbility();
                    }
                    break;
                case ItemUseEnum.InsideCombat:
                    if (UsableInCombat)
                    {
                        UseAbility();
                    }
                    break;
                case ItemUseEnum.InUI:
                    UseInUI();
                    break;
            }
        }

        private void UseAbility()
        {
            if (ItemAbility != null && EquipmentHandle != null)
            {
                EquipmentHandle.Unit.Scene.SelectAbility(ItemAbility, EquipmentHandle.Unit);
            }
        }

        protected virtual void UseInUI()
        {

        }

        public virtual void OnEquipped(bool fromLoad = false)
        {
            if(EquipmentHandle.Unit.PermanentId.Id != 0)
            {
                InitializeAbility(fromLoad);
            }
            else
            {
                //A permanent Id is required for a handful of ability types so
                //we need to ensure we are creating the item ability after the unit
                //gets assigned their permanent Id
                void initAbility() 
                {
                    InitializeAbility(fromLoad, () =>
                    {
                        EquipmentHandle.Unit.PermanentIdInitialized -= initAbility;
                    });
                }

                EquipmentHandle.Unit.PermanentIdInitialized += initAbility;
            }
        }

        public virtual void InitializeAbility(bool fromLoad = false, Action initializeCallback = null)
        {
            if (ItemAbility != null)
            {
                ItemAbility.CastingUnit = EquipmentHandle.Unit;
                ItemAbility.AddAbilityToUnit(fromLoad);
            }

            initializeCallback?.Invoke();
        }

        public virtual void OnUnequipped(bool fromLoad = false)
        {
            if (ItemAbility != null)
            {
                ItemAbility.RemoveAbilityFromUnit(fromLoad);
                ItemAbility.CastingUnit = null;
            }
        }

        public virtual void SetModifier(int modifier)
        {
            Modifier = modifier;
        }

        public virtual void BuildAnimationSet()
        {
            //Instantiate the item's animation set here.
            //Items can have different animations for different modifiers, internal states, etc
        }

        public Icon Generate(UIScale size)
        {
            if (AnimationSet == null)
                return null;

            Icon icon = new Icon(size, AnimationSet.BuildAnimationsFromSet());

            icon.LoadTexture();

            return icon;
        }

        public override bool Equals(object obj)
        {
            return obj is Item item &&
                   Id == item.Id && Modifier == item.Modifier;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Modifier);
        }
    }
}
