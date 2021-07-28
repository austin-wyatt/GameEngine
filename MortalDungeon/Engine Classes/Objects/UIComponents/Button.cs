using OpenTK.Mathematics;

namespace MortalDungeon.Engine_Classes.UIComponents
{
    public class Button : UIObject
    {
        public TextBox _mainObject;
        public Vector4 BaseColor = new Vector4(0.78f, 0.60f, 0.34f, 1);

        public Button(Vector3 position, UIScale size, string text = "", float textScale = 0.1f, Vector4 boxColor = default, Vector4 textColor = default, bool centerText = true)
        {
            Position = position;
            Size = size;

            Clickable = true;
            Hoverable = true;

            Name = "Button";

            TextBox textBox = new TextBox(position, size, text, textScale, centerText);
            _mainObject = textBox;
            BaseComponent = textBox;

            AddChild(textBox);


            if (boxColor != default)
            {
                BaseColor = boxColor;
                textBox.SetColor(boxColor);
            }
            else
            {
                textBox.SetColor(BaseColor);
            }
            if (textColor != default)
            {
                textBox.SetTextColor(textColor);
            }

            ValidateObject(this);
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
            base.OnMouseUp();
            Vector4 hoveredColor = new Vector4(BaseColor.X - 0.1f, BaseColor.Y - 0.1f, BaseColor.Z - 0.1f, BaseColor.W);

            SetColor(hoveredColor);
        }

        public override void SetColor(Vector4 color)
        {
            if (!Selected)
                _mainObject.SetColor(color);
        }
    }
}
