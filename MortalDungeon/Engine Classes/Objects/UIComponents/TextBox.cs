using MortalDungeon.Game.Objects;
using OpenTK.Mathematics;

namespace MortalDungeon.Engine_Classes.UIComponents
{
    public class TextBox : UIObject
    {
        public float TextScale = 1f;
        //public float TitleScale = 1.5f;
        public UIDimensions TextOffset = new UIDimensions(20, 30);
        public bool CenterText = false;

        public TextComponent TextField;

        public TextBox(Vector3 position, UIScale size, string text, float textScale = 0.1f, bool centerText = false, UIDimensions textOffset = default)
        {
            TextScale = textScale;
            Size = size;
            Position = position;
            Name = "TextBox";
            CenterText = centerText;

            if (textOffset != default) 
            {
                TextOffset = textOffset;
            }

            UIBlock block = new UIBlock(Position, Size, default, 71, true);
            block.SetColor(new Vector4(0.2f, 0.2f, 0.2f, 1));

            TextComponent textObj;
            if (CenterText)
            {
                //textObj = new Text(text, block.Position);
                textObj = new TextComponent();
                textObj.SetText(text);
                textObj.SetPositionFromAnchor(block.Position, UIAnchorPosition.Center);
            }
            else
            {
                //textObj = new Text(text, block.Origin + TextOffset);
                textObj = new TextComponent();
                textObj.SetText(text);
                textObj.SetPosition(block.Position);

            }

            textObj.SetTextScale(textScale);
            UIDimensions textDimensions = textObj.GetDimensions();
            UIDimensions blockDimensions = block.GetDimensions();
            if (CenterText)
            {
                textObj.SetPosition(new Vector3(block.Position.X - textDimensions.X / 2, block.Position.Y, block.Position.Z));
            }
            else
            {
                textObj.SetPosition(new Vector3(block.Position.X + TextOffset.X - blockDimensions.X / 2, block.Position.Y - blockDimensions.Y / 2 + TextOffset.Y, block.Position.Z));
            }

            //TextObjects.Add(textObj);

            TextField = textObj;
            BaseComponent = block;

            AddChild(textObj, 10);
            AddChild(block);

            block.OnClickAction = () =>
            {
                //Console.WriteLine(block.Origin);
            };

            ValidateObject(this);
        }

        public override void SetPosition(Vector3 position)
        {
            base.SetPosition(position);
            TextObjects.ForEach(obj =>
            {
                if (CenterText)
                {
                    UIDimensions textDimensions = obj.GetTextDimensions();
                    obj.SetPosition(new Vector3(BaseComponent.Position.X - textDimensions.X / 2 + + TextOffset.X / 2, BaseComponent.Position.Y, BaseComponent.Position.Z));
                }
                else
                {
                    //obj.SetPosition(_mainBlock.Origin + TextOffset);

                    UIDimensions blockDimensions = BaseComponent.GetDimensions();
                    obj.SetPosition(new Vector3(BaseComponent.Position.X + TextOffset.X - blockDimensions.X / 2, BaseComponent.Position.Y - blockDimensions.Y / 2 + TextOffset.Y, BaseComponent.Position.Z));
                }
            });
        }


        public override void SetColor(Vector4 color)
        {
            BaseComponent.SetColor(color);
        }

        public void SetTextColor(Vector4 color)
        {
            TextObjects.ForEach(obj => obj.SetColor(color));
        }
    }
}



