using Empyrean.Engine_Classes.TextHandling;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Drawing;

namespace Empyrean.Engine_Classes.UIComponents
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

        public Input(Vector3 position, UIScale size, string text, int textScale = 16, bool centerText = false, UIDimensions textOffset = default)
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


            Text textBox = new Text(text, Text.DEFAULT_FONT, textScale, Brushes.Black);
            BaseComponent = textBox;
            _textBox = textBox;

            AddChild(textBox);

            _cursorObject = new Cursor(textBox.Position, size.Y / 2);

            AddChild(_cursorObject, 100);

            SetCursorPosition();
            UpdateScissorBounds();

            ValidateObject(this);
        }

        /// <summary>
        /// OnSubmit is called when "enter" is pressed on key down
        /// </summary>
        public event EventHandler OnSubmit;

        public void Clear()
        {
            Vector3 topLeftPos = _textBox.GAP(UIAnchorPosition.TopLeft);

            _textBox.SetText("");
            _textBox.SAP(topLeftPos, UIAnchorPosition.TopLeft);

            _cursorIndex = 0;
            SetCursorPosition();
        }

        public override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            base.OnKeyDown(e);

            string typedLetter = TextHelper.KeyStrokeToString(e);
            string currString = _textBox.TextString;

            _cursorObject.SetRender(true);
            _cursorObject.PropertyAnimations[0].Restart();

            Vector3 topLeftPos = _textBox.GAP(UIAnchorPosition.TopLeft);

            bool change = false;

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
                            _textBox.SAP(topLeftPos, UIAnchorPosition.TopLeft);
                            change = true;
                        }
                        else
                        {
                            OnSubmit.Invoke(this, EventArgs.Empty);
                        }
                    }
                    else
                    {
                        _textBox.SetText(currString.Substring(0, _cursorIndex) + typedLetter + currString.Substring(_cursorIndex, currString.Length - _cursorIndex));
                        _cursorIndex++;
                        _textBox.SAP(topLeftPos, UIAnchorPosition.TopLeft);
                        change = true;
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
                            _textBox.SAP(topLeftPos, UIAnchorPosition.TopLeft);

                            change = true;
                        }
                        break;
                    case Keys.Delete:
                        if (_cursorIndex < currString.Length)
                        {
                            _textBox.SetText(currString.Remove(_cursorIndex, 1));
                            _textBox.SAP(topLeftPos, UIAnchorPosition.TopLeft);

                            change = true;
                        }
                        break;
                    case Keys.Right:
                        _cursorIndex++;

                        change = true;
                        break;
                    case Keys.Left:
                        _cursorIndex--;

                        change = true;
                        break;
                    case Keys.Home:
                        _cursorIndex = 0;
                        change = true;
                        break;
                    case Keys.End:
                        _cursorIndex = _textBox.TextString.Length;
                        change = true;
                        break;
                    case Keys.Escape:
                        OnFocusEnd();
                        break;
                }
            }

            if (change)
            {
                if (_cursorIndex < 0)
                {
                    _cursorIndex = 0;
                }
                else if (_cursorIndex > _textBox.TextString.Length)
                {
                    _cursorIndex = currString.Length;
                }

                SetCursorPosition();
            }
                
        }

        public void SetCursorPosition() 
        {
            Vector3 textBoxLeftCenter = _textBox.GetAnchorPosition(UIAnchorPosition.LeftCenter);

            if (_textBox.TextString.Length > 0 && _cursorIndex != 0)
            {
                //Vector3 textDimensions = _textBox.TextField._textField.Letters[_cursorIndex - 1].GetDimensions();

                //if (_textBox.TextField._textField.Letters[_cursorIndex - 1].Character == Character.NewLine)
                //{
                //    _cursorObject.SetPosition(_textBox.TextField._textField.Letters[_cursorIndex - 1].Position - textDimensions.X / 1.95f * Vector3.UnitX);
                //}
                //else
                //{
                    _cursorObject.SAP(new Vector3(textBoxLeftCenter.X + (float)_cursorIndex / _textBox.TextString.Length * 
                       _textBox.Size.ToDimensions().X * 0.25f, Position.Y, Position.Z), UIAnchorPosition.LeftCenter);
                //}
            }
            else
            {
                _cursorObject.SetPosition(textBoxLeftCenter);
            }

            if (!ScissorBounds.InBoundingArea(_cursorObject.Position))
            {

                //_textBox.SetPosition(_cursorObject.Position);
                UpdateScissorBounds();
            }
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
