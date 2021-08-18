using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.MiscOperations;
using MortalDungeon.Game.Abilities;
using MortalDungeon.Game.GameObjects;
using MortalDungeon.Game.Objects;
using MortalDungeon.Game.Units;
using MortalDungeon.Objects;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using MortalDungeon.Game.UI;
using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Game.Tiles;
using MortalDungeon.Engine_Classes.UIComponents;
using System.Linq;
using MortalDungeon.Game.Tiles.TileMaps;
using MortalDungeon.Game.Map;
using MortalDungeon.Game.Map.FeatureEquations;

namespace MortalDungeon.Game.SceneDefinitions
{
    class MenuScene : CombatScene
    {
        public MenuScene() : base()
        {
            InitializeFields();
        }

        

        public override void Load(Camera camera = null, BaseObject cursorObject = null, MouseRay mouseRay = null) 
        {
            base.Load(camera, cursorObject, mouseRay);


            TestTileMap tileMap = new TestTileMap(default, new TileMapPoint(0, 0), _tileMapController) { Width = 50, Height = 50 };
            tileMap.PopulateTileMap();

            _tileMapController.AddTileMap(new TileMapPoint(0, 0), tileMap);


            //TileMap tileMap2 = new TileMap(default, new TileMapPoint(0, 0), _tileMapController) { Width = 50, Height = 50 };

            //tileMap2.PopulateTileMap();

            //_tileMapController.AddTileMap(new TileMapPoint(-1, 0), tileMap2);


            //TileMap tileMap3 = new TileMap(default, new TileMapPoint(0, 0), _tileMapController) { Width = 50, Height = 50 };

            //tileMap3.PopulateTileMap();

            //_tileMapController.AddTileMap(new TileMapPoint(-2, 0), tileMap3);


            //TileMap tileMap4 = new TileMap(default, new TileMapPoint(0, 0), _tileMapController) { Width = 50, Height = 50 };

            //tileMap4.PopulateTileMap();

            //_tileMapController.AddTileMap(new TileMapPoint(0, -1), tileMap4);

            for (int x = 0; x < 2; x++) 
            {
                for (int y = 0; y < 2; y++) 
                {
                    if (!(x == 0 && y == 0)) 
                    {
                        TestTileMap tileMap2 = new TestTileMap(default, new TileMapPoint(x, y), _tileMapController) { Width = 50, Height = 50 };
                        tileMap2.PopulateTileMap();

                        _tileMapController.AddTileMap(new TileMapPoint(x, y), tileMap2);
                    }
                }
            }


            Guy guy = new Guy(tileMap[0, 0].Position + new Vector3(0, -tileMap.Tiles[0].GetDimensions().Y / 2, 0.2f), this, tileMap[0, 0]) { Clickable = true };
            guy.SetTeam(UnitTeam.Ally);
            guy.CurrentTileMap = tileMap;
            guy._movementAbility.EnergyCost = 0.3f;
            CurrentUnit = guy;

            guy.SelectionTile.UnitOffset.Y += tileMap.Tiles[0].GetDimensions().Y / 2;
            guy.SelectionTile.SetPosition(guy.Position);

            _units.Add(guy);

            Guy badGuy = new Guy(tileMap[0, 3].Position + new Vector3(0, -tileMap.Tiles[0].GetDimensions().Y / 2, 0.2f), this, tileMap[0, 3]) { Clickable = true };
            badGuy.SetTeam(UnitTeam.Enemy);
            badGuy.CurrentTileMap = tileMap;
            badGuy.SetColor(new Vector4(0.76f, 0.14f, 0.26f, 1));

            badGuy.SelectionTile.UnitOffset.Y += tileMap.Tiles[0].GetDimensions().Y / 2;
            badGuy.SelectionTile.SetPosition(badGuy.Position);

            _units.Add(badGuy);

            for (int i = 0; i < 1; i++)
            {
                Fire fire = new Fire(new Vector3(1150 + i * 250, 950, 0.2f));

                _genericObjects.Add(fire);
            }

            MountainTwo mountainBackground = new MountainTwo(new Vector3(30000, 0, -50));
            mountainBackground.BaseObjects[0].GetDisplay().ScaleAll(10);
            _genericObjects.Add(mountainBackground);
            mountainBackground.SetPosition(new Vector3(guy.Position.X, guy.Position.Y - 200, -50));
            _camera.SetPosition(new Vector3(guy.Position.X / WindowConstants.ScreenUnits.X * 2, guy.Position.Y / WindowConstants.ScreenUnits.Y * -2, _camera.Position.Z));



            float footerHeight = 300;

            Footer = new GameFooter(footerHeight, this); ;
            AddUI(Footer, 100);



            EnergyDisplayBar energyDisplayBar = new EnergyDisplayBar(this, new Vector3(30, WindowConstants.ScreenUnits.Y - Footer.GetDimensions().Y - 30, 0), new UIScale(1, 1), 10);

            EnergyDisplayBar = energyDisplayBar;

            AddUI(energyDisplayBar);




            UnitStatusBar guyStatusBar = new UnitStatusBar(guy, _camera);

            badGuy.Name = "Other guy";
            UnitStatusBar guyStatusBar2 = new UnitStatusBar(badGuy, _camera);


            badGuy.SetShields(5);


            Skeleton skeleton = new Skeleton(tileMap[1, 5].Position + new Vector3(0, -tileMap.Tiles[0].GetDimensions().Y / 2, 0.2f), this, tileMap[1, 5]) { };
            skeleton.SetTeam(UnitTeam.Neutral);
            UnitStatusBar skeletonStatusBar = new UnitStatusBar(skeleton, _camera);

            skeleton.SelectionTile.UnitOffset.Y += tileMap.Tiles[0].GetDimensions().Y / 2;
            skeleton.SelectionTile.SetPosition(skeleton.Position);

            _units.Add(skeleton);


            StartCombat();


            UIBlock statusBarContainer = new UIBlock(new Vector3());
            statusBarContainer.MultiTextureData.MixTexture = false;
            statusBarContainer.SetAllInline(0);
            statusBarContainer.SetColor(Colors.Transparent);

            statusBarContainer.AddChild(skeletonStatusBar);
            statusBarContainer.AddChild(guyStatusBar2);
            statusBarContainer.AddChild(guyStatusBar);

            AddUI(statusBarContainer);


            FillInTeamFog(InitiativeOrder[0].Team);


            //TextComponent description = new TextComponent();
            //description.SetTextScale(0.2f);
            //description.SetColor(Colors.UITextBlack);
            //description.SetText("description 1");

            //description.SetPositionFromAnchor(WindowConstants.CenterScreen, UIAnchorPosition.Center);

            //TextComponent desc2 = new TextComponent();
            //desc2.SetTextScale(0.2f);
            //desc2.SetColor(Colors.UITextBlack);
            //desc2.SetText("description 2");

            //desc2.SetPositionFromAnchor(description.GetAnchorPosition(UIAnchorPosition.BottomRight), UIAnchorPosition.TopCenter);


            //AddUI(description);
            //AddUI(desc2);
        }


