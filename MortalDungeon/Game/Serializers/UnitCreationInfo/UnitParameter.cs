using Empyrean.Game.Save;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Empyrean.Game.Serializers
{
    [Serializable]
    public class AffectedPoint : ParameterDict, ISerializable
    {
        public Vector3i Point;
        public int Value;
    }

    [Serializable]
    public class ParameterDict : ISerializable
    {
        [XmlIgnore]
        public Dictionary<string, string> Parameters = new Dictionary<string, string>();

        [XmlElement(Namespace = "UPa")]
        public DeserializableDictionary<string, string> _parameters = new DeserializableDictionary<string, string>();

        public bool TryGetValue(string key, out string value)
        {
            return Parameters.TryGetValue(key, out value);
        }

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
