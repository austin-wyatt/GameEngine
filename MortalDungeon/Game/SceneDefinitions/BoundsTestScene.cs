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
    class BoundsTestScene : Scene
    {
        public BoundsTestScene()
        {
            InitializeFields();
        }

        //public static float[] imageData = new float[512 * 512 * 4];
        //public static Texture tex;

        public override void Load(Camera camera = null, BaseObject cursorObject = null, MouseRay mouseRay = null)
        {
            base.Load(camera, cursorObject, mouseRay);

            //load object here without camera perspective and use Q, R, and P to outline the object and get the bounds

            
            //RenderableObject obj = new RenderableObject(new SpritesheetObject(0, Spritesheets.TestSheet, 10, 10).CreateObjectDefinition(true), WindowConstants.FullColor, ObjectRenderType.Texture, Shaders.DEFAULT_SHADER);

            //int imageSize = 512 * 512 * 4;

            ////float[] imageData = new float[imageSize];

            //for (int i = 0; i < imageSize; i++) 
            //{
            //    imageData[i] = 1f;
            //}

            //tex = Texture.LoadFromArray(imageData, new Vector2i(512, 512), true);
            //obj.TextureReference = tex;
            //obj.TextureReference.TextureName = TextureName.DynamicTexture;

            //obj.Textures.Textures[0] = TextureName.DynamicTexture;

            //Renderer.LoadTextureFromTextureObj(obj.TextureReference, TextureName.DynamicTexture);

            //Animation Idle = new Animation()
            //{
            //    Frames = new List<RenderableObject>() { obj },
            //    Frequency = -1,
            //    Repeats = -1
            //};

            //BaseObject baseObj = new BaseObject(new List<Animation>() { Idle }, 0, "", new Vector3());


            //GameObject temp = new GameObject();
            //temp.BaseObjects.Add(baseObj);

            //temp.BaseObjects[0].BaseFrame.CameraPerspective = false;

            //temp.SetPosition(WindowConstants.CenterScreen);

            //_genericObjects.Add(temp);

            TileMap tileMap = new TileMap(default, new TileMapPoint(0, 0), _tileMapController) { Width = 50, Height = 50 };
            tileMap.PopulateTileMap();

            //_tileMapController.AddTileMap(new TileMapPoint(0, 0), tileMap);

            TileTexturer.InitializeTexture(tileMap);

            RenderableObject obj = new RenderableObject(new SpritesheetObject(0, Spritesheets.TestSheet, 10, 10).CreateObjectDefinition(true), WindowConstants.FullColor, ObjectRenderType.Texture, Shaders.DEFAULT_SHADER)
            {
                TextureReference = tileMap.DynamicTexture
            };
            obj.TextureReference.TextureName = TextureName.DynamicTexture;

            obj.Textures.Textures[0] = TextureName.DynamicTexture;

            Renderer.LoadTextureFromTextureObj(obj.TextureReference, TextureName.DynamicTexture);

            Animation Idle = new Animation()
            {
                Frames = new List<RenderableObject>() { obj },
                Frequency = -1,
                Repeats = -1
            };

            BaseObject baseObj = new BaseObject(new List<Animation>() { Idle }, 0, "", new Vector3());


            GameObject temp = new GameObject();
            temp.BaseObjects.Add(baseObj);

            temp.BaseObjects[0].BaseFrame.CameraPerspective = true;
            temp.BaseObjects[0].BaseFrame.ScaleAll(50);

            temp.SetPosition(WindowConstants.CenterScreen);

            _genericObjects.Add(temp);
        }

        public override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);

            float _cameraSpeed = 4.0f;

            if (!GetBit(_interceptKeystrokes, ObjectType.All) && _focusedObj == null)
            {

                if (MouseState.ScrollDelta[1] < 0)
                {
                    Vector3 movement = _camera.Front * _cameraSpeed / 2;
                    if (_camera.Position.Z - movement.Z < 26)
                    {
                        _camera.SetPosition(_camera.Position - movement); // Backwards
                        OnMouseMove();
                        OnCameraMoved();
                    }
                }
                else if (MouseState.ScrollDelta[1] > 0)
                {
                    Vector3 movement = _camera.Front * _cameraSpeed / 2;
                    if (_camera.Position.Z + movement.Z > 0)
                    {
                        _camera.SetPosition(_camera.Position + movement); // Forward
                        OnMouseMove();
                        OnCameraMoved();
                    }
                }

                if (KeyboardState.IsKeyDown(Keys.LeftShift))
                {
                    _cameraSpeed *= 20;
                }


                if (KeyboardState.IsKeyDown(Keys.W))
                {
                    _camera.SetPosition(_camera.Position + Vector3.UnitY * _cameraSpeed * (float)args.Time);
                    OnMouseMove();
                    OnCameraMoved();
                }

                if (KeyboardState.IsKeyDown(Keys.S))
                {
                    //_camera.Position -= _camera.Front * cameraSpeed * (float)args.Time; // Backwards
                    //_camera.Position -= _camera.Up * cameraSpeed * (float)args.Time; // Down
                    _camera.SetPosition(_camera.Position - Vector3.UnitY * _cameraSpeed * (float)args.Time);
                    OnMouseMove();
                    OnCameraMoved();
                }
                if (KeyboardState.IsKeyDown(Keys.A))
                {
                    //_camera.Position -= _camera.Right * _cameraSpeed * (float)args.Time; // Left
                    _camera.SetPosition(_camera.Position - _camera.Right * _cameraSpeed * (float)args.Time);
                    OnMouseMove();
                    OnCameraMoved();
                }
                if (KeyboardState.IsKeyDown(Keys.D))
                {
                    //_camera.Position += _camera.Right * _cameraSpeed * (float)args.Time; // Right
                    _camera.SetPosition(_camera.Position + _camera.Right * _cameraSpeed * (float)args.Time);
                    OnMouseMove();
                    OnCameraMoved();
                }
                if (KeyboardState.IsKeyDown(Keys.Space))
                {
                    //_camera.Position += _camera.Up * cameraSpeed * (float)args.Time; // Up
                }
            }
        }
    }
}
