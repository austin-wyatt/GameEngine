﻿using MortalDungeon.Engine_Classes.UIComponents;
using MortalDungeon.Objects;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace MortalDungeon.Engine_Classes.TextHandling
{
    public class Text : UIObject
    {
        public string TextString = "";
        private Texture _texture;

        public Vector2 TextDimensions;

        private string _font;
        private int _fontSize;
        private Brush _fontColor;

        private UIScale _fontBaseScale = new UIScale();
        /// <summary>
        /// Scales the text object from it's default size
        /// </summary>
        public float TextScale = 1;

        public static string DEFAULT_FONT = "Moshita Mono";

        public Text(string text, string font, int fontSize, Brush fontColor)
        {
            TextString = text;

            _font = font;
            _fontSize = fontSize;
            _fontColor = fontColor;

            _baseObject = CreateBaseObject();
            AddBaseObject(_baseObject);

            int newLines = text.Split("\n").Length;

            _fontBaseScale = new UIScale(TextDimensions.X / (TextDimensions.Y / newLines), newLines);
            SetSize(new UIScale(1, 1));

            //RenderAfterParent = true;

            _baseObject.RenderData.AlphaThreshold = 0.7f;

            ValidateObject(this);
        }

        public void SetText(string text)
        {
            TextString = text;

            var oldScale = _fontBaseScale;
            var oldTexture = _texture;

            var baseObj = CreateBaseObject();
            baseObj.SetPosition(Position);
            _baseObject = baseObj;
            _baseObject.RenderData.AlphaThreshold = 0.7f;


            AddBaseObject(baseObj);

            BaseObjects.RemoveAt(0);
            oldTexture.Dispose();

            int newLines = text.Split("\n").Length;

            _fontBaseScale = new UIScale(TextDimensions.X / (TextDimensions.Y / newLines), newLines);
            SetSize(new UIScale(Size.X / oldScale.X, Size.Y / oldScale.Y));

            ForceTreeRegeneration();
        }

        public void SetTextScale(float scale)
        {
            TextScale = scale;
            SetSize(new UIScale(scale, scale));
        }

        public override void SetSize(UIScale size)
        {
            UIScale temp = new UIScale(size);

            temp.X *= _fontBaseScale.X;
            temp.Y *= _fontBaseScale.Y;

            base.SetSize(temp);
        }

        public override void OnResize()
        {
            SetSize(new UIScale(Size.X / _fontBaseScale.X, Size.Y / _fontBaseScale.Y));
        }

        public BaseObject CreateBaseObject()
        {
            (var texture, var dimensions) = TextBuilder.DrawString(TextString, _font, _fontSize, _fontColor);

            TextDimensions = dimensions;

            _texture = texture;

            Spritesheet temp = new Spritesheet() { Columns = 1, Rows = 1 };

            var baseObj = CreateBaseObjectFromSpritesheet(temp, 0);
            baseObj.BaseFrame.Material.Diffuse = texture;
            baseObj.BaseFrame.Textures.TextureIds[0] = texture.TextureId;

            baseObj.BaseFrame.CameraPerspective = false;

            TextureLoaded = true;

            return baseObj;
        }


        public override void CleanUp()
        {
            base.CleanUp();

            _texture.Dispose();
        }

    }
}
