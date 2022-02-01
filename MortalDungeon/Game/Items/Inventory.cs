using MortalDungeon.Game.Serializers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace MortalDungeon.Game.Items
{
    public enum ItemAddError
    {
        None,
        UniqueAlreadyPresent,
        MaximumStackSizeReached
    }

    public enum GoldTransactionError
    {
        None,
        NotEnoughGold
    }

    [Serializable]
    public class Inventory : ISerializable
    {
        public long Gold;

        [XmlIgnore]
        public List<Item> Items = new List<Item>();

        public List<ItemEntry> _itemEntries = new List<ItemEntry>();

        public ItemAddError AddItemToInventory(Item item)
        {
            item.Location = ItemLocation.Inventory;
            
            if (item.Stackable)
            {
                var foundItem = Items.Find(i => i.Id == item.Id);

                if (foundItem != null)
                {
                    if (foundItem.StackSize + item.StackSize <= foundItem.MaxInventoryStack)
                    {
                        foundItem.StackSize += item.StackSize;
                    }
                    else
                    {
                        foundItem.StackSize = foundItem.MaxInventoryStack;
                        return ItemAddError.MaximumStackSizeReached;
                    }
                }
                else
                {
                    Items.Add(item);
                }
            }
            else if (item.Unique)
            {
                return ItemAddError.UniqueAlreadyPresent;
            }
            else
            {
                Items.Add(item);
            }

            return ItemAddError.None;
        }

        public void RemoveItemFromInventory(Item item)
        {
            Items.Remove(item);
        }

        public void AddGold(long amount)
        {
            Gold += amount;
        }

        public GoldTransactionError RemoveGold(long amount)
        {
            if(Gold - amount >= 0)
            {
                Gold -= amount;
                return GoldTransactionError.None;
            }
            else
            {
                return GoldTransactionError.NotEnoughGold;
            }
        }

        public void CompleteDeserialization()
        {
            Items.Clear();

            foreach(var item in _itemEntries)
            {
                Items.Add(item.GetItemFromEntry());
            }
        }

        public void PrepareForSerialization()
        {
            _itemEntries.Clear();

            foreach(var item in Items)
            {
                _itemEntries.Add(new ItemEntry(item));
            }
        }
    }
}
