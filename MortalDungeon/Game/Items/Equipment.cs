using MortalDungeon.Engine_Classes;
using MortalDungeon.Game.Player;
using MortalDungeon.Game.Save;
using MortalDungeon.Game.Serializers;
using MortalDungeon.Game.Units;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace MortalDungeon.Game.Items
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
        public EquipmentSlot AvailableSlots = (EquipmentSlot)2047;

        [XmlElement("AvailableSlots")]
        public int _availableSlot
        {
            get { return (int)AvailableSlots; }
            set { AvailableSlots = (EquipmentSlot)value; }
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

            return EquipItemError.None;
        }

        public void UnequipItem(EquipmentSlot slot)
        {
            EquippedItems.TryGetValue(slot, out Item removedItem);

            if (removedItem != null)
            {
                EquippedItems.Remove(slot);
                PlayerParty.Inventory.AddItemToInventory(removedItem);
            }
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
