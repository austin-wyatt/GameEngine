using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Items
{
    [Serializable]
    public class ItemEntry
    {
        public int Id;
        public int Name;

        public int StackSize = 1;
        public int Charges;

        public int Modifier = 0;

        public ItemLocation ItemLocation;

        public ItemEntry() { }

        public ItemEntry(Item item)
        {
            Id = item.Id;
            Name = item.Name;
            StackSize = item.StackSize;
            Charges = item.Charges;
            Modifier = item.Modifier;
            ItemLocation = item.Location;
        }

        public Item GetItemFromEntry()
        {
            var item = (Item)Activator.CreateInstance(ItemManager.Items[Id]);
            ApplyEntryToItem(item);

            return item;
        }

        private void ApplyEntryToItem(Item item)
        {
            item.StackSize = StackSize;
            item.Charges = Charges;
            item.Modifier = Modifier;
            item.Location = ItemLocation;
        }
    }
}
