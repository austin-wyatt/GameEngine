using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MortalDungeon.Game.Serializers
{
    [XmlType(TypeName = "ANS")]
    [Serializable]
    public class AnimationSet : ISerializable
    {
        [XmlElement("AnSas")]
        public List<Animation> Animations = new List<Animation>();

        [XmlElement("AnSn")]
        public string Name = "";

        public int Id = 0;

        public List<Engine_Classes.Animation> BuildAnimationsFromSet() 
        {
            List<Engine_Classes.Animation> builtAnimations = new List<Engine_Classes.Animation>();

            foreach(var anim in Animations)
            {
                builtAnimations.Add(anim.BuildAnimation());
            }

            return builtAnimations;
        }

        public void CompleteDeserialization()
        {

        }

        public void PrepareForSerialization()
        {

        }
    }
}
