using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.MiscOperations;
using MortalDungeon.Engine_Classes.Rendering;
using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Game.GameObjects;
using MortalDungeon.Game.Objects;
using MortalDungeon.Game.Tiles;
using MortalDungeon.Objects;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MortalDungeon.Game.SceneDefinitions
{
    class WriteDataScene : Scene
    {
        public WriteDataScene()
        {
            InitializeFields();
        }


        public override void Load(Camera camera = null, MouseRay mouseRay = null)
        {
            base.Load(camera, mouseRay);


        }

        public override void OnUpdateFrame(FrameEventArgs args)
        {
            
        }

        public override bool OnKeyUp(KeyboardKeyEventArgs e)
        {
            if (e.Key == Keys.Escape)
            {
                Window.CloseWindow();
            }

            return base.OnKeyUp(e);
        }
    }
}
