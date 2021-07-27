using OpenTK.Mathematics;
using System;

namespace MortalDungeon.Engine_Classes.UIComponents
{
    public class ToggleableButton : Button
    {
        public Action OnSelectAction = null;
        public Action OnDeselectAction = null;
        public ToggleableButton(Vector3 pos, Vector2 size, string text = "", float textScale = 1, Vector4 boxColor = default, Vector4 textColor = default, bool centerText = false) 
            : base(pos, size, text, textScale, boxColor, textColor, centerText)
        {
            Name = "ToggleableButton";

            BaseColor = Colors.UISelectedGray;
            SetColor(BaseColor);
        }

        public override void OnHover()
        {
            if (!Hovered)
            {
                Vector4 hoveredColor = new Vector4(BaseColor.X - 0.1f, BaseColor.Y - 0.1f, BaseColor.Z - 0.1f, BaseColor.W);

                SetColor(hoveredColor);
            }

            base.OnHover();
        }

        public override void HoverEnd()
        {
            if (Hovered)
            {
                SetColor(BaseColor);
            }

            base.HoverEnd();
        }

        public override void OnMouseDown()
        {
            base.OnMouseDown();
            Vector4 mouseDownColor = new Vector4(BaseColor.X - 0.2f, BaseColor.Y - 0.2f, BaseColor.Z - 0.2f, BaseColor.W);

            SetColor(mouseDownColor);
        }
        public override void OnMouseUp()
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

        public override void SetColor(Vector4 color)
        {
            if (!Selected)
                _mainObject.SetColor(color);
        }
    }
}
