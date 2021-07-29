using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.MiscOperations;
using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Engine_Classes.UIComponents;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;

namespace MortalDungeon.Game.SceneDefinitions
{
    class EscapeMenuScene : Scene
    {
        public EscapeMenuScene(Action exitFunc)
        {
            InitializeFields();

            ExitFunc = exitFunc;
        }

        public override void Load(Camera camera = null, BaseObject cursorObject = null, MouseRay mouseRay = null)
        {
            base.Load(camera, cursorObject, mouseRay);
           
            UIBlock escapeMenu = new UIBlock(WindowConstants.CenterScreen) { Clickable = true, Hoverable = true };
            UIDimensions menuDimensions = escapeMenu.GetDimensions();

            Button exitButton = new Button(escapeMenu.Origin + new Vector3(menuDimensions.X / 2, menuDimensions.Y / 4, 0), new UIScale(0.5f, 0.15f), "EXIT");
            exitButton.OnClickAction = () =>
            {
                ExitFunc?.Invoke();
            };
            escapeMenu.AddChild(exitButton);

            Button testButton = new Button(exitButton.Position + new Vector3(0, exitButton.GetDimensions().Y * 2, 0), new UIScale(0.5f, 0.15f), "TEST");
            testButton.OnClickAction = () =>
            {
                //testButton._mainObject._mainBlock.SetSize(testButton._mainObject._mainBlock.Size * 1.05f); //UI resizing example
            };

            escapeMenu.AddChild(testButton);



            Backdrop backdropModal = new Backdrop(new Vector3(0, 0, 0), new UIScale(WindowConstants.ScreenUnits.X * 5f, WindowConstants.ScreenUnits.Y * 5f), default, 90, false);

            //AddUI(backdropModal);
            AddUI(escapeMenu, 100);
            Vector4 slightlyTransparentBackdropColor = new Vector4(0.25f, 0.25f, 0.25f, 0.3f);
            backdropModal.SetColor(slightlyTransparentBackdropColor);

            escapeMenu.Render = false;
            backdropModal.Render = false;
        }

        private bool MenuOpen = false;

        public override bool onKeyDown(KeyboardKeyEventArgs e)
        {
            if (!base.onKeyDown(e))
            {
                return false;
            }

            if (e.Key == Keys.Escape && !e.IsRepeat) 
            {
                MenuOpen = !MenuOpen;
                _UI.ForEach(ui =>
                {
                    ui.Render = MenuOpen;
                });

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
            }

            return true;
        }
    }
}
