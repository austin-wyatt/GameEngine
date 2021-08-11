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


            TileMap tileMap = new TileMap(default, new TileMapPoint(0,0), _tileMapController) { Width = 50, Height = 50 };
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


            InitiativeOrder = new List<Unit>(_units);

            StartRound();


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


        private List<BaseTile> selectedTiles = new List<BaseTile>();

        public override void onMouseUp(MouseButtonEventArgs e)
        {
            base.onMouseUp(e);
        }
        public override void onMouseDown(MouseButtonEventArgs e)
        {
            base.onMouseDown(e);
        }

        public override bool onKeyUp(KeyboardKeyEventArgs e)
        {
            if (!base.onKeyUp(e)) 
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

        public override bool onMouseMove()
        {
            return base.onMouseMove();
        }

        public override void onUpdateFrame(FrameEventArgs args)
        {
            base.onUpdateFrame(args);

            float _cameraSpeed = 4.0f;

            if (!GetBit(_interceptKeystrokes, ObjectType.All) && _focusedObj == null)
            {

                if (MouseState.ScrollDelta[1] < 0)
                {
                    Vector3 movement = _camera.Front * _cameraSpeed / 2;
                    if (_camera.Position.Z - movement.Z < 26)
                    {
                        _camera.SetPosition(_camera.Position - movement); // Backwards
                        onMouseMove();
                        onCameraMoved();
                    }
                }
                else if (MouseState.ScrollDelta[1] > 0)
                {
                    Vector3 movement = _camera.Front * _cameraSpeed / 2;
                    if (_camera.Position.Z + movement.Z > 0)
                    {
                        _camera.SetPosition(_camera.Position + movement); // Forward
                        onMouseMove();
                        onCameraMoved();
                    }
                }

                if (KeyboardState.IsKeyDown(Keys.LeftShift))
                {
                    _cameraSpeed *= 20;
                }


                if (KeyboardState.IsKeyDown(Keys.W))
                {
                    _camera.SetPosition(_camera.Position + Vector3.UnitY * _cameraSpeed * (float)args.Time);
                    onMouseMove();
                    onCameraMoved();
                }

                if (KeyboardState.IsKeyDown(Keys.S))
                {
                    //_camera.Position -= _camera.Front * cameraSpeed * (float)args.Time; // Backwards
                    //_camera.Position -= _camera.Up * cameraSpeed * (float)args.Time; // Down
                    _camera.SetPosition(_camera.Position - Vector3.UnitY * _cameraSpeed * (float)args.Time);
                    onMouseMove();
                    onCameraMoved();
                }
                if (KeyboardState.IsKeyDown(Keys.A))
                {
                    //_camera.Position -= _camera.Right * _cameraSpeed * (float)args.Time; // Left
                    _camera.SetPosition(_camera.Position - _camera.Right * _cameraSpeed * (float)args.Time);
                    onMouseMove();
                    onCameraMoved();
                }
                if (KeyboardState.IsKeyDown(Keys.D))
                {
                    //_camera.Position += _camera.Right * _cameraSpeed * (float)args.Time; // Right
                    _camera.SetPosition(_camera.Position + _camera.Right * _cameraSpeed * (float)args.Time);
                    onMouseMove();
                    onCameraMoved();
                }
                if (KeyboardState.IsKeyDown(Keys.Space))
                {
                    //_camera.Position += _camera.Up * cameraSpeed * (float)args.Time; // Up
                }
            }
        }

        

        public override void onTileClicked(TileMap map, BaseTile tile) 
        {
            base.onTileClicked(map, tile);

            if (_selectedAbility != null)
            {
                _selectedAbility.OnTileClicked(map, tile);
            }
            else if (KeyboardState.IsKeyDown(Keys.LeftControl))
            {

                tile.TileClassification = TileClassification.AttackableTerrain;
                tile.BlocksVision = true;
                tile.SetAnimation(AnimationType.Idle);
                tile.DefaultAnimation = (BaseTileAnimationType)AnimationType.Idle;
                tile.DefaultColor = tile.SetFogColor();

                //Console.WriteLine(map.ConvertIndexToCoord(tile.TileIndex));
            }
            else if (KeyboardState.IsKeyDown(Keys.LeftAlt))
            {
                //map.SetDefaultTileValues();
                map.Tiles.ForEach(t =>
                {
                    if (!t.BlocksVision)
                    {
                        t.TileClassification = TileClassification.Ground;
                        t.SetColor(t.DefaultColor);
                        t.SetAnimation(t.DefaultAnimation);
                    }
                    if (t.BlocksVision)
                    {
                        t.SetFogColor();
                        t.SetAnimation(BaseTileAnimationType.SolidWhite);
                    }
                });
            }
            else if (KeyboardState.IsKeyDown(Keys.RightAlt))
            {
                //validTiles = map.FindValidTilesInRadius(tile.TileIndex, 6, new List<TileClassification> { TileClassification.Ground });
                //Unit unit = _units[0];
                selectedTiles = map.GetVisionInRadius(tile.TilePoint, 6);
                selectedTiles.ForEach(t =>
                {
                    if (!t.BlocksVision)
                    {
                        t.SetColor(new Vector4(0.1f, 0.25f, 0.25f, 1));
                        t.SetAnimation(AnimationType.Idle);
                    }
                });
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
                selectedTiles.ForEach(t =>
                {
                    if (t.CurrentAnimation != BaseTileAnimationType.SolidWhite)
                    {
                        t.SetColor(t.DefaultColor);
                        t.SetAnimation(t.DefaultAnimation);
                    }

                });
                selectedTiles = map.GetPathToPoint(_units[0].TileMapPosition, tile.TilePoint, 100, new List<TileClassification>() { TileClassification.Ground }, _units);

                if (selectedTiles.Count == 0)
                {
                    selectedTiles.Add(tile);
                    tile.SetColor(new Vector4(1f, 0f, 0f, 1));
                }
                else
                {
                    selectedTiles.ForEach(t =>
                    {
                        t.SetColor(new Vector4(0.75f, 0.5f, 0.5f, 1));
                        t.SetAnimation(t.DefaultAnimation);
                    });
                }
            }
            else if (KeyboardState.IsKeyDown(Keys.F)) 
            {
                List<BaseTile> tiles = new List<BaseTile>();
                _units.ForEach(unit =>
                {
                    tiles = map.GetVisionInRadius(unit.TileMapPosition, unit.VisionRadius, new List<TileClassification>() { TileClassification.Terrain }, _units.FindAll(u => u.TileMapPosition != unit.TileMapPosition));

                    tiles.ForEach(tile =>
                    {
                        tile.SetFog(false);
                        tile.SetExplored();
                    });
                });
            }
            else if (KeyboardState.IsKeyDown(Keys.G))
            {
                map.Tiles.ForEach(tile =>
                {
                    tile.SetFog(true);
                });
            }

        }
    }


}
