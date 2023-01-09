using Empyrean.Engine_Classes;
using Empyrean.Engine_Classes.MiscOperations;
using Empyrean.Engine_Classes.Rendering;
using Empyrean.Engine_Classes.Scenes;
using Empyrean.Game.Objects;
using Empyrean.Game.Tiles;
using Empyrean.Game.Tools;
using Empyrean.Objects;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Empyrean.Game.SceneDefinitions
{
    class ToolScene : Scene
    {
        public ToolScene()
        {
            InitializeFields();

            SceneContext = ContentContext.Tools;
        }


        public override void Load(Camera camera = null, MouseRay mouseRay = null)
        {
            base.Load(camera, mouseRay);


        }

        public override void OnUpdateFrame(FrameEventArgs args)
        {
            
        }

        public void RefreshUI()
        {
            foreach(var ui in UIManager.TopLevelObjects.ToArray())
            {
                RemoveUI(ui);
            }

            FeatureEditorUI featureEditorUI = new FeatureEditorUI(this);
        }

        public override bool OnKeyUp(KeyboardKeyEventArgs e)
        {
            switch (e.Key)
            {
                case Keys.F12:
                    RefreshUI();
                    break;
            }

            return base.OnKeyUp(e);
        }
    }
}
