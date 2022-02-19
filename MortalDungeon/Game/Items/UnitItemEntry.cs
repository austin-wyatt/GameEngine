using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace MortalDungeon.Game.Items
{
    [XmlType(TypeName = "UIEm")]
    [Serializable]
    public class UnitItemEntry
    {
        [XmlElement("UIEmi")]
        public ItemEntry ItemEntry = new ItemEntry();

        [XmlElement("UIEml")]
        public ItemLocation Location = ItemLocation.Inventory;

        [XmlElement("UIEme")]
        public EquipmentSlot EquipmentSlot = EquipmentSlot.None;
    }
}
