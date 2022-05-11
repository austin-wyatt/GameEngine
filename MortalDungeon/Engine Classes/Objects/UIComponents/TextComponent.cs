﻿using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Engine_Classes.UIComponents
{
    public class TextComponent : UIObject
    {
        public _Text _textField;
        public TextComponent(TextRenderData textRenderData = null) 
        {
            UIBlock mainBlock = new UIBlock(new Vector3(0,0,0));
            mainBlock.MultiTextureData.MixPercent = 0.1f;
            mainBlock.MultiTextureData.MixTexture = false;
            //mainBlock.SetColor(Colors.Red);
            mainBlock.SetColor(_Colors.Transparent);
            mainBlock.SetAllInline(0);
            mainBlock.SetRender(true);

            BaseComponent = mainBlock;

            AddChild(BaseComponent, -10);


            _textField = new _Text() { ScissorData = ScissorData };

            if(textRenderData != null) 
            {
                _textField.TextRenderData = textRenderData;
            }


            TextObjects.Add(_textField);

            SetTextPosition(mainBlock.Position);
        }

        public void SetTextScale(float textScale) 
        {
            _textField.SetTextScale(textScale);

            RescaleBaseComponent();
        }

        public void SetText(string text) 
        {
            _textField.SetTextString(text);

            RescaleBaseComponent();
        }

        public void RescaleBaseComponent() 
        {
            BaseComponent.SetSize(GetScale());
            BaseComponent.SetPositionFromAnchor(_textField.Position, UIAnchorPosition.LeftCenter);
        }

        public UIScale GetScale() 
        {
            UIDimensions dim = _textField.GetTextDimensions();
            dim.Y *= 2;
            dim.X *= 2 * WindowConstants.AspectRatio;

            return dim;
        }

        public override void SetPosition(Vector3 position)
        {
            Position = position;

            SetTextPosition(position);

            Vector3 pos = _textField.Position;

            if (_textField.Letters.Count > 0)
            {
                Vector3 dim = _textField.Letters[0].GetDimensions();
                pos.X -= dim.X / 2;
                pos.Y -= dim.Y / 2;
            }

            BaseComponent.SetPositionFromAnchor(pos, UIAnchorPosition.TopLeft);
        }

        public override void SetPositionFromAnchor(Vector3 position, UIAnchorPosition anchor = UIAnchorPosition.Center)
        {
            if (anchor == UIAnchorPosition.Center)
                anchor = Anchor;

            UIDimensions anchorOffset = GetAnchorOffset(anchor);

            _anchorOffset = anchorOffset;


            SetPosition(position - anchorOffset);
        }

        public override UIDimensions GetAnchorOffset(UIAnchorPosition anchorPosition)
        {
            UIDimensions dimensions = _textField.GetTextDimensions();
            UIDimensions returnDim = new UIDimensions();

            Vector3 letterDim = new Vector3();
            if (_textField.Letters.Count > 0) 
            {
                letterDim = _textField.Letters[0].GetDimensions();
            }

            switch (anchorPosition)
            {
                case UIAnchorPosition.TopCenter:
                    returnDim.Y -= dimensions.Y / 2;
                    returnDim.X += dimensions.X / 2;
                    break;
                case UIAnchorPosition.TopLeft:
                    returnDim.Y -= letterDim.Y / 2;
                    break;
                case UIAnchorPosition.TopRight:
                    returnDim.Y -= letterDim.Y / 2;
                    returnDim.X += dimensions.X;
                    break;
                case UIAnchorPosition.LeftCenter:
                    break;
                case UIAnchorPosition.RightCenter:
                    returnDim.X += dimensions.X;
                    break;
                case UIAnchorPosition.BottomCenter:
                    returnDim.Y += dimensions.Y;
                    returnDim.X += dimensions.X / 2;
                    break;
                case UIAnchorPosition.BottomLeft:
                    returnDim.Y += dimensions.Y;
                    break;
                case UIAnchorPosition.BottomRight:
                    returnDim.Y += dimensions.Y;
                    returnDim.X += dimensions.X;
                    break;
                case UIAnchorPosition.Center:
                    returnDim.X += dimensions.X / 2;
                    break;
                default:
                    break;
            }

            if (_textField.Letters.Count > 0)
            {
                Vector3 dim = _textField.Letters[0].GetDimensions();
                returnDim.X -= dim.X / 2;
            }

            return returnDim;
        }

        //public override Vector3 GetAnchorPosition(UIAnchorPosition anchorPosition, Vector3 position)
        //{
        //    //Vector3 temp = base.GetAnchorPosition(anchorPosition, position);

        //    //temp.X += GetDimensions().X / 2;
        //    //temp.Y += GetDimensions().Y / 2;

        //    Vector3 anchorPos = new Vector3(Position);

        //    anchorPos += GetAnchorOffset(anchorPosition);


        //    return BaseComponent.GetAnchorPosition(anchorPosition);
        //}

        public override Vector3 GetAnchorPosition(UIAnchorPosition anchorPosition, Vector3 position)
        {
            //Vector3 temp = base.GetAnchorPosition(anchorPosition, position);

            //temp.X += GetDimensions().X / 2;
            //temp.Y += GetDimensions().Y / 2;

            Vector3 anchorPos = new Vector3(Position);

            anchorPos += GetAnchorOffset(anchorPosition);


            return anchorPos;
        }

        public void SetTextPosition(Vector3 position) 
        {
            _textField.SetPosition(position);
        }

        public void SetColor(Vector4 color) 
        {
            _textField.SetColor(color);
        }
    }
}
