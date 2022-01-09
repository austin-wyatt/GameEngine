using MortalDungeon.Game.Map;
using MortalDungeon.Game.Save;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace MortalDungeon.Game.Serializers
{
    [Serializable]
    public class BoundingPoints : ISerializable
    {
        public List<Vector3i> CubePoints = new List<Vector3i>();

        [XmlIgnore]
        public List<FeaturePoint> OffsetPoints = new List<FeaturePoint>();

        /// <summary>
        /// What "style" to apply to the inside of the bounding points. This would be like generic forest, generic desert, etc.
        /// If the value is 0 then no style will be applied.
        /// </summary>
        public int BoundingPointsId = 0;

        public bool SubscribeToEntrance = false;

        [XmlIgnore]
        public Dictionary<string, string> Parameters = new Dictionary<string, string>
        {
            {"name", "bounding point"}
        };

        [XmlElement(Namespace = "BPp")]
        public DeserializableDictionary<string, string> _parameters = new DeserializableDictionary<string, string>();

        public void PrepareForSerialization()
        {
            _parameters = new DeserializableDictionary<string, string>(Parameters);
        }

        public void CompleteDeserialization()
        {
            _parameters.FillDictionary(Parameters);
            _parameters = new DeserializableDictionary<string, string>();
        }
    }
}
