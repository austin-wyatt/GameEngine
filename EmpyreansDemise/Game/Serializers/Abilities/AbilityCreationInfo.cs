using Empyrean.Game.Abilities;
using Empyrean.Game.Units;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Empyrean.Game.Serializers.Abilities
{
    [Serializable]
    public class AbilityCreationInfo : ISerializable
    {
        [XmlElement("ACIti")]
        public AbilityTreeType TreeId;
        [XmlElement("ACIai")]
        public int AbilityId;
        [XmlElement("ACImod")]
        public int Modifier;
        [XmlElement("ACIap", Namespace = "ACI")]
        public ParameterDict AbilityParameters;


        public void CompleteDeserialization()
        {
            AbilityParameters.CompleteDeserialization();
        }

        public void PrepareForSerialization()
        {
            AbilityParameters.PrepareForSerialization();
        }
    }
}
