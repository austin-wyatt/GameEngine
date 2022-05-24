using Empyrean.Engine_Classes.UIComponents;
using Empyrean.Objects;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Empyrean.Engine_Classes.TextHandling
{
    public class Text : UIObject
    {
        public string TextString = "";
        private Texture _texture;

        public Vector2 TextDimensions;

        private string _font;
        private int _fontSize;
        private Brush _fontColor;

        /// <summary>
        /// Scales the text object from it's default size
        /// </summary>
        public float TextScale = 1;

        public float LineHeightMultiplier = 1;

        //public static string DEFAULT_FONT = "Moshita Mono";
        public static string DEFAULT_FONT = "Segoe UI";
        //public static string DEFAULT_FONT = "Arial";

        public Color BackgroundClearColor = Color.White;

        public Text(string text, string font, int fontSize, Brush fontColor, Color clearColor = default, float lineHeightMult = 1)
        {
            if (clearColor != default)
            {
                BackgroundClearColor = clearColor;
            }

            TextString = text;

            _font = font;
            _fontSize = fontSize;
            _fontColor = fontColor;

            _canLoadTexture = false;

            LineHeightMultiplier = lineHeightMult;

            _baseObject = CreateBaseObject();
            AddBaseObject(_baseObject);

            int newLines = text.Split("\n").Length;

            SetSize(new UIScale(1, 1));

            //RenderAfterParent = true;

            _baseObject.RenderData.AlphaThreshold = Rendering.RenderingConstants.TextAlphaThreshold;

            ValidateObject(this);
        }

        public void SetText(string text)
        {
            if (text == TextString)
                return;

            TextureLoaded = false;

            lock (_textLoadLock)
            {
                TextString = text;

                var oldTexture = _texture;

                var baseObj = CreateBaseObject();
                baseObj.SetPosition(Position);
                _baseObject = baseObj;
                _baseObject.RenderData.AlphaThreshold = Rendering.RenderingConstants.TextAlphaThreshold;

                AddBaseObject(baseObj);

                BaseObjects.RemoveAt(0);
                if(oldTexture != null)
                {
                    oldTexture.Dispose();
                }

                int newLines = text.Split("\n").Length;

                SetTextScale(TextScale);

                //ForceTreeRegeneration();
            }
        }

        public void SetTextScale(float scale)
        {
            TextScale = scale;
            SetSize(new UIScale(scale, scale));
        }

        public override void SetSize(UIScale size)
        {
            UIScale temp = new UIScale(size);

            temp.X *= TextDimensions.X * 2 / WindowConstants.ClientSize.Y;
            temp.Y *= TextDimensions.Y * 2 / WindowConstants.ClientSize.Y;

            base.SetSize(temp);
        }

        public override void OnResize()
        {
            SetTextScale(TextScale);
        }

        private object _textLoadLock = new object();
        public BaseObject CreateBaseObject()
        {
            Spritesheet temp = new Spritesheet() { Columns = 1, Rows = 1 };

            var baseObj = CreateBaseObjectFromSpritesheet(temp, 0);
            baseObj.BaseFrame.CameraPerspective = false;

            baseObj.RenderData.AlphaThreshold = Rendering.RenderingConstants.TextAlphaThreshold;

            //TextureLoaded = false;
            //SetRender(false);

            if (_texture == null)
            {
                TextureLoaded = false;
                SetRender(false);
            }


            var dimensions = TextBuilder.DrawString(TextString, _font, _fontSize, _fontColor, (texture) =>
            {
                lock (_textLoadLock)
                {
                    var oldTexture = _texture;

                    _texture = texture;
                    baseObj.BaseFrame.Material.Diffuse = texture;
                    baseObj.BaseFrame.Textures.TextureIds[0] = texture.TextureId;
                    SetRender(true);
                    TextureLoaded = true;

                    if (_cleanedUp)
                    {
                        _texture.Dispose();
                    }

                    if (oldTexture != null)
                    {
                        oldTexture.Dispose();
                    }

                    _texture = null;
                }
            }, BackgroundClearColor, LineHeightMultiplier);

            TextDimensions = dimensions;


            return baseObj;
        }

        private bool _cleanedUp = false;
        public override void CleanUp()
        {
            lock (_textLoadLock)
            {
                base.CleanUp();
                _cleanedUp = true;

                if (_texture != null)
                {
                    _texture.Dispose();
                }

                _texture = null;
            }
        }

    }
}
