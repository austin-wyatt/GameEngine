using Empyrean.Game.Items;
using Empyrean.Game.Serializers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Game.Save
{
    [Serializable]
    public class InventoryEntry : ISerializable
    {
        public int Id;
        public Inventory Inventory = new Inventory();
        public bool IsDefault = true;

        public InventoryEntry() { }

        public InventoryEntry(InventoryEntry entry)
        {
            Id = entry.Id;
            Inventory = new Inventory(entry.Inventory);
            IsDefault = entry.IsDefault;
        }

        public void CompleteDeserialization()
        {
            Inventory.CompleteDeserialization();
        }

        public void PrepareForSerialization()
        {
            Inventory.PrepareForSerialization();
        }
    }
}
