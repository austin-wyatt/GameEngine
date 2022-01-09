using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace MortalDungeon.Game.Serializers
{
    [XmlType(TypeName = "inst")]
    [Serializable]
    public class Instructions
    {
        [XmlElement("insd")]
        public string Description = "Script";
        [XmlElement("insc")]
        public string Script = "";
        [XmlElement("inni")]
        public List<Instructions> NestedInstructions = new List<Instructions>();
    }
}
