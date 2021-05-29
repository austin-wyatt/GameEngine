using MortalDungeon.Engine_Classes;
using MortalDungeon.Game.GameObjects;
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

            //Vector3 GrassTilePosition = new Vector3();
            //for (int i = 0; i < 10; i++) 
            //{
            //    for(int o = 0; o <= 10; o++)
            //    {
            //        BaseObject GrassTile = new BaseObject(ClientSize, GRASS_ANIMATION.List, 0, "", GrassTilePosition, EnvironmentObjects.GRASS_TILE.Bounds);
            //        GrassTile.BaseFrame.CameraPerspective = true;

            //        _renderedObjects.Add(GrassTile);

            //        GrassTilePosition.X = i * 500;
            //        GrassTilePosition.Y = o * 500;
            //    }
            //}

            Vector3 TilePosition = new Vector3();
            BaseTile baseTile = new BaseTile();

            for (int i = 0; i < 45; i++)
            {
                for (int o = 0; o < 45; o++)
                {
                    baseTile = new BaseTile(ClientSize, TilePosition, i * 45 + o + 1);

                    _renderedObjects.Add(baseTile);
                    _clickableObjects.Add(baseTile);

                    TilePosition.Y += baseTile.BaseObjects[0].Dimensions.Y;
                }
                TilePosition.X = (i + 1) * baseTile.BaseObjects[0].Dimensions.X / 1.29f;
                TilePosition.Y = ((i + 1) % 2 == 0 ? 0 : baseTile.BaseObjects[0].Dimensions.Y / -2);
                TilePosition.Z += 0.0001f;
            }

            Guy guy = new Guy(ClientSize, new Vector3(baseTile.BaseObjects[0].Dimensions.X * 0, 0, 0.2f), 0);

            _renderedObjects.Add(guy);
            _clickableObjects.Add(guy);

            for (int i = 0; i < 20; i++)
            {
                Fire fire = new Fire(ClientSize, new Vector3(1150 + i * 250, 950, 0.2f));

                _renderedObjects.Add(fire);
            }
        }

        public override void onMouseUp(MouseButtonEventArgs e)
        {

            if (e.Button == MouseButton.Left && e.Action == InputAction.Release)
            {
                Vector4 MouseCoordinates = new Vector4(NormalizeGlobalCoordinates(new Vector2(_cursorObject.Position.X, _cursorObject.Position.Y), ClientSize));

                _clickableObjects.ForEach(o =>
                {
                    o.BaseObjects.ForEach(b =>
                    {
                        if (!b.BaseFrame.CameraPerspective)
                        {
                            if (b.Bounds.Contains(new Vector2(MouseCoordinates.X, MouseCoordinates.Y), _camera))
                            {
                                Console.WriteLine("Object " + b.Name + " clicked.");

                                b.Display.Color = new Vector4(1, 0, 0, 0);
                                b.Display.ColorProportion = 0.5f;

                                if (b.OnClick != null)
                                    b.OnClick(b);
                            }
                        }
                    });
                });

                _cursorBoundsCheck(_clickableObjects).ForEach((foundObj) =>
                {
                    foundObj.BaseObjects.ForEach(obj =>
                    {
                        Vector4 color = obj.BaseFrame.Color;
                        color.Z += 0.1f;
                        obj.BaseFrame.Color = color;
                        obj.BaseFrame.ColorProportion = 0.5f;

                        //foundObj.CurrentAnimation.Reset();
                        //foundObj.CurrentAnimation.Reverse = true;

                        if (obj.Name == "BadGuy")
                        {
                            obj.CurrentAnimation.Reset();
                            obj.SetAnimation(AnimationType.Idle);
                        }
                    });
                });
            }
            base.onMouseUp(e);
        }

        private int tilePosition = 1;
        public override void onKeyUp(KeyboardKeyEventArgs e)
        {
            GameObject badGuy = _renderedObjects.Find(g => g.Name == "Guy");
            GameObject baseTileRef = _renderedObjects.Find(g => g.BaseObjects[0].ID == tilePosition);
            Console.WriteLine(badGuy.Position);
            if (e.Key == Keys.Right)
            {
                //badGuy.MoveObject(new Vector3((baseTileRef.Dimensions.X - 1) * 2 , baseTileRef.Dimensions.Y * (rand.Next(0, 2) == 0 ? -1 : 1), 0));
                tilePosition += 45;
                baseTileRef = _renderedObjects.Find(g => g.BaseObjects[0].ID == tilePosition);
                badGuy.SetPosition(new Vector3(baseTileRef.Position.X, baseTileRef.Position.Y + badGuy.PositionalOffset.Y, badGuy.Position.Z));
            }
            if (e.Key == Keys.Left)
            {
                tilePosition -= 45;
                baseTileRef = _renderedObjects.Find(g => g.BaseObjects[0].ID == tilePosition);
                badGuy.SetPosition(new Vector3(baseTileRef.Position.X, baseTileRef.Position.Y + badGuy.PositionalOffset.Y, badGuy.Position.Z));
            }
            if (e.Key == Keys.Up)
            {
                tilePosition -= 1;
                baseTileRef = _renderedObjects.Find(g => g.BaseObjects[0].ID == tilePosition);

                badGuy.SetPosition(new Vector3(baseTileRef.Position.X, baseTileRef.Position.Y + badGuy.PositionalOffset.Y, badGuy.Position.Z));
            }
            if (e.Key == Keys.Down)
            {
                tilePosition += 1;
                baseTileRef = _renderedObjects.Find(g => g.BaseObjects[0].ID == tilePosition);
                badGuy.SetPosition(new Vector3(baseTileRef.Position.X, baseTileRef.Position.Y + badGuy.PositionalOffset.Y, badGuy.Position.Z));
            }

            if (e.Key == Keys.Equal)
            {
                badGuy.BaseObjects[0].CurrentAnimation.Frequency++;
                badGuy.BaseObjects[0].BaseFrame.ScaleAddition(0.1f);
            }
            if (e.Key == Keys.Minus)
            {
                badGuy.BaseObjects[0].CurrentAnimation.Frequency--;
                badGuy.BaseObjects[0].BaseFrame.ScaleAddition(-0.1f);
            }
            base.onKeyUp(e);
        }

        private GameObject hoveredObject = null;
        public override void onMouseMove(MouseMoveEventArgs e)
        {

            Vector4 MouseCoordinates = new Vector4(NormalizeGlobalCoordinates(new Vector2(_cursorObject.Position.X, _cursorObject.Position.Y), ClientSize));

            List<GameObject> foundObjs = _cursorBoundsCheck(_renderedObjects);

            if (foundObjs.Count > 0)
            {
                Vector4 color = new Vector4(0, 0, 100f, 1);

                hoveredObject = GetObjWithHighestZ(foundObjs); //we only care about the topmost object


                if(hoveredObject.Name == "Guy")
                {
                    hoveredObject.BaseObjects[0].SetAnimation(AnimationType.Die);
                }
            }


            base.onMouseMove(e);
        }
    }


}
