using MortalDungeon.Game.Objects;
using OpenTK.Mathematics;

namespace MortalDungeon.Engine_Classes.UIComponents
{
    public class TextBox : UIObject
    {
        public float TextScale = 1f;
        //public float TitleScale = 1.5f;
        public Vector3 TextOffset = new Vector3(20, 30, 0);
        public bool CenterText = false;

        public Text TextField;

        public TextBox(Vector3 position, Vector2 size, string text, float textScale = 1, bool centerText = false, Vector3 textOffset = default)
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

            Text textObj;
            if (CenterText)
            {
                textObj = new Text(text, block.Position);
            }
            else
            {
                textObj = new Text(text, block.Origin + TextOffset);
            }

            textObj.SetScale(textScale);
            Vector2 textDimensions = textObj.GetTextDimensions();
            Vector3 blockDimensions = block.GetDimensions();
            if (CenterText)
            {
                textObj.SetPosition(new Vector3(block.Position.X - textDimensions.X / 2, block.Position.Y, block.Position.Z));
            }
            else
            {
                textObj.SetPosition(new Vector3(block.Position.X + TextOffset.X - blockDimensions.X / 2, block.Position.Y - blockDimensions.Y / 2 + TextOffset.Y, block.Position.Z));
            }

            TextObjects.Add(textObj);

            TextField = textObj;
            BaseComponent = block;


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
                    Vector2 textDimensions = obj.GetTextDimensions();
                    obj.SetPosition(new Vector3(BaseComponent.Position.X - textDimensions.X / 2, BaseComponent.Position.Y, BaseComponent.Position.Z));
                }
                else
                {
                    //obj.SetPosition(_mainBlock.Origin + TextOffset);

                    Vector3 blockDimensions = BaseComponent.GetDimensions();
                    obj.SetPosition(new Vector3(BaseComponent.Position.X + TextOffset.X - blockDimensions.X / 2, BaseComponent.Position.Y - blockDimensions.Y / 2 + TextOffset.Y, BaseComponent.Position.Z));
                }
            });
        }

        //public void SetSize(Vector2 size)
        //{
        //    float aspectRatio = (float)WindowConstants.ClientSize.Y / WindowConstants.ClientSize.X;

        //    Vector2 ScaleFactor = new Vector2(size.X, size.Y);
        //    BaseComponent._baseObject.BaseFrame.SetScaleAll(1);

        //    BaseComponent._baseObject.BaseFrame.ScaleX(aspectRatio);
        //    BaseComponent._baseObject.BaseFrame.ScaleX(ScaleFactor.X);
        //    BaseComponent._baseObject.BaseFrame.ScaleY(ScaleFactor.Y);

        //    Size = size;
        //    SetOrigin(aspectRatio, Size);
        //}

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



