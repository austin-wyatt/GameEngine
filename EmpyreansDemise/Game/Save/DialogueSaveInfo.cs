using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Empyrean.Game.Save
{
    [XmlType(TypeName = "DSI")]
    [Serializable]
    public class DialogueSaveInfo
    {
        [XmlElement("D_id")]
        public int ID;

        [XmlElement("Dqs", Namespace = "Dsi")]
        public DeserializableHashset<int> RecievedOutcomes;
    }
}
