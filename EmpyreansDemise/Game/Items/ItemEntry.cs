using Empyrean.Game.Serializers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Empyrean.Game.Items
{

    [XmlType(TypeName = "IEm")]
    [Serializable]
    public class ItemEntry
    {
        public int Id;

        public TextId Name = new TextId();

        [XmlElement("IEd")]
        public TextId Description = new TextId();

        [XmlElement("IEss")]
        public int StackSize = 1;

        [XmlElement("IEc")]
        public int Charges;

        [XmlElement("IEm")]
        public int Modifier = 0;

        [XmlElement("IEl")]
        public ItemLocation ItemLocation;

        public ItemEntry() { }

        public ItemEntry(Item item)
        {
            Id = item.Id;

            Name = new TextId(item.Name.Id);
            Description = new TextId(item.Description.Id);

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
            item.SetModifier(Modifier);
            item.Location = ItemLocation;
        }
    }
}
