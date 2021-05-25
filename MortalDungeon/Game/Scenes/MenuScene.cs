using MortalDungeon.Engine_Classes;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Scenes
{
    class MenuScene : Scene
    {
        public MenuScene() 
        {
            _objects = new List<BaseObject>();
            _renderedObjects = new List<BaseObject>();
            _clickableObjects = new List<BaseObject>();
            ScenePosition = new Vector3(0, 0, 0);
        }
    }
}
