using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Engine_Classes.UIComponents
{
    public class Cursor : UIObject
    {
        public UIScale CursorScale;

        private float _cursorWidthRatio = 0.055f;
        private float _cursorHeightRatio = 1.3f;
        public Cursor(Vector3 position, float textScale) 
        {
            CursorScale = new UIScale(_cursorWidthRatio * textScale, _cursorHeightRatio * textScale);

            UIBlock block = new UIBlock(Position, CursorScale, default, 71, true);

            BaseComponent = block;

            block.SetColor(Colors.White);
            block.MultiTextureData.MixTexture = false;
            block._baseObject.OutlineParameters.SetAllInline(0);
            block._baseObject.OutlineParameters.SetAllOutline(0);
            block._baseObject.RenderData = new RenderData() { AlphaThreshold = 0 };

            AddChild(block);

            PropertyAnimation animation = new PropertyAnimation(block._baseObject.BaseFrame);
            Keyframe offFrame = new Keyframe(25, (_) => Render = false);
            Keyframe onFrame = new Keyframe(50, (_) => Render = true);

            animation.Repeat = true;

            animation.Keyframes.Add(offFrame);
            animation.Keyframes.Add(onFrame);

            animation.Play();

            PropertyAnimations.Add(animation);
        }
    }
}
