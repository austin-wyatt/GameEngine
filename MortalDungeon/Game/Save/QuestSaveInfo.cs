using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Empyrean.Game.Save
{
    [XmlType(TypeName = "QSI")]
    [Serializable]
    public class QuestSaveInfo
    {
        [XmlElement("Q_id")]
        public int ID;

        [XmlElement("Qst", Namespace = "Qsi")]
        public DeserializableHashset<int> QuestState;
    }
}
