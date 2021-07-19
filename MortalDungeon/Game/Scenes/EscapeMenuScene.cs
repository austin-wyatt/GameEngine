using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.MiscOperations;
using MortalDungeon.Game.Objects;
using MortalDungeon.Game.UI;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Scenes
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

            //Environment.Exit(0);
           
            UIBlock escapeMenu = new UIBlock(WindowConstants.CenterScreen) { Clickable = true };

            Vector3 menuDimensions = escapeMenu.GetDimensions();

            Button exitButton = new Button(escapeMenu.Origin + new Vector3(menuDimensions.X / 2, menuDimensions.Y / 2, 0), new Vector2(500,150), "EXIT");
            exitButton.OnClickAction = () =>
            {
                ExitFunc?.Invoke();
            };
            escapeMenu.AddChild(exitButton);

            _UI.Add(escapeMenu);

            Backdrop backdropModal = new Backdrop(new Vector3(0,0,0), new Vector2(5f,5f), default, 90, false);
            _UI.Add(backdropModal);

            Vector4 slightlyTransparentBackdrop = new Vector4(0.25f, 0.25f, 0.25f, 0.75f);
            backdropModal.SetColor(slightlyTransparentBackdrop);

            escapeMenu.Render = false;
            backdropModal.Render = false;
        }


        public override void onKeyUp(KeyboardKeyEventArgs e)
        {
            base.onKeyUp(e);

            if (e.Key == Keys.Escape) 
            {
                _UI.ForEach(ui =>
                {
                    ui.Render = !ui.Render;
                });
            }
        }
    }
}
