using Empyrean.Engine_Classes.Text;
using Empyrean.Engine_Classes.TextHandling;
using OpenTK.Mathematics;
using System;
using System.Drawing;

namespace Empyrean.Engine_Classes.UIComponents
{
    public class Button : UIObject
    {
        public TextString TextBox;
        //public Vector4 BaseColor = new Vector4(0.78f, 0.60f, 0.34f, 1);
        public Vector4 BaseColor = _Colors.UILightGray;
        public static Color BaseBoxColor = Color.FromArgb(216, 216, 216);

        public Button(Vector3 position, UIScale size, FontInfo font, string text = "", Vector4 boxColor = default, Vector4 textColor = default, bool centerText = true)
        {
            if(textColor == default)
            {
                textColor = _Colors.UITextBlack;
            }

            Position = position;
            Size = size;

            Clickable = true;
            Hoverable = true;

            Name = "Button";

            BaseComponent = new UIBlock();
            //BaseComponent.SetColor(_Colors.UILightGray);
            BaseComponent.SetPosition(Position);

            BaseComponent.SetSize(size);

            //TextBox textBox = new TextBox(position, size, text, textScale, centerText);
            //TextBox = textBox;
            //BaseComponent = textBox;

            //AddChild(textBox);
            //TextComponent textBox = new TextComponent();
            //textBox.SetText(text);
            //textBox.SetTextScale(textScale);

            var textBox = new TextString(font, centerText ? TextAlignment.Center : TextAlignment.LeftAlign)
            {
                TextColor = textColor
            };

            textBox.SetText(text);

            TextBox = textBox;

            BaseComponent.AddTextString(textBox);
            
            AddChild(BaseComponent, 49);


            textBox.SetPosition(Position - new Vector3(0, textBox.GetDescender(), 0));


            if (boxColor != default)
            {
                BaseColor = boxColor;
                BaseComponent.SetColor(boxColor);
            }
            else
            {
                BaseComponent.SetColor(BaseColor);
            }

            SetColor(BaseColor);
            HoverColor = new Vector4(Math.Clamp(BaseColor.X - 0.1f, 0, 1), Math.Clamp(BaseColor.Y - 0.1f, 0, 1), Math.Clamp(BaseColor.Z - 0.1f, 0, 1), BaseColor.W);

            ValidateObject(this);
        }

        //public override void OnHover()
        //{
        //    if (!Hovered)
        //    {
        //        Vector4 hoveredColor = new Vector4(BaseColor.X - 0.1f, BaseColor.Y - 0.1f, BaseColor.Z - 0.1f, BaseColor.W);

        //        SetColor(hoveredColor);
        //    }

        //    base.OnHover();
        //}

        //public override void OnHoverEnd()
        //{
        //    if (Hovered)
        //    {
        //        SetColor(BaseColor);
        //    }

        //    base.OnHoverEnd();
        //}


        public override void OnMouseDown()
        {
            base.OnMouseDown();
            //Vector4 mouseDownColor = new Vector4(BaseColor.X - 0.2f, BaseColor.Y - 0.2f, BaseColor.Z - 0.2f, BaseColor.W);

            //SetColor(mouseDownColor);
        }
        public override void OnMouseUp()
        {
            base.OnMouseUp();
            //Vector4 hoveredColor = new Vector4(BaseColor.X - 0.1f, BaseColor.Y - 0.1f, BaseColor.Z - 0.1f, BaseColor.W);

            //SetColor(hoveredColor);
        }

        public override void OnClick()
        {
            base.OnClick();

            Audio.Sound sound = new Audio.Sound(Game.Sounds.Select) { Gain = 0.15f, Pitch = GlobalRandom.NextFloat(1f, 1f) };
            sound.Play();
        }

        //public override void SetColor(Vector4 color, SetColorFlag setColorFlag = SetColorFlag.Base)
        //{
        //    if (Disabled) 
        //    {
        //        BaseComponent.SetColor(_Colors.UIDisabledGray);
        //    }
        //    else if (!Selected)
        //        BaseComponent.SetColor(color);
        //    else 
        //    {
        //        Vector4 hoveredColor = new Vector4(BaseColor.X - 0.2f, BaseColor.Y - 0.2f, BaseColor.Z - 0.2f, BaseColor.W);
        //        BaseComponent.SetColor(hoveredColor);
        //    }
        //}

        public override void SetColor(Vector4 color, SetColorFlag flag = SetColorFlag.Base)
        {
            if (flag == SetColorFlag.Base)
                DefaultColor = color;

            BaseComponent.SetColor(color);
        }

        public override void OnDisabled(bool disable)
        {
            base.OnDisabled(disable);

            SetColor(BaseColor);
        }

        public void SetSelected(bool selected) 
        {
            Selected = selected;

            SetColor(BaseColor);
        }
    }
}
