﻿using OpenTK.Mathematics;
using System;

namespace MortalDungeon.Engine_Classes.UIComponents
{
    public class Button : UIObject
    {
        public TextComponent TextBox;
        public Vector4 BaseColor = new Vector4(0.78f, 0.60f, 0.34f, 1);

        public Button(Vector3 position, UIScale size, string text = "", float textScale = 0.1f, Vector4 boxColor = default, Vector4 textColor = default, bool centerText = true)
        {
            Position = position;
            Size = size;

            Clickable = true;
            Hoverable = true;

            Name = "Button";

            BaseComponent = new UIBlock();
            BaseComponent.SetColor(Colors.UILightGray);
            BaseComponent.SetPosition(Position);

            BaseComponent.SetSize(size);

            //TextBox textBox = new TextBox(position, size, text, textScale, centerText);
            //TextBox = textBox;
            //BaseComponent = textBox;

            //AddChild(textBox);
            TextComponent textBox = new TextComponent();
            textBox.SetText(text);
            textBox.SetTextScale(textScale);
            
            TextBox = textBox;
            

            AddChild(textBox, 50);
            AddChild(BaseComponent, 49);


            textBox.SetPositionFromAnchor(Position, UIAnchorPosition.Center);


            if (boxColor != default)
            {
                BaseColor = boxColor;
                BaseComponent.SetColor(boxColor);
            }
            else
            {
                BaseComponent.SetColor(BaseColor);
            }

            if (textColor != default)
            {
                textBox.SetColor(textColor);
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
            base.OnMouseUp();
            Vector4 hoveredColor = new Vector4(BaseColor.X - 0.1f, BaseColor.Y - 0.1f, BaseColor.Z - 0.1f, BaseColor.W);

            SetColor(hoveredColor);
        }

        public override void OnClick()
        {
            base.OnClick();

            Audio.Sound sound = new Audio.Sound(Game.Sounds.Select) { Gain = 0.15f, Pitch = GlobalRandom.NextFloat(1f, 1f) };
            sound.Play();
        }

        public override void SetColor(Vector4 color)
        {
            if (Disabled) 
            {
                BaseComponent.SetColor(Colors.UIDisabledGray);
            }
            else if (!Selected)
                BaseComponent.SetColor(color);
            else 
            {
                Vector4 hoveredColor = new Vector4(BaseColor.X - 0.2f, BaseColor.Y - 0.2f, BaseColor.Z - 0.2f, BaseColor.W);
                BaseComponent.SetColor(hoveredColor);
            }
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
