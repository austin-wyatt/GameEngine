using Empyrean.Engine_Classes;
using Empyrean.Game.Objects;
using Empyrean.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Empyrean.Game.Serializers
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
        public int Spritesheet = 1;

        [XmlElement("AnT")]
        public int Type = 0;

        [XmlElement("AnFI")]
        public List<int> FrameIndices = new List<int>();

        public Engine_Classes.Animation BuildAnimation()
        {
            Engine_Classes.Animation builtAnim = new Engine_Classes.Animation();

            builtAnim.Frequency = Frequency;
            builtAnim.Repeats = Repeats;

            builtAnim.Type = (AnimationType)Type;

            foreach(int frameIndex in FrameIndices)
            {
                RenderableObject frame = new RenderableObject(
                    new SpritesheetObject(frameIndex, SpritesheetManager.GetSpritesheet(Spritesheet)).CreateObjectDefinition(true), 
                    WindowConstants.FullColor, ObjectRenderType.Texture, Shaders.DEFAULT_SHADER);

                builtAnim.Frames.Add(frame);
            }

            return builtAnim;
        }
    }
}
