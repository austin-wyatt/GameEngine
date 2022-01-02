using MortalDungeon.Game.Save;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MortalDungeon.Game.Serializers
{
    [XmlType(TypeName = "TT")]
    [Serializable]
    public class TextTable
    {

        [XmlIgnore]
        public Dictionary<int, TextEntry> Strings = new Dictionary<int, TextEntry>();
        [XmlElement("Tti")]
        public int TableID = 0;

        [XmlElement("Tdd")]
        public DeserializableDictionary<int, TextEntry> _strings = new DeserializableDictionary<int, TextEntry>();

        public TextEntry AddTextEntry(int id, string text)
        {
            TextEntry entry = new TextEntry() { ID = id, Text = text };

            Strings.Add(id, entry);

            return entry;
        }

        public void RemoveTextEntry(int id)
        {
            Strings.Remove(id);
        }

        public void ModifyTextEntry(int id, string text)
        {
            if (Strings.TryGetValue(id, out TextEntry entry))
            {
                entry.Text = text;
            }
        }

        public bool TryGetTextEntry(int textEntryID, out TextEntry entry)
        {
            entry = null;

            if (Strings.TryGetValue(textEntryID, out var e))
            {
                entry = e;
                return true;
            }

            return false;
        }

        public int GetNextAvailableID()
        {
            var list = Strings.Keys.ToList();
            list.Sort();

            int id = 0;

            int lastId = 0;
            foreach (var key in list)
            {
                if (lastId == 0)
                {
                    lastId = key;
                }
                else if (key != lastId + 1)
                {
                    return lastId + 1;
                }

                lastId = key;
            }

            return list[^1] + 1;
        }


        public TextTable() { }
    }


    [XmlType(TypeName = "TE")]
    [Serializable]
    public class TextEntry
    {
        [XmlElement("i")]
        public int ID = -1;
        [XmlElement("T")]
        public string Text = "";

        public TextEntry() { }
    }
}