        public override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);
        }
        public override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
        }

        public override bool OnKeyUp(KeyboardKeyEventArgs e)
        {
            if (!base.OnKeyUp(e)) 
            {
                return false;
            }
            
            Unit badGuy = _units.Find(g => g.Name == "Guy");
            //Console.WriteLine(badGuy.Position);
            if (_focusedObj == null) 
            {
                //if (e.Key == Keys.Equal)
                //{
                //    EnergyDisplayBar.AddEnergy(1);
                //}
                //if (e.Key == Keys.Minus)
                //{
                //    EnergyDisplayBar.AddEnergy(-1);
                //}
            }

            return true;
        }

        public override bool OnMouseMove()
        {
            return base.OnMouseMove();
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



        public override void OnTileClicked(TileMap map, BaseTile tile, MouseButton button, ContextManager<MouseUpFlags> flags)
        {
            base.OnTileClicked(map, tile, button, flags);
            if (button == MouseButton.Left)
            {
                if (_selectedAbility != null)
                {
                    _selectedAbility.OnTileClicked(map, tile);
                }
                else if (KeyboardState.IsKeyDown(Keys.LeftAlt))
                {
                    Unit tree = new Unit(this, Spritesheets.StructureSheet, rand.Next() % 2 + 2, tile.Position + new Vector3(0, -200, 0.22f));
                    tree.BaseObject.BaseFrame.RotateX(25);
                    tree.BaseObject.BaseFrame.SetScaleAll(1 + (float)rand.NextDouble() / 2);

                    //tree.NonCombatant = true;
                    tree.VisibleThroughFog = true;
                    tree.BlocksVision = true;
                    tree.TileMapPosition = tile;
                    tree.Name = "Tree";

                    tree.SelectionTile.UnitOffset = new Vector3(0, 200, -0.19f);

                    tree.Selectable = true;
                    tree.Clickable = true;
                    tree.SetTeam(UnitTeam.Neutral);

                    _units.Add(tree);

                    StartCombat();
                }
                else if (KeyboardState.IsKeyDown(Keys.RightAlt))
                {
                    _tileMapController.TileMaps.ForEach(m => m.PopulateFeatures());
                }
                else if (KeyboardState.IsKeyDown(Keys.RightShift))
                {
                    Unit unit = _units[0];
                    map.GetLineOfTiles(unit.TileMapPosition, tile.TilePoint).ForEach(t =>
                    {
                        t.SetAnimation(AnimationType.Idle);
                        t.SetColor(new Vector4(0.9f, 0.25f, 0.25f, 1));
                    });
                    //validTiles.Clear();
                    //map.GetRingOfTiles(tile.TileIndex, validTiles, 6);
                    //validTiles.ForEach(t =>
                    //{
                    //    t.SetColor(new Vector4(0.9f, 0.25f, 0.25f, 1));
                    //});
                }
                else if (KeyboardState.IsKeyDown(Keys.LeftShift))
                {
                    map.GenerateCliff(tile);
                }
                else if (KeyboardState.IsKeyDown(Keys.LeftControl))
                {
                    FeatureGenerator.GenerateRiver(tile.TilePoint, 5, 100);
                }
                else if (KeyboardState.IsKeyDown(Keys.F))
                {
                    List<BaseTile> tiles = new List<BaseTile>();

                    tiles = map.GetVisionInRadius(tile.TilePoint, 6);

                    tiles.ForEach(tile =>
                    {
                        tile.Properties.Height += 2;
                    });
                }
                else if (KeyboardState.IsKeyDown(Keys.V))
                {
                    List<BaseTile> tiles = new List<BaseTile>();

                    tiles = map.GetVisionInRadius(tile.TilePoint, 6);

                    tiles.ForEach(tile =>
                    {
                        tile.Properties.Height -= 2;
                    });
                }
                else if (KeyboardState.IsKeyDown(Keys.G))
                {
                    _tileMapController.TileMaps.ForEach(m => m.GenerateCliffs());
                }
                else if (KeyboardState.IsKeyDown(Keys.H))
                {
                    tile.Properties.Height--;
                }
                else if (KeyboardState.IsKeyDown(Keys.J))
                {
                    tile.Properties.Height++;
                }
                else if (KeyboardState.IsKeyDown(Keys.M))
                {
                    _tileMapController.ToggleHeightmap();
                }
                else if (KeyboardState.IsKeyDown(Keys.KeyPad1))
                {
                    TileMapPoint temp = new TileMapPoint(map.TileMapCoords.X, map.TileMapCoords.Y);
                    if (KeyboardState.IsKeyDown(Keys.KeyPadAdd))
                    {
                        temp.X++;
                        TestTileMap tileMap = new TestTileMap(default, temp, _tileMapController) { Width = 50, Height = 50 };
                        tileMap.PopulateTileMap();

                        _tileMapController.AddTileMap(temp, tileMap);
                    }
                    else if (KeyboardState.IsKeyDown(Keys.KeyPadSubtract))
                    {
                        temp.X--;
                        TestTileMap tileMap = new TestTileMap(default, temp, _tileMapController) { Width = 50, Height = 50 };
                        tileMap.PopulateTileMap();

                        _tileMapController.AddTileMap(temp, tileMap);
                    }
                }
                else if (KeyboardState.IsKeyDown(Keys.KeyPad2))
                {
                    Console.WriteLine(_camera.Position);
                    Console.WriteLine(_units[0].Position);
                }
                else if (KeyboardState.IsKeyDown(Keys.KeyPad3))
                {
                    _tileMapController.RemoveTileMap(map);
                }
                else if (KeyboardState.IsKeyDown(Keys.KeyPadDivide))
                {
                    //Vector2i point = FeatureEquation.PointToMapCoords(tile.TilePoint);
                    //River_1 river = new River_1(point.Y, 5, 1f, point.X);
                    _tileMapController.TileMaps.ForEach(m =>
                    {
                        m.Tiles.ForEach(t =>
                        {
                            t.Properties.Type = TileType.Grass;
                            t.Outline = true;
                            t.NeverOutline = false;
                            t.Update();
                        });
                    });


                    RiverParams riverParams = new RiverParams(new TileMapPoint(0, 0), new Vector2i(0, rand.Next(45)), new Vector2i(49, rand.Next(45)), 5);
                    riverParams.AddStop(new Vector2i(rand.Next(45), rand.Next(45)));

                    RiverParams riverParams2 = new RiverParams(new TileMapPoint(1, 0), new Vector2i(0, riverParams.Stops[^1].Y), new Vector2i(49, rand.Next(45)), 5);
                    riverParams2.AddStop(new Vector2i(rand.Next(45), rand.Next(45)));

                    River_1 river = new River_1(new List<RiverParams>() { riverParams, riverParams2 });

                    _tileMapController.ApplyFeatureEquationToMaps(river);
                }
                else if (KeyboardState.IsKeyDown(Keys.GraveAccent))
                {
                    if (_wallTemp == null)
                    {
                        _wallTemp = tile.TilePoint;
                    }
                    else 
                    {
                        TestTileMap temp = map as TestTileMap;

                        temp.CreateWalls(_wallTemp, tile.TilePoint);
                        _wallTemp = null;
                    }
                }
            }
            else
            {
                tile.OnRightClick(flags);
            }
        }

        private TilePoint _wallTemp = null;
    }
}
