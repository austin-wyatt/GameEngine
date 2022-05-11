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

        private UIScale _fontBaseScale = new UIScale();
        /// <summary>
        /// Scales the text object from it's default size
        /// </summary>
        public float TextScale = 1;

        //public static string DEFAULT_FONT = "Moshita Mono";
        public static string DEFAULT_FONT = "Arial";

        public Text(string text, string font, int fontSize, Brush fontColor)
        {
            TextString = text;

            _font = font;
            _fontSize = fontSize;
            _fontColor = fontColor;

            _canLoadTexture = false;

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
            if (text == TextString)
                return;

            TextureLoaded = false;

            lock (_textLoadLock)
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
                if(oldTexture != null)
                {
                    oldTexture.Dispose();
                }

                int newLines = text.Split("\n").Length;

                _fontBaseScale = new UIScale(TextDimensions.X / (TextDimensions.Y / newLines), newLines);
                SetSize(new UIScale(Size.X / oldScale.X, Size.Y / oldScale.Y));

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

            temp.X *= _fontBaseScale.X;
            temp.Y *= _fontBaseScale.Y;

            base.SetSize(temp);
        }

        public override void OnResize()
        {
            SetSize(new UIScale(Size.X / _fontBaseScale.X, Size.Y / _fontBaseScale.Y));
        }

        private object _textLoadLock = new object();
        public BaseObject CreateBaseObject()
        {
            Spritesheet temp = new Spritesheet() { Columns = 1, Rows = 1 };

            var baseObj = CreateBaseObjectFromSpritesheet(temp, 0);
            baseObj.BaseFrame.CameraPerspective = false;

            //TextureLoaded = false;
            //SetRender(false);

            if(_texture == null)
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
            });

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
