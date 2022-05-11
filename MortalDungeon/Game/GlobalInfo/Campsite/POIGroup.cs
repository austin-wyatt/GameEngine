using Empyrean.Game.Serializers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Empyrean.Game.Save
{
    [Serializable]
    public class POIGroup : ISerializable
    {
        public static Dictionary<int, POIEntry> DefaultInfo = new Dictionary<int, POIEntry>();

        public static List<int> CampsiteIDs = new List<int>();

        [XmlIgnore]
        public Dictionary<int, POIEntry> POIInfo = new Dictionary<int, POIEntry>();

        [XmlElement("_ci")]
        public DeserializableDictionary_<int, POIEntry> _poiInfo = new DeserializableDictionary_<int, POIEntry>();

        public POIGroup() { }

        public void CompleteDeserialization()
        {
            POIInfo.Clear();
            _poiInfo.FillDictionary(POIInfo);
        }

        public void PrepareForSerialization()
        {
            _poiInfo = new DeserializableDictionary_<int, POIEntry>(POIInfo);
        }
    }
}
