using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace MortalDungeon.Game.Save
{

    [XmlType(TypeName = "FSI")]
    [Serializable]
    public class FeatureSaveInfo
    {
        public long ID;

        [XmlElement("Fi", Namespace = "Fsi")]
        public DeserializableDictionary<long, short> SignificantInteractions;
    }
}
