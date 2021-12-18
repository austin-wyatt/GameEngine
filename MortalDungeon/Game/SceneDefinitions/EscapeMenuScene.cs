using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.Audio;
using MortalDungeon.Engine_Classes.MiscOperations;
using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Engine_Classes.UIComponents;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Diagnostics;

namespace MortalDungeon.Game.SceneDefinitions
{
    class EscapeMenuScene : Scene
    {

        private UIObject _backdrop;
        internal EscapeMenuScene(Action exitFunc)
        {
            InitializeFields();

            ExitFunc = exitFunc;
        }

        internal override void Load(Camera camera = null, BaseObject cursorObject = null, MouseRay mouseRay = null)
        {
            base.Load(camera, cursorObject, mouseRay);
           
            UIBlock escapeMenu = new UIBlock(WindowConstants.CenterScreen) { Clickable = true, Hoverable = true };
            UIDimensions menuDimensions = escapeMenu.GetDimensions();

            Button exitButton = new Button(escapeMenu.Origin + new Vector3(menuDimensions.X / 2, menuDimensions.Y / 4, 0), new UIScale(0.5f, 0.15f), "EXIT")
            {
                OnClickAction = () =>
                {
                    SoundPlayer.FreeAllSources();
                    ExitFunc?.Invoke();
                }
            };

            escapeMenu.AddChild(exitButton);

            Button testButton = new Button(exitButton.Position + new Vector3(0, exitButton.GetDimensions().Y * 2, 0), new UIScale(0.5f, 0.15f), "Toggle Vsync", 0.05f)
            {
                OnClickAction = () =>
                {
                    //testButton._mainObject._mainBlock.SetSize(testButton._mainObject._mainBlock.Size * 1.05f); //UI resizing example
                    //Settings.EnableTileTooltips = !Settings.EnableTileTooltips;
                    if (Settings.VsyncEnabled)
                    {
                        Settings.VsyncEnabled = false;
                        Program.Window.VSync = VSyncMode.Off;
                    }
                    else 
                    {
                        Settings.VsyncEnabled = true;
                        Program.Window.VSync = VSyncMode.On;
                    }

                }
            };

            escapeMenu.AddChild(testButton);


            Button loadButton = new Button(testButton.Position + new Vector3(0, testButton.GetDimensions().Y + 10, 0), new UIScale(0.5f, 0.15f), "Load Audio", 0.05f)
            {
                OnClickAction = () =>
                {
                    Sounds.Test.Load();
                }
            };

            escapeMenu.AddChild(loadButton);

            Button playButton = new Button(loadButton.Position + new Vector3(0, loadButton.GetDimensions().Y + 10, 0), new UIScale(0.5f, 0.15f), "Play", 0.05f)
            {
                OnClickAction = () =>
                {
                    Sound testSound = new Sound(Sounds.Test);

                    testSound.Play();
                }
            };

            escapeMenu.AddChild(playButton);

            Button disposeButton = new Button(playButton.Position + new Vector3(0, loadButton.GetDimensions().Y + 10, 0), new UIScale(0.5f, 0.15f), "Dispose", 0.05f)
            {
                OnClickAction = () =>
                {
                    //testSound.Dispose();
                    Sounds.Test.Unload();
                }
            };

            escapeMenu.AddChild(disposeButton);


            UIBlock backdropModal = new UIBlock(new Vector3(0, 0, 0), new UIScale(WindowConstants.ScreenUnits.X * 5f, WindowConstants.ScreenUnits.Y * 5f));
            backdropModal.MultiTextureData.MixTexture = false;
            Vector4 slightlyTransparentBackdropColor = new Vector4(0.25f, 0.25f, 0.25f, 0.75f);
            backdropModal.SetColor(slightlyTransparentBackdropColor);

            //_lowPriorityObjects.Add(backdropModal);
            _backdrop = backdropModal;

            AddUI(escapeMenu, 100);
            AddUI(backdropModal, 99);

            
            escapeMenu.SetRender(false);
            backdropModal.SetRender(false);

            backdropModal.LoadTexture();

            _menuCooldown.Start();

            void messageReciever(Message msg) 
            {
                if(msg.Flag == MessageFlag.OpenEscapeMenu) 
                {
                    ToggleMenu();
                }
            }

            OnMessageRecieved += messageReciever;
        }

        private bool MenuOpen = false;

        internal override bool OnKeyDown(KeyboardKeyEventArgs e)
        {
            if (!base.OnKeyDown(e))
            {
                return false;
            }

            if (e.Key == Keys.Escape && !e.IsRepeat) 
            {
                if (MenuOpen) 
                {
                    ToggleMenu();
                }
            }

            return true;
        }

        private Stopwatch _menuCooldown = new Stopwatch();
        private void ToggleMenu() 
        {
            if (_menuCooldown.ElapsedMilliseconds < 100) return;

            MenuOpen = !MenuOpen;
            _UI.ForEach(ui =>
            {
                ui.SetRender(MenuOpen);
            });

            _backdrop.SetRender(MenuOpen);

            Sound sound = new Sound(Sounds.Select) { Gain = 0.15f, Pitch = GlobalRandom.NextFloat(0.5f, 0.5f) };
            sound.Play();

            Message msg;

            if (MenuOpen)
            {
                msg = MessageCenter.CreateMessage(MessageType.Request, MessageBody.StopRendering, MessageTarget.UI);
                MessageCenter.SendMessage(msg);

                msg = MessageCenter.CreateMessage(MessageType.Request, MessageBody.InterceptClicks, MessageTarget.All);
                MessageCenter.SendMessage(msg);

                msg = MessageCenter.CreateMessage(MessageType.Request, MessageBody.InterceptKeyStrokes, MessageTarget.All);
                MessageCenter.SendMessage(msg);
            }
            else
            {
                msg = MessageCenter.CreateMessage(MessageType.Request, MessageBody.StartRendering, MessageTarget.UI);
                MessageCenter.SendMessage(msg);

                msg = MessageCenter.CreateMessage(MessageType.Request, MessageBody.EndClickInterception, MessageTarget.All);
                MessageCenter.SendMessage(msg);

                msg = MessageCenter.CreateMessage(MessageType.Request, MessageBody.EndKeyStrokeInterception, MessageTarget.All);
                MessageCenter.SendMessage(msg);
            }

            _menuCooldown.Restart();
        }
    }
}
