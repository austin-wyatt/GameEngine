using Empyrean.Game.Items;
using Empyrean.Game.Save;
using Empyrean.Game.Serializers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Empyrean.Game.Save
{
    [Serializable]
    public class InventoryGroup : ISerializable
    {
        public static Dictionary<int, InventoryEntry> DefaultInfo = new Dictionary<int, InventoryEntry>();

        [XmlIgnore]
        public Dictionary<int, InventoryEntry> InventoryInfo = new Dictionary<int, InventoryEntry>();

        [XmlElement("_ii")]
        public DeserializableDictionary<int, InventoryEntry> _inventoryInfo = new DeserializableDictionary<int, InventoryEntry>();

        public InventoryGroup() { }

        public void CompleteDeserialization()
        {
            InventoryInfo.Clear();
            _inventoryInfo.FillDictionary(InventoryInfo);
        }

        public void PrepareForSerialization()
        {
            _inventoryInfo = new DeserializableDictionary<int, InventoryEntry>(InventoryInfo);
        }
    }
}
