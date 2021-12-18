using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.UIComponents;
using MortalDungeon.Game.Units;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.UI
{
    internal class FocusBar : UIObject
    {
        internal float _focusPercent = 1;
        private UIObject _focusBar;

        internal Vector4 BarColor = new Vector4(0.57f, 0, 1, 1f);

        internal FocusBar(Vector3 position, UIScale scale)
        {
            Size = scale;
            Position = position;

            BaseComponent = new UIBlock(Position, Size);


            _focusBar = new UIBlock(Position, Size);
            _focusBar.SetColor(BarColor);
            _focusBar.MultiTextureData.MixTexture = true;
            _focusBar.MultiTextureData.MixPercent = 0.25f;
            _focusBar.SetPositionFromAnchor(BaseComponent.GetAnchorPosition(UIAnchorPosition.BottomLeft), UIAnchorPosition.TopLeft);

            BaseComponent.AddChild(_focusBar);


            AddChild(BaseComponent);
        }

        internal void SetFocusPercent(float percent)
        {
            if (percent < 0)
            {
                _focusPercent = 0;
            }
            else
            {
                _focusPercent = percent;
            }

            BaseComponent.SetSize(Size);
            _focusBar.SetSize(new UIScale(Size.X * _focusPercent, Size.Y));
            _focusBar.SetPositionFromAnchor(BaseComponent.GetAnchorPosition(UIAnchorPosition.TopLeft), UIAnchorPosition.TopLeft);
        }

        internal override void SetSize(UIScale size)
        {
            base.SetSize(size);

            SetFocusPercent(_focusPercent);
        }

        internal override void SetInlineColor(Vector4 color)
        {
            base.SetInlineColor(color);
            _focusBar.SetInlineColor(color);
        }
    }
}
