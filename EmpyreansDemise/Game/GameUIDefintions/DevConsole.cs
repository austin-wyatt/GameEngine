using Empyrean.Engine_Classes;
using Empyrean.Engine_Classes.Scenes;
using Empyrean.Engine_Classes.UIComponents;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Empyrean.Game.Scripting;
using System.Linq;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Empyrean.Game.UI
{
    public class DevConsole
    {
        private const int HISTORY_SIZE_BASE = 20;
        private const int HISTORY_SIZE_MAX = 40;

        private List<string> _historyBackStack = new List<string>();
        private List<string> _historyForwardStack = new List<string>();

        public bool Visible = false;

        public int HistoryIndex = 0;

        public Input InputComponent;
        private UIBlock _background;

        private CombatScene _scene;

        public DevConsole(CombatScene scene) 
        {
            UIScale consoleSize = new UIScale(2 * WindowConstants.AspectRatio - 0.1f, 0.1f);

            InputComponent = new Input(default, consoleSize, "", textColor: Brushes.Wheat);
            _background = new UIBlock(default, consoleSize);

            Color col = Color.DarkSlateGray;
            _background.SetColor(new Vector4(col.R / 255f, col.G / 255f, col.B / 255f, 0.75f));

            InputComponent._textBox.BackgroundClearColor = col;

            InputComponent.Columns = 200;

            _background.AddChild(InputComponent);
            _background.Focusable = true;
            _background.Clickable = true;
            _background.FocusHandle = InputComponent;
            

            _scene = scene;
            //_scene.AddUI(InputComponent, 9999999);
            _scene.AddUI(_background, 9999999);

            _scene.Tick += InputComponent.Tick;
            InputComponent.OnCleanUp += (GameObject obj) =>
            {
                _scene.Tick -= InputComponent.Tick;
            };

            _background.SAP(new Vector3(10, 10, 0), UIAnchorPosition.TopLeft);
            InputComponent.SAP(_background.GAP(UIAnchorPosition.LeftCenter) + new Vector3(5, 0, 0), UIAnchorPosition.LeftCenter);

            InputComponent.OnSubmit += OnSubmit;
            InputComponent.KeyDown += InputKeyDown;
        }

        public void ToggleVisibility()
        {
            //InputComponent.Render = !InputComponent.Render;
            _background.Render = !_background.Render;
            Visible = _background.Render;
        }

        public void OnSubmit(object source, EventArgs e)
        {
            object item = JSManager.EvaluateScript<object>(InputComponent._textBox.TextString);
            string output = item != null ? item.ToString() : "";

            Console.WriteLine(output);

            if(output != "")
            {
                _scene.EventLog.AddEvent(output);
            }
            
            AddToBackwardStack(InputComponent._textBox.TextString);
            //_historyForwardStack.Clear();

            InputComponent.Clear();
        }

        public void InputKeyDown(object source, KeyboardKeyEventArgs e)
        {
            Vector3 leftCenterPos;

            switch (e.Key)
            {
                case Keys.F11:
                    ToggleVisibility();
                    InputComponent.OnFocusEnd();

                    Window.QueueToRenderCycle(() => _scene.EndObjectFocus(null));
                    break;
                case Keys.Up:
                    if(_historyBackStack.Count > 0)
                    {
                        leftCenterPos = _background.GAP(UIAnchorPosition.LeftCenter) + Vector3.UnitX * 5;

                        AddToForwardStack(InputComponent._textBox.TextString);

                        InputComponent._textBox.SetText(_historyBackStack[^1]);
                        _historyBackStack.RemoveAt(_historyBackStack.Count - 1);

                        InputComponent._textBox.SAP(leftCenterPos, UIAnchorPosition.LeftCenter);
                    }
                    break;
                case Keys.Down:
                    leftCenterPos = _background.GAP(UIAnchorPosition.LeftCenter) + Vector3.UnitX * 5;

                    if (InputComponent._textBox.TextString != "")
                    {
                        AddToBackwardStack(InputComponent._textBox.TextString);
                    }
                        

                    if (_historyForwardStack.Count > 0) 
                    {
                        InputComponent._textBox.SetText(_historyForwardStack[^1]);
                        _historyForwardStack.RemoveAt(_historyForwardStack.Count - 1);
                    }
                    else
                    {
                        InputComponent._textBox.SetText("");
                    }

                    InputComponent._textBox.SAP(leftCenterPos, UIAnchorPosition.LeftCenter);

                    break;
            }
        }

        private void AddToForwardStack(string text)
        {
            if (text == "" || text == null)
                return;

            if (_historyForwardStack.Count > 0 && _historyForwardStack[^1] == text)
                return;

            _historyForwardStack.Add(text);
            if(_historyForwardStack.Count > HISTORY_SIZE_MAX)
            {
                _historyForwardStack.RemoveRange(0, HISTORY_SIZE_MAX - HISTORY_SIZE_BASE);
            }
        }

        private void AddToBackwardStack(string text)
        {
            if (text == "" || text == null) 
                return;

            if (_historyBackStack.Count > 0 && _historyBackStack[^1] == text)
                return;

            _historyBackStack.Add(text);

            if (_historyBackStack.Count > HISTORY_SIZE_MAX)
            {
                _historyBackStack.RemoveRange(0, HISTORY_SIZE_MAX - HISTORY_SIZE_BASE);
            }
        }
    }
}
