﻿using MortalDungeon.Game.Save;
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
    public class TextTable : ISerializable
    {

        [XmlIgnore]
        public Dictionary<int, TextEntry> Strings = new Dictionary<int, TextEntry>();
        [XmlElement("Tti")]
        public int TableID = 0;

        [XmlElement("Ttn")]
        public string Name = "";

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

            if(list.Count == 0)
            {
                return 1;
            }

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

        public void PrepareForSerialization()
        {
            _strings = new DeserializableDictionary<int, TextEntry>(Strings);
        }

        public void CompleteDeserialization()
        {
            _strings.FillDictionary(Strings);
            _strings = new DeserializableDictionary<int, TextEntry>();
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

    [XmlType(TypeName = "TIn")]
    [Serializable]
    public class TextInfo
    {
        [XmlElement("i")]
        public int Id = 0;
        [XmlElement("T")]
        public int TableId = 0;

        [XmlIgnore]
        public List<TextReplacementParameter> TextReplacementParameters = new List<TextReplacementParameter>();

        public TextInfo() { }

        public TextInfo(int id, int tableId = 0) 
        {
            Id = id;
            TableId = tableId;
        }

        public override string ToString()
        {
            string val = TextTableManager.GetTextEntry(this);

            if(TextReplacementParameters.Count > 0)
            {
                for(int i = 0; i < TextReplacementParameters.Count; i++)
                {
                    val = val.Replace($"{{{TextReplacementParameters[i].Key}}}", TextReplacementParameters[i].Value());
                }
            }

            return val;
        }
    }

    public struct TextReplacementParameter
    {
        public string Key;
        public Func<string> Value;
    }
}
