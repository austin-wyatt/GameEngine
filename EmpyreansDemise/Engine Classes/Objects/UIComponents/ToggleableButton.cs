using OpenTK.Mathematics;
using System;

namespace Empyrean.Engine_Classes.UIComponents
{
    public class ToggleableButton : Button
    {
        public Action OnSelectAction = null;
        public Action OnDeselectAction = null;
        public ToggleableButton(Vector3 pos, UIScale size, string text = "", float textScale = 1f, Vector4 boxColor = default, Vector4 textColor = default, bool centerText = false) 
            : base(pos, size, text)
        {
            Name = "ToggleableButton";

            BaseColor = _Colors.UISelectedGray;
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

        public override void OnHoverEnd()
        {
            if (Hovered)
            {
                SetColor(BaseColor);
            }

            base.OnHoverEnd();
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

        public override void SetColor(Vector4 color, SetColorFlag flag = SetColorFlag.Base)
        {
            if (!Selected)
                TextBox.SetColor(color);
        }
    }
}
