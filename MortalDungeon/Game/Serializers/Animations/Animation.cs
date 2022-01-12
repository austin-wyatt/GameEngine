using MortalDungeon.Engine_Classes;
using MortalDungeon.Game.Objects;
using MortalDungeon.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MortalDungeon.Game.Serializers
{
    [XmlType(TypeName = "Anim")]
    [Serializable]
    public class Animation
    {
        [XmlElement("AnF")]
        public int Frequency = 5;
        [XmlElement("AnR")]
        public int Repeats = -1;
        [XmlElement("AnS")]
        public TextureName Spritesheet = TextureName.SpritesheetTest;

        [XmlElement("AnT")]
        public AnimationType Type = AnimationType.Idle;

        [XmlElement("AnFI")]
        public List<int> FrameIndices = new List<int>();

        public Engine_Classes.Animation BuildAnimation()
        {
            Engine_Classes.Animation builtAnim = new Engine_Classes.Animation();

            builtAnim.Frequency = Frequency;
            builtAnim.Repeats = Repeats;

            builtAnim.Type = Type;

            foreach(int frameIndex in FrameIndices)
            {
                RenderableObject frame = new RenderableObject(
                    new SpritesheetObject(frameIndex, Spritesheets.AllSpritesheets[(int)Spritesheet]).CreateObjectDefinition(true), 
                    WindowConstants.FullColor, ObjectRenderType.Texture, Shaders.DEFAULT_SHADER);

                builtAnim.Frames.Add(frame);
            }

            return builtAnim;
        }
    }
}
