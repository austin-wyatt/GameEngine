using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.MiscOperations;
using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Game.GameObjects;
using MortalDungeon.Game.Tiles;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.SceneDefinitions
{
    class BoundsTestScene : Scene
    {
        public BoundsTestScene()
        {
            InitializeFields();
        }

        public override void Load(Camera camera = null, BaseObject cursorObject = null, MouseRay mouseRay = null)
        {
            base.Load(camera, cursorObject, mouseRay);

            //load object here without camera perspective and use Q, R, and P to outline the object and get the bounds

            //BaseTile _tile = new BaseTile(WindowConstants.CenterScreen, 0);
            //_tile._tileObject.BaseFrame.CameraPerspective = false;
            //_genericObjects.Add(_tile);
        }
    }
}
