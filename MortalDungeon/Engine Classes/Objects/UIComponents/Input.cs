using MortalDungeon.Engine_Classes.TextHandling;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Drawing;

namespace MortalDungeon.Engine_Classes.UIComponents
{
    public class Input : UIObject
    {
        public float TextScale = 1f;
        public UIDimensions TextOffset = new UIDimensions(20, 30);
        public bool CenterText = false;

        public int _cursorIndex = 0;

        public Text _textBox;

        public Cursor _cursorObject;

        public bool WordWrap = false;
        public int Lines = 1;
        public int Columns = 25;

        private int _lineCount = 0;

        public Action<string> OnTypeAction = null;

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

            _cursorIndex = text.Length;

            Typeable = true;


            Text textBox = new Text(text, Text.DEFAULT_FONT, 48, Brushes.Black);
            textBox.SetTextScale(TextScale);
            BaseComponent = textBox;
            _textBox = textBox;

            AddChild(textBox);

            _cursorObject = new Cursor(textBox.Position, textScale);

            AddChild(_cursorObject, 100);


            SetCursorPosition();
            UpdateScissorBounds();

            ValidateObject(this);
        }

        public override void OnKeyUp(KeyboardKeyEventArgs e)
        {
            base.OnType(e);
            string typedLetter = TextHelper.KeyStrokeToString(e);
            string currString = _textBox.TextString;

            _cursorObject.SetRender(true);
            _cursorObject.PropertyAnimations[0].Restart();


            if (typedLetter.Length > 0)
            {
                if (currString.Length <= Columns * Lines) 
                {
                    if (typedLetter == "\n")
                    {
                        if (_lineCount < Lines - 1) 
                        {
                            _textBox.SetText(currString.Substring(0, _cursorIndex) + typedLetter + currString.Substring(_cursorIndex, currString.Length - _cursorIndex));
                            _cursorIndex++;
                            _lineCount++;
                        }
                            
                    }
                    else
                    {
                        _textBox.SetText(currString.Substring(0, _cursorIndex) + typedLetter + currString.Substring(_cursorIndex, currString.Length - _cursorIndex));
                        _cursorIndex++;
                    }
                }
            }
            else 
            {
                switch (e.Key) 
                {
                    case Keys.Backspace:
                        if (_cursorIndex > 0) 
                        {
                            if (currString[_cursorIndex - 1] == '\n') 
                            {
                                _lineCount--;
                            }

                            _textBox.SetText(currString.Remove(_cursorIndex - 1, 1));
                            _cursorIndex--;
                        }
                        break;
                    case Keys.Delete:
                        if (_cursorIndex < currString.Length)
                        {
                            _textBox.SetText(currString.Remove(_cursorIndex, 1));
                        }
                        break;
                    case Keys.Home:
                        _cursorIndex = 0;
                        break;
                    case Keys.End:
                        _cursorIndex = currString.Length;
                        break;
                    case Keys.Escape:
                        OnFocusEnd();
                        break;
                    case Keys.Right:
                        _cursorIndex++;
                        break;
                    case Keys.Left:
                        _cursorIndex--;
                        break;
                }

                if (_cursorIndex < 0)
                {
                    _cursorIndex = 0;
                }
                else if (_cursorIndex > currString.Length) 
                {
                    _cursorIndex = currString.Length;
                }
            }

            OnTypeAction?.Invoke(_textBox.TextString);

            SetCursorPosition();
        }

        public void SetCursorPosition() 
        {
            //if (_textBox._textField.Letters.Count > 0 && _cursorIndex != 0)
            //{
            //    Vector3 textDimensions = _textBox.TextField._textField.Letters[_cursorIndex - 1].GetDimensions();

            //    if (_textBox.TextField._textField.Letters[_cursorIndex - 1].Character == Character.NewLine)
            //    {
            //        _cursorObject.SetPosition(_textBox.TextField._textField.Letters[_cursorIndex - 1].Position - textDimensions.X / 1.95f * Vector3.UnitX);
            //    }
            //    else 
            //    {
            //        _cursorObject.SetPosition(_textBox.TextField._textField.Letters[_cursorIndex - 1].Position + textDimensions.X / 2 * Vector3.UnitX);
            //    }
            //}
            //else 
            //{
            //    _cursorObject.SetPosition(_textBox.GetAnchorPosition(UIAnchorPosition.LeftCenter) + (_textBox.TextOffset.X / 2) * Vector3.UnitX);
            //}

            //if (!ScissorBounds.InBoundingArea(_cursorObject.Position)) 
            //{
                
            //    //_textBox.SetPosition(_cursorObject.Position);
            //    UpdateScissorBounds();
            //}
        }

        

        public override void OnFocus()
        {
            base.OnFocus();
            _cursorObject.SetRender(true);
            _cursorObject.PropertyAnimations[0].Restart();
            _cursorObject.PropertyAnimations[0].Play();
            SetCursorPosition();
        }

        public override void OnFocusEnd()
        {
            base.OnFocusEnd();
            _cursorObject.PropertyAnimations[0].Reset();
            _cursorObject.SetRender(false);
        }
    }
}
