using MortalDungeon.Game.Save;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace MortalDungeon.Game.Serializers
{

    [Serializable]
    public class UnitInfoBlock : ISerializable
    {
        [XmlIgnore]
        public Dictionary<int, UnitCreationInfo> Units = new Dictionary<int, UnitCreationInfo>();

        public DeserializableDictionary<int, UnitCreationInfo> _units = new DeserializableDictionary<int, UnitCreationInfo>();

        public int BlockId = 0;

        public void CompleteDeserialization()
        {
            _units.FillDictionary(Units);

            foreach (var unit in Units)
            {
                unit.Value.CompleteDeserialization();
            }
        }

        public void PrepareForSerialization()
        {
            _units = new DeserializableDictionary<int, UnitCreationInfo>(Units);

            foreach (var unit in _units.Values)
            {
                unit.PrepareForSerialization();
            }
        }
    }
}
