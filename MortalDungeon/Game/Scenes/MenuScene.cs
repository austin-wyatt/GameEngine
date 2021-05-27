using MortalDungeon.Engine_Classes;
using MortalDungeon.Game.Objects;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Game.Scenes
{
    class MenuScene : Scene
    {
        public MenuScene() 
        {
            InitializeFields();
        }

        public override void Load(Vector2i clientSize, Camera camera = null, BaseObject cursorObject = null) 
        {
            base.Load(clientSize, camera, cursorObject);

            Vector3 hexagonTilePosition = WindowConstants.CenterScreen;

            //for (int p = 0; p < 10; p++)
            //{
            //    for (int i = 0; i < 100; i++)
            //    {
            //        BaseObject hexagonTileObject = new BaseObject(ClientSize, HEXAGON_ANIMATION.List, p * 20 + i, "Hexagon " + (p * 20 + i), hexagonTilePosition, EnvironmentObjects.HEXAGON_TILE_SQUARE_Generic.Bounds);
            //        hexagonTileObject.BaseFrame.CameraPerspective = true;
            //        hexagonTileObject.BaseFrame.ScaleAll(0.5f);
            //        hexagonTileObject.BaseFrame.ColorProportion = 0f;

            //        _clickableObjects.Add(hexagonTileObject);
            //        _renderedObjects.Add(hexagonTileObject);

            //        hexagonTilePosition.X += 150;
            //        hexagonTilePosition.Y += 150 * (i % 2 == 0 ? 1 : -1);

            //        hexagonTileObject.OnClick = (obj) =>
            //        {
            //            obj.CurrentAnimation.Reset();
            //            obj.CurrentAnimation.Repeats = -1;
            //            obj.BaseFrame.Color = new Vector4(obj.Display.Color.X + 0.1f, obj.Display.Color.Y, obj.Display.Color.Z, obj.Display.Color.W);
            //            hexagonTileObject.BaseFrame.ColorProportion = 0.5f;
            //        };
            //    }
            //    hexagonTilePosition = WindowConstants.CenterScreen;
            //    hexagonTilePosition.Y -= p * 300;
            //}

            Vector3 GrassTilePosition = new Vector3();
            for (int i = 0; i < 10; i++) 
            {
                for(int o = 0; o <= 10; o++)
                {
                    BaseObject GrassTile = new BaseObject(ClientSize, GRASS_ANIMATION.List, 0, "", GrassTilePosition, EnvironmentObjects.GRASS_TILE.Bounds);
                    GrassTile.BaseFrame.CameraPerspective = true;

                    _renderedObjects.Add(GrassTile);

                    GrassTilePosition.X = i * 500;
                    GrassTilePosition.Y = o * 500;
                }
            }


            ParticleGenTest particleGen = new ParticleGenTest(new Vector3(1000, -1000, 0));
            particleGen.Playing = false;

            _particleGenerators.Add(particleGen);
        }

        public override void onMouseUp(MouseButtonEventArgs e)
        {

            if (e.Button == MouseButton.Left && e.Action == InputAction.Release)
            {
                Vector4 MouseCoordinates = new Vector4(NormalizeGlobalCoordinates(new Vector2(_cursorObject.Position.X, _cursorObject.Position.Y), ClientSize));

                _clickableObjects.ForEach(o =>
                {
                    if (!o.BaseFrame.CameraPerspective)
                    {
                        if (o.Bounds.Contains(new Vector2(MouseCoordinates.X, MouseCoordinates.Y), _camera))
                        {
                            Console.WriteLine("Object " + o.Name + " clicked.");

                            o.Display.Color = new Vector4(1, 0, 0, 0);
                            o.Display.ColorProportion = 0.5f;

                            if (o.OnClick != null)
                                o.OnClick(o);
                        }
                    }
                });

                _cursorBoundsCheck(_clickableObjects).ForEach((foundObj) =>
                {
                    Vector4 color = foundObj.BaseFrame.Color;
                    color.Z += 0.1f;
                    foundObj.BaseFrame.Color = color;
                    foundObj.BaseFrame.ColorProportion = 0.5f;

                    foundObj.CurrentAnimation.Reset();
                    foundObj.CurrentAnimation.Reverse = true;
                });
            }
            base.onMouseUp(e);
        }


        private BaseObject hoveredObject = null;
        public override void onMouseMove(MouseMoveEventArgs e)
        {

            Vector4 MouseCoordinates = new Vector4(NormalizeGlobalCoordinates(new Vector2(_cursorObject.Position.X, _cursorObject.Position.Y), ClientSize));



            List<BaseObject> foundObjs = _cursorBoundsCheck(_renderedObjects);

            if (foundObjs.Count > 0)
            {
                Vector4 color = new Vector4(0, 0, 100f, 1);

                hoveredObject = foundObjs[0];

                if(hoveredObject.ID != 0)
                {
                    hoveredObject.BaseFrame.Color = color;
                    hoveredObject.BaseFrame.ColorProportion = 1.0f;
                    hoveredObject.CurrentAnimation.Reset();
                }
            }


            base.onMouseMove(e);
        }
    }


}
