using MortalDungeon.Engine_Classes;
using MortalDungeon.Game.GameObjects;
using MortalDungeon.Game.Objects;
using MortalDungeon.Game.Units;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;

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


            TileMap tileMap = new TileMap(ClientSize, default) { Width = 50, Height = 50};

            tileMap.PopulateTileMap();
            _tileMaps.Add(tileMap);

            Guy guy = new Guy(ClientSize, tileMap.GetPositionOfTile(0) + Vector3.UnitZ * 0.2f, 5);

            _units.Add(guy);

            for (int i = 0; i < 1; i++)
            {
                Fire fire = new Fire(ClientSize, new Vector3(1150 + i * 250, 950, 0.2f));

                _renderedObjects.Add(fire);
            }

            MountainTwo mountainBackground = new MountainTwo(ClientSize, new Vector3(30000, 0, -50));
            //mountainBackground.BaseObjects[0].Display.RotateX(-15);
            mountainBackground.BaseObjects[0].Display.ScaleAll(10);
            _renderedObjects.Add(mountainBackground);

            Text textTest = new Text(ClientSize, "Test string", new Vector3(25, -2300, 0.1f), true);

            //Text textTest2 = new Text(ClientSize, "THE QUICK BROWN FOX JUMPS \nOVER THE LAZY DOG", new Vector3(100, 400, 0));
            textTest.SetScale(2);


            _text.Add(textTest);
            //_text.Add(textTest2);
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

        private int tilePosition = 0;
        public override void onKeyUp(KeyboardKeyEventArgs e)
        {
            Unit badGuy = _units.Find(g => g.Name == "Guy");
            Console.WriteLine(badGuy.Position);
            if (e.Key == Keys.Right)
            {
                tilePosition += _tileMaps[0].Height;
                Vector3 position = _tileMaps[0].GetPositionOfTile(tilePosition);
                badGuy.GradualMove(new Vector3(position.X, position.Y + badGuy.PositionalOffset.Y, badGuy.Position.Z), 1, 5);
                badGuy.BaseObjects[0].SetAnimation(AnimationType.Die, () => badGuy.BaseObjects[0].SetAnimation(AnimationType.Idle));
            }
            if (e.Key == Keys.Left)
            {
                tilePosition -= _tileMaps[0].Height;
                Vector3 position = _tileMaps[0].GetPositionOfTile(tilePosition);
                badGuy.GradualMove(new Vector3(position.X, position.Y + badGuy.PositionalOffset.Y, badGuy.Position.Z), 1, 5);
                badGuy.BaseObjects[0].SetAnimation(AnimationType.Die, () => badGuy.BaseObjects[0].SetAnimation(AnimationType.Idle));
            }
            if (e.Key == Keys.Up)
            {
                tilePosition -= 1;
                Vector3 position = _tileMaps[0].GetPositionOfTile(tilePosition);
                badGuy.GradualMove(new Vector3(position.X, position.Y + badGuy.PositionalOffset.Y, badGuy.Position.Z), 1, 5);
                badGuy.BaseObjects[0].SetAnimation(AnimationType.Die, () => badGuy.BaseObjects[0].SetAnimation(AnimationType.Idle));
            }
            if (e.Key == Keys.Down)
            {
                tilePosition += 1;
                Vector3 position = _tileMaps[0].GetPositionOfTile(tilePosition);
                badGuy.GradualMove(new Vector3(position.X, position.Y + badGuy.PositionalOffset.Y, badGuy.Position.Z), 1, 5);
                badGuy.BaseObjects[0].SetAnimation(AnimationType.Die, () => badGuy.BaseObjects[0].SetAnimation(AnimationType.Idle));
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
        public override bool onMouseMove(MouseMoveEventArgs e)
        {
            if(base.onMouseMove(e))
            {
                Vector4 MouseCoordinates = new Vector4(NormalizeGlobalCoordinates(new Vector2(_cursorObject.Position.X, _cursorObject.Position.Y), ClientSize));

                List<GameObject> foundObjs = _cursorBoundsCheck(_renderedObjects);

                if (foundObjs.Count > 0)
                {
                    Vector4 color = new Vector4(0, 0, 100f, 1);

                    hoveredObject = GetObjWithHighestZ(foundObjs); //we only care about the topmost object


                    if (hoveredObject.Name == "Guy")
                    {
                        hoveredObject.BaseObjects[0].SetAnimation(AnimationType.Die);
                    }
                }
            }

            return true;
        }
    }


}
