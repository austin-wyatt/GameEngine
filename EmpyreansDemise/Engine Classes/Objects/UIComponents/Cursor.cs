using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Engine_Classes.UIComponents
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

            block.SetColor(_Colors.White);
            block.MultiTextureData.MixTexture = false;
            block._baseObject.OutlineParameters.SetAllInline(0);
            block._baseObject.OutlineParameters.SetAllOutline(0);
            block._baseObject.RenderData = new RenderData() { AlphaThreshold = 0 };

            AddChild(block);

            PropertyAnimation animation = new PropertyAnimation(block._baseObject.BaseFrame);

            Keyframe onFrame = new Keyframe(0, () =>
            {
                BaseComponent.SetRender(true);
            });
            Keyframe offFrame = new Keyframe(25, () =>
            {
                BaseComponent.SetRender(false);
            });
            Keyframe endFrame = new Keyframe(50, () =>
            {
                BaseComponent.SetRender(true);
            });

            animation.Repeat = true;

            animation.Keyframes.Add(onFrame);
            animation.Keyframes.Add(offFrame);
            animation.Keyframes.Add(endFrame);

            animation.DEBUG_ID = 1;

            PropertyAnimations.Add(animation);

            animation.Play();
        }
    }
}
