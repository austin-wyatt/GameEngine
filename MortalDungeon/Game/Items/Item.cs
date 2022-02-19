using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.UIComponents;
using MortalDungeon.Game.Abilities;
using MortalDungeon.Game.Objects;
using MortalDungeon.Game.Player;
using MortalDungeon.Game.Serializers;
using MortalDungeon.Game.Units;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Items
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

    public class Item
    {
        public int Id;
        public TextInfo Name;
        public TextInfo Description;

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

        public virtual void OnEquipped()
        {
            if(ItemAbility != null)
            {
                ItemAbility.CastingUnit = EquipmentHandle.Unit;
            }
        }

        public virtual void OnUnequipped()
        {
            if (ItemAbility != null)
            {
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

        public BaseObject CreateBaseObject()
        {
            BaseObject obj = new BaseObject(AnimationSet.BuildAnimationsFromSet(), 0, "", default);

            return obj;
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
