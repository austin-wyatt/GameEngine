using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace MortalDungeon.Engine_Classes.UIComponents
{
    public class Input : UIObject
    {
        public float TextScale = 1f;
        public UIDimensions TextOffset = new UIDimensions(20, 30);
        public bool CenterText = false;

        public int _cursorPosition = 0;

        public TextBox _textBox;

        public Cursor _cursorObject;

        public Input(Vector3 position, UIScale size, string text, float textScale = 0.1f, bool centerText = false, UIDimensions textOffset = default)
        {
            TextScale = textScale;
            Size = size;
            Position = position;
            Name = "Input";
            CenterText = centerText;

            if (textOffset != default)
            {
                TextOffset = textOffset;
            }

            Focusable = true;

            _cursorPosition = text.Length;

            TextBox textBox = new TextBox(position, Size, text, textScale, centerText, textOffset);
            BaseComponent = textBox;
            _textBox = textBox;

            textBox.BaseComponent.MultiTextureData.MixTexture = false;

            AddChild(textBox);

            _cursorObject = new Cursor(textBox.TextField.Position, textScale);

            AddChild(_cursorObject, 100);

            ValidateObject(this);
        }

        public override void OnType(KeyboardKeyEventArgs e)
        {
            base.OnType(e);
            string typedLetter = TextHelper.KeyStrokeToString(e);
            string currString = _textBox.TextField.TextString;

            _cursorObject.Render = true;
            _cursorObject.PropertyAnimations[0].Restart();

            if (typedLetter.Length > 0)
            {
                _textBox.TextField.SetTextString(currString.Substring(0, _cursorPosition) + typedLetter + currString.Substring(_cursorPosition, currString.Length - _cursorPosition));
                _cursorPosition++;
            }
            else 
            {
                switch (e.Key) 
                {
                    case Keys.Backspace:
                        if (_cursorPosition > 0) 
                        {
                            _textBox.TextField.SetTextString(currString.Remove(_cursorPosition - 1, 1));
                            _cursorPosition--;
                        }
                        break;
                    case Keys.Delete:
                        if (_cursorPosition < currString.Length)
                        {
                            _textBox.TextField.SetTextString(currString.Remove(_cursorPosition, 1));
                        }
                        break;
                    case Keys.Home:
                        _cursorPosition = 0;
                        break;
                    case Keys.End:
                        _cursorPosition = currString.Length;
                        break;
                    case Keys.Escape:
                        EndFocus();
                        break;
                    case Keys.Right:
                        _cursorPosition++;
                        break;
                    case Keys.Left:
                        _cursorPosition--;
                        break;
                }

                if (_cursorPosition < 0)
                {
                    _cursorPosition = 0;
                }
                else if (_cursorPosition > currString.Length) 
                {
                    _cursorPosition = currString.Length;
                }
            }

            SetCursorPosition();
        }

        public void SetCursorPosition() 
        {
            if (_textBox.TextField.Letters.Count > 0 && _cursorPosition != 0)
            {
                Vector3 textDimensions = _textBox.TextField.Letters[_cursorPosition - 1].GetDimensions();

                if (_textBox.TextField.Letters[_cursorPosition - 1].Character == Character.NewLine)
                {
                    _cursorObject.SetPosition(_textBox.TextField.Letters[_cursorPosition - 1].Position - textDimensions.X / 1.95f * Vector3.UnitX);
                }
                else 
                {
                    _cursorObject.SetPosition(_textBox.TextField.Letters[_cursorPosition - 1].Position + textDimensions.X / 2 * Vector3.UnitX);
                }
            }
            else 
            {
                _cursorObject.SetPosition(_textBox.TextField.Position - new Vector3(_textBox.TextField.LetterOffset / 2.01f, 0, 0));
            }
        }

        public override void OnFocus()
        {
            base.OnFocus();
            _cursorObject.Render = true;
            _cursorObject.PropertyAnimations[0].Restart();
            _cursorObject.PropertyAnimations[0].Play();
            SetCursorPosition();
        }

        public override void EndFocus()
        {
            base.EndFocus();
            _cursorObject.PropertyAnimations[0].Stop();
            _cursorObject.Render = false;
        }
    }
}
