using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.MiscOperations;
using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Game.GameObjects;
using MortalDungeon.Game.Objects;
using MortalDungeon.Game.Tiles;
using MortalDungeon.Objects;
using OpenTK.Mathematics;
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

            
            RenderableObject obj = new RenderableObject(new SpritesheetObject(0, Spritesheets.TestSheet, 10, 10).CreateObjectDefinition(true), WindowConstants.FullColor, ObjectRenderType.Texture, Shaders.DEFAULT_SHADER);

            int imageSize = 512 * 512 * 4;

            float[] imageData = new float[imageSize];

            for (int i = 0; i < imageSize; i++) 
            {
                if (i % 4 == 0)
                {
                    imageData[i] = 1;
                }
                else if ((i + 3) % 4 == 0) 
                {
                    imageData[i] = 1;
                }
            }

            obj.TextureReference = Texture.LoadFromArray(imageData, new Vector2i(512, 512), true);

            Animation Idle = new Animation()
            {
                Frames = new List<RenderableObject>() { obj },
                Frequency = -1,
                Repeats = -1
            };

            BaseObject baseObj = new BaseObject(new List<Animation>() { Idle }, 0, "", new Vector3());


            GameObject temp = new GameObject();
            temp.BaseObjects.Add(baseObj);

            temp.BaseObjects[0].BaseFrame.CameraPerspective = false;

            temp.SetPosition(WindowConstants.CenterScreen);

            _genericObjects.Add(temp);
        }
    }
}
