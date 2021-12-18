using OpenTK.Mathematics;
using System;

namespace MortalDungeon.Engine_Classes.UIComponents
{
    internal class ToggleableButton : Button
    {
        internal Action OnSelectAction = null;
        internal Action OnDeselectAction = null;
        internal ToggleableButton(Vector3 pos, UIScale size, string text = "", float textScale = 0.1f, Vector4 boxColor = default, Vector4 textColor = default, bool centerText = false) 
            : base(pos, size, text, textScale, boxColor, textColor, centerText)
        {
            Name = "ToggleableButton";

            BaseColor = Colors.UISelectedGray;
            SetColor(BaseColor);
        }

        internal override void OnHover()
        {
            if (!Hovered)
            {
                Vector4 hoveredColor = new Vector4(BaseColor.X - 0.1f, BaseColor.Y - 0.1f, BaseColor.Z - 0.1f, BaseColor.W);

                SetColor(hoveredColor);
            }

            base.OnHover();
        }

        internal override void OnHoverEnd()
        {
            if (Hovered)
            {
                SetColor(BaseColor);
            }

            base.OnHoverEnd();
        }

        internal override void OnMouseDown()
        {
            base.OnMouseDown();
            Vector4 mouseDownColor = new Vector4(BaseColor.X - 0.2f, BaseColor.Y - 0.2f, BaseColor.Z - 0.2f, BaseColor.W);

            SetColor(mouseDownColor);
        }
        internal override void OnMouseUp()
        {
            Selected = !Selected;

            if (Selected)
            {
                OnSelectAction?.Invoke();
                base.OnMouseUp();
            }
            else 
            {
                OnDeselectAction?.Invoke();
            }
        }

        internal override void SetColor(Vector4 color)
        {
            if (!Selected)
                TextBox.SetColor(color);
        }
    }
}
