using Empyrean.Engine_Classes.Text;
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
        public FontInfo FontInfo = UIManager.DEFAULT_FONT_INFO_16;
        public UIDimensions TextOffset = new UIDimensions(20, 30);
        public bool CenterText = false;

        public int _cursorIndex = 0;

        public TextString _textBox;

        public Cursor _cursorObject;

        public bool WordWrap = false;
        public int Lines = 1;
        public int Columns = 25;

        private int _lineCount = 0;

        public Action<string> OnTypeAction = null;

        public Input(Vector3 position, UIScale size, string text, FontInfo fontInfo, bool centerText = false, UIDimensions textOffset = default, 
            Vector4 textColor = default)
        {
            FontInfo = fontInfo;
            Size = size;
            Position = position;
            Name = "Input";
            CenterText = centerText;

            if(textColor == default)
            {
                textColor = _Colors.White;
            }

            if (textOffset != default)
            {
                TextOffset = textOffset;
            }

            Focusable = true;

            _cursorIndex = text.Length;

            Typeable = true;

            UIBlock block = new UIBlock(position, size);
            block.SetRender(false);

            TextString textString = new TextString(fontInfo)
            {
                TextColor = textColor,
                VerticalAlignment = VerticalAlignment.Top
            };

            BaseComponent = block;
            _textBox = textString;

            AddChild(block);

            AddTextString(textString);
            textString.SetText(text);

            _textBox.SetPosition(GAP(UIAnchorPosition.BottomLeft) + new Vector3(0, -15, 0));

            _cursorObject = new Cursor(textString.Position, size.Y / 2);

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
            _textBox.SetText("");

            _cursorIndex = 0;
            SetCursorPosition();
        }

        public override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            base.OnKeyDown(e);

            string typedLetter = TextHelper.KeyStrokeToString(e);
            string currString = _textBox.Text;

            _cursorObject.SetRender(true);
            _cursorObject.PropertyAnimations[0].Restart();

            bool change = false;

            if (_cursorIndex > currString.Length)
            {
                change = true;
                _cursorIndex = 0;
            }

            if (typedLetter.Length > 0)
            {
                if (e.Control)
                {
                    switch (e.Key)
                    {
                        case Keys.V:
                            string clipboardText = ClipboardHelper.GetText();

                            _textBox.SetText(currString.Substring(0, _cursorIndex) + clipboardText + currString.Substring(_cursorIndex));
                            return;
                        case Keys.C:
                            ClipboardHelper.SetText(currString);
                            return;
                    }
                }


                if (currString.Length <= Columns * Lines)
                {
                    if (typedLetter == "\n")
                    {
                        if (_lineCount < Lines - 1)
                        {
                            _textBox.SetText(currString.Substring(0, _cursorIndex) + typedLetter + currString.Substring(_cursorIndex, currString.Length - _cursorIndex));
                            _cursorIndex++;
                            _lineCount++;
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

                            change = true;
                        }
                        break;
                    case Keys.Delete:
                        if (_cursorIndex < currString.Length)
                        {
                            _textBox.SetText(currString.Remove(_cursorIndex, 1));

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
                        _cursorIndex = _textBox.Text.Length;
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
                else if (_cursorIndex > _textBox.Text.Length)
                {
                    _cursorIndex = currString.Length;
                }

                SetCursorPosition();
            }
        }

        public void SetCursorPosition() 
        {
            if (_textBox.Text.Length > 0 && _cursorIndex != 0)
            {
                Vector3 pos = new Vector3(_textBox.Characters[_cursorIndex - 1].NextCharXPosition(),
                    _textBox.Position.Y - _textBox.Characters[_cursorIndex - 1].Glyph.Descender, 
                    _textBox.Position.Z);

                _cursorObject.SAP(pos, UIAnchorPosition.BottomRight);
            }
            else if(_textBox.Text.Length > 0 && _cursorIndex == 0)
            {
                Vector3 pos = new Vector3(_textBox.Characters[0].PrevCharXPosition(),
                    _textBox.Position.Y - _textBox.Characters[0].Glyph.Descender,
                    _textBox.Position.Z);

                _cursorObject.SAP(pos, UIAnchorPosition.BottomRight);
            }
            else 
            {
                float descender = _textBox.GetDescender();
                _cursorObject.SAP(_textBox.Position - new Vector3(0, descender, 0), UIAnchorPosition.BottomLeft);
            }

            if (!ScissorBounds.InBoundingArea(_cursorObject.Position))
            {
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
