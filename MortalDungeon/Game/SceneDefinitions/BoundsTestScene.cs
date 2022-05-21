using Empyrean.Engine_Classes;
using Empyrean.Engine_Classes.MiscOperations;
using Empyrean.Engine_Classes.Rendering;
using Empyrean.Engine_Classes.Scenes;
using Empyrean.Engine_Classes.UIComponents;
using Empyrean.Game.Events;
using Empyrean.Game.Objects;
using Empyrean.Game.Player;
using Empyrean.Game.Tiles;
using Empyrean.Game.Tiles.TileMaps;
using Empyrean.Game.Units;
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
    class BoundsTestScene : CombatScene
    {
        public BoundsTestScene()
        {
            InitializeFields();
        }

        //public static float[] imageData = new float[512 * 512 * 4];
        //public static Texture tex;

        public override void Load(Camera camera = null, MouseRay mouseRay = null)
        {
            base.Load(camera, mouseRay);

            TileMapManager.Scene = this;
            PlayerParty.Scene = this;
            EventManager.Scene = this;
            VisionManager.Scene = this;

            TestTileMap newMap = new TestTileMap(default, new TileMapPoint(0, 0), _tileMapController) 
            { 
                Width = TileMapManager.TILE_MAP_DIMENSIONS.X, 
                Height = TileMapManager.TILE_MAP_DIMENSIONS.Y 
            };

            newMap.PopulateTileMap();

            newMap.TileMapCoords = new TileMapPoint(0, 0);

            newMap.OnAddedToController();

            TileMapManager.LoadedMaps.Add(newMap.TileMapCoords, newMap);
            TileMapManager.ActiveMaps.Add(newMap);
            TileMapManager.SetVisibleMaps(new List<TileMap> { newMap });

            newMap.Visible = true;
            VisionManager.SetRevealAll(true);

            foreach(var chunk in newMap.TileChunks)
            {
                //chunk.MeshChunk.tempRaiseTile();
                chunk.Update(TileUpdateType.Vertex);
            }

            #region random objects
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
            #endregion

            RenderingConstants.LightColor = new Vector4(1, 1, 1, 1);
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
                        //OnMouseMove();
                        //OnCameraMoved();
                    }
                }
                else if (MouseState.ScrollDelta[1] > 0)
                {
                    Vector3 movement = _camera.Front * _cameraSpeed / 2;
                    if (_camera.Position.Z + movement.Z > 0)
                    {
                        _camera.SetPosition(_camera.Position + movement); // Forward
                        //OnMouseMove();
                        //OnCameraMoved();
                    }
                }

                if (KeyboardState.IsKeyDown(Keys.LeftShift))
                {
                    _cameraSpeed *= 20;
                }


                if (KeyboardState.IsKeyDown(Keys.W))
                {
                    _camera.SetPosition(_camera.Position + Vector3.UnitY * _cameraSpeed * (float)args.Time);
                    //OnMouseMove();
                    //OnCameraMoved();
                }

                if (KeyboardState.IsKeyDown(Keys.S))
                {
                    //_camera.Position -= _camera.Front * cameraSpeed * (float)args.Time; // Backwards
                    //_camera.Position -= _camera.Up * cameraSpeed * (float)args.Time; // Down
                    _camera.SetPosition(_camera.Position - Vector3.UnitY * _cameraSpeed * (float)args.Time);
                    //OnMouseMove();
                    //OnCameraMoved();
                }
                if (KeyboardState.IsKeyDown(Keys.A))
                {
                    //_camera.Position -= _camera.Right * _cameraSpeed * (float)args.Time; // Left
                    _camera.SetPosition(_camera.Position - _camera.Right * _cameraSpeed * (float)args.Time);
                    //OnMouseMove();
                    //OnCameraMoved();
                }
                if (KeyboardState.IsKeyDown(Keys.D))
                {
                    //_camera.Position += _camera.Right * _cameraSpeed * (float)args.Time; // Right
                    _camera.SetPosition(_camera.Position + _camera.Right * _cameraSpeed * (float)args.Time);
                    //OnMouseMove();
                    //OnCameraMoved();
                }
                if (KeyboardState.IsKeyDown(Keys.Space))
                {
                    //_camera.Position += _camera.Up * cameraSpeed * (float)args.Time; // Up
                }
            }
        }

        public override bool OnKeyDown(KeyboardKeyEventArgs e)
        {
            switch (e.Key)
            {
                
            }

            return true;
        }
    }
}
