using Empyrean.Game.Save;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Empyrean.Game.Serializers
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


    /// <summary>
    /// Used internally for TextTables
    /// </summary>
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

    /// <summary>
    /// Used externally
    /// </summary>
    [XmlType(TypeName = "TIn")]
    [Serializable]
    public struct TextInfo
    {
        [XmlElement("i")]
        public int Id;
        [XmlElement("T")]
        public int TableId;

        [XmlIgnore]
        public TextReplacementParameter[] TextReplacementParameters;

        public static TextInfo Empty = new TextInfo(-1, -1);

        public TextInfo(int id, int tableId = 0) 
        {
            Id = id;
            TableId = tableId;
            TextReplacementParameters = new TextReplacementParameter[0];
        }

        public TextInfo(ref TextInfo info)
        {
            Id = info.Id;
            TableId = info.TableId;
            TextReplacementParameters = new TextReplacementParameter[info.TextReplacementParameters.Length];

            info.TextReplacementParameters.CopyTo(TextReplacementParameters, 0);
        }

        public static bool operator ==(TextInfo a, TextInfo b) => a.Equals(b);
        public static bool operator !=(TextInfo a, TextInfo b) => !a.Equals(b);

        public override string ToString()
        {
            string val = TextTableManager.GetTextEntry(this);

            if(TextReplacementParameters?.Length > 0)
            {
                for(int i = 0; i < TextReplacementParameters.Length; i++)
                {
                    val = val.Replace($"{{{TextReplacementParameters[i].Key}}}", TextReplacementParameters[i].Value());
                }
            }

            return val;
        }

        public override bool Equals(object obj)
        {
            return obj is TextInfo info &&
                   Id == info.Id &&
                   TableId == info.TableId;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, TableId);
        }
    }

    public struct TextReplacementParameter
    {
        public string Key;
        public Func<string> Value;
    }
}
