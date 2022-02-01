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
        public EquipmentSlot AvailableSlots = EquipmentSlot.Consumable_1 | EquipmentSlot.Consumable_2 | EquipmentSlot.Weapon_1;

        [XmlIgnore]
        public Unit Unit;

        public Equipment() { }
        public Equipment(Unit unit)
        {
            Unit = unit;
        }

        public EquipItemError EquipItem(Item item, EquipmentSlot slot)
        {
            //check if equipment slot is available
            Item removedItem;

            if((item.ValidEquipmentSlots & slot) == EquipmentSlot.None)
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
                if (kvp.Key >= EquipmentSlot.Consumable_1)
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

            for (int i = 0; i < _equippedItems.Keys.Count; i++)
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
