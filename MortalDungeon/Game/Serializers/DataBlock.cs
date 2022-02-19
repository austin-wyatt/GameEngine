using MortalDungeon.Game.Save;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace MortalDungeon.Game.Serializers
{
    [Serializable]
    public class DataBlock<T> : ISerializable where T : ISerializable
    {
        public int BlockId = 0;

        [XmlIgnore]
        public Dictionary<int, T> LoadedItems = new Dictionary<int, T>();

        public DeserializableDictionary<int, T> _loadedItems = new DeserializableDictionary<int, T>();


        public void CompleteDeserialization()
        {
            _loadedItems.FillDictionary(LoadedItems);

            foreach (var item in LoadedItems)
            {
                item.Value.CompleteDeserialization();
            }
        }

        public void PrepareForSerialization()
        {
            _loadedItems = new DeserializableDictionary<int, T>(LoadedItems);

            foreach (var item in _loadedItems.Values)
            {
                item.PrepareForSerialization();
            }
        }
    }
}
