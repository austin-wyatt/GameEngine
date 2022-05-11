using Empyrean.Engine_Classes;
using Empyrean.Game.Player;
using Empyrean.Game.Save;
using Empyrean.Game.Serializers;
using Empyrean.Game.Units;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Empyrean.Game.Items
{
    public enum EquipmentSlot
    {
        None = 0,
        Weapon_1 = 1, //mostly active
        Trinket = 2, //mostly active
        Boots = 4, //mostly passive
        Gloves = 8, //mostly passive
        Armor = 16, //mostly passive
        Jewelry_1 = 32, //mostly active
        Jewelry_2 = 64,
        Consumable_1 = 128,
        Consumable_2 = 256,
        Consumable_3 = 512,
        Consumable_4 = 1024,
        Weapon_2 = 2048,
        All = None | Weapon_1 | Trinket | Boots | Gloves | Armor | Jewelry_1 | Jewelry_2 | Consumable_1 | Consumable_2 | Consumable_3 | Consumable_4 | Weapon_2

    }

    public enum ItemType
    {
        BasicItem,
        CraftingComponent,
        Weapon,
        Trinket,
        Boots,
        Gloves,
        Armor,
        Jewelry,
        Consumable
    }

    public enum EquipItemError
    {
        None,
        RequirementsNotMet,
        SlotUnavailable,
        InvalidEquipmentSlot
    }

    [Serializable]
    public class Equipment : ISerializable
    {
        [XmlIgnore]
        public Dictionary<EquipmentSlot, Item> EquippedItems = new Dictionary<EquipmentSlot, Item>();

        [XmlIgnore]
        public EquipmentSlot PrimaryWeaponSlot = EquipmentSlot.Weapon_1;

        [XmlIgnore]
        public EquipmentSlot AvailableSlots = EquipmentSlot.All;

        [XmlElement("AvailableSlots")]
        public int _availableSlot
        {
            get { return (int)AvailableSlots; }
            set { AvailableSlots = (EquipmentSlot)value; }
        }

        /// <summary>
        /// Does not include offhand weapon
        /// </summary>
        [XmlIgnore]
        public ItemTag EquippedItemTags = ItemTag.None;

        [XmlElement("EquippedItemTags")]
        public long _equippedItemTags
        {
            get { return (long)EquippedItemTags; }
            set { EquippedItemTags = (ItemTag)value; }
        }
        
        /// <summary>
        /// Includes offhand weapon
        /// </summary>
        [XmlIgnore]
        public ItemTag AllEquippedItemTags = ItemTag.None;

        [XmlElement("EquippedItemTags")]
        public long _allEquippedItemTags
        {
            get { return (long)AllEquippedItemTags; }
            set { AllEquippedItemTags = (ItemTag)value; }
        }


        [XmlIgnore]
        public Unit Unit;

        public Equipment() { }
        public Equipment(Unit unit)
        {
            Unit = unit;
        }

        public EquipItemError EquipItem(Item item, EquipmentSlot slot)
        {
            Item removedItem;

            if((item.ItemType.EquipmentSlot() & slot) == EquipmentSlot.None)
            {
                return EquipItemError.InvalidEquipmentSlot;
            }

            if((AvailableSlots & slot) != EquipmentSlot.None)
            {
                item.Location = ItemLocation.Equipment;
                item.EquipmentHandle = this;

                EquippedItems.TryGetValue(slot, out removedItem);

                EquippedItems.AddOrSet(slot, item);
                item.OnEquipped();

                if (Unit.Info.PartyMember)
                {
                    PlayerParty.Inventory.RemoveItemFromInventory(item);
                }
            }
            else
            {
                return EquipItemError.SlotUnavailable;
            }


            if (removedItem != null && Unit.Info.PartyMember)
            {
                removedItem.EquipmentHandle = null;
                removedItem.OnUnequipped();

                PlayerParty.Inventory.AddItemToInventory(removedItem);
            }
            else if(removedItem != null)
            {
                removedItem.EquipmentHandle = null;
                removedItem.OnUnequipped();

                Unit.Info.Inventory.AddItemToInventory(removedItem);
            }

            CollateItemTags();

            return EquipItemError.None;
        }

        public void UnequipItem(EquipmentSlot slot)
        {
            EquippedItems.TryGetValue(slot, out Item removedItem);

            if (removedItem != null)
            {
                removedItem.EquipmentHandle = null;
                removedItem.OnUnequipped();

                EquippedItems.Remove(slot);
                PlayerParty.Inventory.AddItemToInventory(removedItem);

                CollateItemTags();
            }
        }

        private void CollateItemTags()
        {
            EquippedItemTags = ItemTag.None;
            AllEquippedItemTags = ItemTag.None;

            foreach (var itemKVP in EquippedItems)
            {
                AllEquippedItemTags |= itemKVP.Value.Tags;

                if (itemKVP.Key == EquipmentSlot.Weapon_2 && ((itemKVP.Value.Tags & ItemTag.Weapon_Concealed) != ItemTag.Weapon_Concealed)) 
                    continue;

                EquippedItemTags |= itemKVP.Value.Tags;
            }
        }

        public void SwapWeapons()
        {
            switch (PrimaryWeaponSlot)
            {
                case EquipmentSlot.Weapon_1:
                    PrimaryWeaponSlot = EquipmentSlot.Weapon_2;
                    break;
                case EquipmentSlot.Weapon_2:
                    PrimaryWeaponSlot = EquipmentSlot.Weapon_1;
                    break;
            }

            if(Unit != null && Unit.Scene.InCombat)
            {
                Unit.Info.Context.SetFlag(UnitContext.WeaponSwappedThisTurn, true);

                int staminaCost = Math.Clamp(Unit.GetResI(ResI.Stamina), 0, Unit.WEAPON_SWAP_MAX_STAMINA_COST);

                Unit.AddResI(ResI.Stamina, -staminaCost);

                if (staminaCost > 0)
                    Unit.Scene.Footer.UpdateFooterInfo();
            }

            CollateItemTags();
        }

        public List<Item> GetItems()
        {
            List<Item> items = new List<Item>();

            foreach(var kvp in EquippedItems)
            {
                if(kvp.Key < EquipmentSlot.Consumable_1)
                {
                    items.Add(kvp.Value);
                }
            }

            return items;
        }

        public List<Item> GetConsumables()
        {
            List<Item> items = new List<Item>();

            foreach (var kvp in EquippedItems)
            {
                if (kvp.Value.ItemType == ItemType.Consumable)
                {
                    items.Add(kvp.Value);
                }
            }

            return items;
        }


        [XmlElement(Namespace = "equ")]
        private DeserializableDictionary<EquipmentSlot, ItemEntry> _equippedItems;
        public void CompleteDeserialization()
        {
            EquippedItems.Clear();

            for (int i = 0; i < (_equippedItems?.Keys.Count ?? 0); i++)
            {
                Item item = _equippedItems.Values[i].GetItemFromEntry();

                EquippedItems.Add(_equippedItems.Keys[i], item);
            }

            CollateItemTags();
        }

        public void PrepareForSerialization()
        {
            _equippedItems = new DeserializableDictionary<EquipmentSlot, ItemEntry>();

            foreach(var kvp in EquippedItems)
            {
                _equippedItems.Keys.Add(kvp.Key);

                var itemEntry = new ItemEntry(kvp.Value);
                _equippedItems.Values.Add(itemEntry);
            }
        }
    }
}
