using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace MortalDungeon.Game.Serializers
{
    [XmlType(TypeName = "STI")]
    [Serializable]
    public class StateIDValuePair
    {
        [XmlElement("STt")]
        public int Type; //use LedgerUpdateTypes
        [XmlElement("STs")]
        public long StateID;
        [XmlElement("STo")]
        public long ObjectHash;
        [XmlElement("STd")]
        public int Data;

        [XmlElement("STin")]
        public int Instruction; //use StateInstructions
        [XmlElement("STsv")]
        public List<StateIDValuePair> Values = new List<StateIDValuePair>();
    }
}
