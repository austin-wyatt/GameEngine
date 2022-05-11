using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Xml.Serialization;

namespace Empyrean.Game.Serializers
{
    [XmlType(TypeName = "STI")]
    [Serializable]
    public class StateIDValuePair
    {
        [XmlElement("STt")]
        public int Type; //use LedgerUpdateTypes
        [XmlElement("STs")]
        public BigInteger StateID;
        [XmlElement("STo")]
        public BigInteger ObjectHash;
        [XmlElement("STd")]
        public int Data;

        [XmlElement("STin")]
        public int Instruction; //use StateInstructions
        [XmlElement("STsv")]
        public List<StateIDValuePair> Values = new List<StateIDValuePair>();
    }
}
