using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Xml.Serialization;

namespace Empyrean.Game.Save
{

    [XmlType(TypeName = "FSI")]
    [Serializable]
    public class FeatureSaveInfo
    {
        public BigInteger ID;

        [XmlElement("Fi", Namespace = "Fsi")]
        public DeserializableDictionary<BigInteger, short> SignificantInteractions;

        [XmlElement("Fhd", Namespace = "Fshd")]
        public DeserializableDictionary<BigInteger, DeserializableDictionary_<string, string>> HashData;
    }
}
