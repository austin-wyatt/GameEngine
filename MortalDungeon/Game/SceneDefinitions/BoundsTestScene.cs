using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.MiscOperations;
using MortalDungeon.Engine_Classes.Rendering;
using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Engine_Classes.UIComponents;
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
            //temp.AddBaseObject(baseObj);

            //temp.BaseObjects[0].BaseFrame.CameraPerspective = false;

            //temp.SetPosition(WindowConstants.CenterScreen);

            //_genericObjects.Add(temp);

            //
            //TileMap tileMap = new TileMap(default, new TileMapPoint(0, 0), _tileMapController) { Width = 50, Height = 50 };
            //tileMap.PopulateTileMap();

            ////_tileMapController.AddTileMap(new TileMapPoint(0, 0), tileMap);

            //TileTexturer.InitializeTexture(tileMap);

            //RenderableObject obj = new RenderableObject(new SpritesheetObject(0, Spritesheets.TestSheet, 10, 10).CreateObjectDefinition(true), WindowConstants.FullColor, ObjectRenderType.Texture, Shaders.DEFAULT_SHADER)
            //{
            //    TextureReference = tileMap.DynamicTexture
            //};
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
            //temp.AddBaseObject(baseObj);

            //temp.BaseObjects[0].BaseFrame.CameraPerspective = true;
            //temp.BaseObjects[0].BaseFrame.ScaleAll(50);

            //temp.SetPosition(WindowConstants.CenterScreen);

            //_genericObjects.Add(temp);

            //GameObject temp = new GameObject();
            //temp.AddBaseObject(_3DObjects.CreateBaseObject(new SpritesheetObject(0, Textures.TentTexture), _3DObjects.Tent, default));

            //temp.SetPosition(new Vector3(0, 0, 0));
            //_genericObjects.Add(temp);

            //GameObject temp1 = new GameObject();
            //temp1.AddBaseObject(_3DObjects.CreateBaseObject(new SpritesheetObject(3, Spritesheets.CubeTexture), _3DObjects.Cube, default));

            //temp1.SetPosition(new Vector3(2000, 2000, 2));
            //_genericObjects.Add(temp1);


            //GameObject temp2 = new GameObject();
            //temp2.AddBaseObject(_3DObjects.CreateBaseObject(new SpritesheetObject(3, Spritesheets.CubeTexture), _3DObjects.Cube, default));

            //temp2.SetPosition(new Vector3(0 * 1000, -0 * 1000, 0));
            //temp2.BaseObject.BaseFrame.SetScaleAll(0.2f);
            //_genericObjects.Add(temp2);


            //GameObject temp3 = new GameObject();
            //temp3.AddBaseObject(_3DObjects.CreateBaseObject(new SpritesheetObject(0, Spritesheets.CubeTexture), _3DObjects.Cube, default));


            //temp3.SetPosition(new Vector3(-20000, 0, 0));
            //_genericObjects.Add(temp3);


            //SPECULAR_TEST = Texture.LoadFromFile("Resources/cube specular map.png");

            ////temp.BaseObject.BaseFrame.RotateZ(45);


            //PropertyAnimation anim2 = new PropertyAnimation();

            //Keyframe frame2 = new Keyframe(1);
            //frame2.Action = () =>
            //{
            //    temp1.BaseObject.BaseFrame.RotateY(5);
            //    temp1.BaseObject.BaseFrame.RotateX(5);
            //};

            //anim2.Keyframes.Add(frame2);
            //anim2.Playing = true;
            //anim2.Repeat = true;


            //GameObject.LoadTexture(temp3);
            //GameObject.LoadTexture(temp2);
            //GameObject.LoadTexture(temp1);
            //GameObject.LoadTexture(temp);

            //TickableObjects.Add(anim2);

            UIBlock block1 = new UIBlock(default, new UIScale(1, 1));
            block1.SetPosition(WindowConstants.CenterScreen);
            block1.Draggable = true;
            block1.Clickable = true;
            block1.Hoverable = true;

            UIBlock block2 = new UIBlock(default, new UIScale(1, 1));
            block2.SetPosition(WindowConstants.CenterScreen);
            block2.Draggable = true;
            block2.Clickable = true;
            block2.Hoverable = true;
            block2.SetColor(Colors.Red);

            block1.LoadTexture();
            block2.LoadTexture();

            block1.GenerateReverseTree(UIManager);
            block2.GenerateReverseTree(UIManager);


            UIBlock blockt = new UIBlock(default, new UIScale(0.5f, 0.5f));
            blockt.SetPositionFromAnchor(block2.GetAnchorPosition(UIAnchorPosition.TopLeft), UIAnchorPosition.TopLeft);
            blockt.SetColor(Colors.Blue);
            blockt.LoadTexture();

            block2.AddChild(blockt);


            AddUI(block1, 10);
            AddUI(block2, 50);
        }

        public static Texture SPECULAR_TEST = null;

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
