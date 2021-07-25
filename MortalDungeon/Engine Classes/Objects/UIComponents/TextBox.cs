using MortalDungeon.Game.Objects;
using OpenTK.Mathematics;

namespace MortalDungeon.Game.UI
{
    public class TextBox : UIObject
    {
        public float TextScale = 1f;
        //public float TitleScale = 1.5f;
        public Vector3 TextOffset = new Vector3(20, 30, 0);
        public bool CenterText = false;

        public UIBlock _mainBlock;

        public Text TextField;

        public TextBox(Vector3 position, Vector2 size, string text, float textScale = 1, bool centerText = false, bool cameraPerspective = false)
        {
            TextScale = textScale;
            Size = size;
            Position = position;
            Name = "TextBox";
            CenterText = centerText;
            CameraPerspective = cameraPerspective;

            UIBlock block = new UIBlock(Position, new Vector2(Size.X / WindowConstants.ScreenUnits.X, Size.Y / WindowConstants.ScreenUnits.Y), default, 71, true, cameraPerspective);
            block.SetColor(new Vector4(0.2f, 0.2f, 0.2f, 1));

            Text textObj;
            if (CenterText)
            {
                textObj = new Text(text, block.Position, cameraPerspective);
            }
            else
            {
                textObj = new Text(text, block.Origin + TextOffset, cameraPerspective);
            }

            textObj.SetScale(textScale);

            if (CenterText)
            {
                Vector2 textDimensions = textObj.GetTextDimensions();
                textObj.SetPosition(new Vector3(block.Position.X - textDimensions.X / 2, block.Position.Y, block.Position.Z));
            }

            TextObjects.Add(textObj);

            TextField = textObj;

            AddChild(block);
            _mainBlock = block;

            block.OnClickAction = () =>
            {
                //Console.WriteLine(block.Origin);
            };
        }

        public override void SetPosition(Vector3 position)
        {
            base.SetPosition(position);
            TextObjects.ForEach(obj =>
            {
                if (CenterText)
                {
                    Vector2 textDimensions = obj.GetTextDimensions();
                    obj.SetPosition(new Vector3(_mainBlock.Position.X - textDimensions.X / 2, _mainBlock.Position.Y, _mainBlock.Position.Z));
                }
                else
                {
                    obj.SetPosition(_mainBlock.Origin + TextOffset);
                }
            });
        }
        public override void SetColor(Vector4 color)
        {
            _mainBlock.SetColor(color);
        }

        public void SetTextColor(Vector4 color)
        {
            TextObjects.ForEach(obj => obj.SetColor(color));
        }
    }
}



