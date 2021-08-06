using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Engine_Classes.UIComponents
{
    public class TextComponent : UIObject
    {
        public Text _textField;
        public TextComponent() 
        {
            UIBlock mainBlock = new UIBlock(new Vector3(0,0,0));
            mainBlock.MultiTextureData.MixPercent = 0.1f;
            mainBlock.MultiTextureData.MixTexture = false;
            //mainBlock.SetColor(Colors.Red);
            mainBlock.SetColor(Colors.Transparent);
            mainBlock.SetAllInline(0);
            mainBlock.SetRender(true);

            BaseComponent = mainBlock;

            AddChild(BaseComponent, -10);


            _textField = new Text();

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

            switch (anchorPosition)
            {
                case UIAnchorPosition.TopCenter:
                    returnDim.Y -= dimensions.Y / 2;
                    returnDim.X += dimensions.X / 2;
                    break;
                case UIAnchorPosition.TopLeft:
                    returnDim.Y -= dimensions.Y / 2;
                    break;
                case UIAnchorPosition.TopRight:
                    returnDim.Y -= dimensions.Y / 2;
                    returnDim.X += dimensions.X;
                    break;
                case UIAnchorPosition.LeftCenter:
                    returnDim.X += dimensions.X / 2;
                    break;
                case UIAnchorPosition.RightCenter:
                    returnDim.X += dimensions.X;
                    break;
                case UIAnchorPosition.BottomCenter:
                    returnDim.Y += dimensions.Y / 2;
                    returnDim.X += dimensions.X / 2;
                    break;
                case UIAnchorPosition.BottomLeft:
                    returnDim.Y += dimensions.Y / 2;
                    break;
                case UIAnchorPosition.BottomRight:
                    returnDim.Y += dimensions.Y / 2;
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

        public override Vector3 GetAnchorPosition(UIAnchorPosition anchorPosition, Vector3 position)
        {
            //Vector3 temp = base.GetAnchorPosition(anchorPosition, position);

            //temp.X += GetDimensions().X / 2;
            //temp.Y += GetDimensions().Y / 2;

            return BaseComponent.GetAnchorPosition(anchorPosition);
        }

        public void SetTextPosition(Vector3 position) 
        {
            _textField.SetPosition(position);
        }

        public new void SetColor(Vector4 color) 
        {
            _textField.SetColor(color);
        }
    }
}
