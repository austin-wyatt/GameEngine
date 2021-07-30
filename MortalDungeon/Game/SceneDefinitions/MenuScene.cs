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

        Footer _footer;

        public override void Load(Camera camera = null, BaseObject cursorObject = null, MouseRay mouseRay = null) 
        {
            base.Load(camera, cursorObject, mouseRay);


            TileMap tileMap = new TileMap(default) { Width = 50, Height = 50 };
            //TileMap tileMap = new TileMap(default) { Width = 5, Height = 5 };

            tileMap.PopulateTileMap();
            _tileMaps.Add(tileMap);


            Guy guy = new Guy(tileMap.GetPositionOfTile(0) + Vector3.UnitZ * 0.2f, 0) { Clickable = true };
            guy.Team = UnitTeam.Ally;
            guy.CurrentTileMap = tileMap;
            guy._movementAbility.EnergyCost = 2f;
            CurrentUnit = guy;

            _units.Add(guy);

            Guy badGuy = new Guy(tileMap.GetPositionOfTile(3) + Vector3.UnitZ * 0.2f, 0) { Clickable = true };
            badGuy.TileMapPosition = 3;
            badGuy.Team = UnitTeam.Enemy;
            badGuy.CurrentTileMap = tileMap;
            badGuy.SetColor(new Vector4(0.76f, 0.14f, 0.26f, 1));

            _units.Add(badGuy);

            for (int i = 0; i < 1; i++)
            {
                Fire fire = new Fire(new Vector3(1150 + i * 250, 950, 0.2f));

                _genericObjects.Add(fire);
            }

            MountainTwo mountainBackground = new MountainTwo(new Vector3(30000, 0, -50));
            //mountainBackground.BaseObjects[0].Display.RotateX(-15);
            mountainBackground.BaseObjects[0].GetDisplay().ScaleAll(10);
            _genericObjects.Add(mountainBackground);

            //Text textTest = new Text("Test string\nwith line break", new Vector3(25, -2300, 0.1f), true);
            //textTest.SetScale(20);

            //_text.Add(textTest);


            float footerHeight = 300;
            Footer footer = new Footer(footerHeight);
            AddUI(footer, 100);

            _footer = footer;


            ToggleableButton toggleableButton = new ToggleableButton(footer.Position + new Vector3(-footer.GetDimensions().X / 2 + 30, 0, 0), new UIScale(0.15f, 0.1f), "^", 0.1f);

            toggleableButton.OnSelectAction = () =>
            {
                UIDimensions buttonDimensions = toggleableButton.GetDimensions();

                Vector3 dim = toggleableButton.GetAnchorPosition(UIAnchorPosition.TopLeft);

                UIList abilityList = new UIList(dim, new UIScale(0.75f, 0.15f), 0.05f) { Ascending = true };

                foreach (Ability ability in CurrentUnit.Abilities.Values) 
                {
                    ListItem newItem = abilityList.AddItem(ability.Name, () => 
                    {
                        DeselectAbility();
                        SelectAbility(ability);
                        toggleableButton.OnMouseUp();
                    });

                    if (ability.GetEnergyCost() > EnergyDisplayBar.CurrentEnergy)
                    {
                        newItem.SetDisabled(true);
                    }
                }

                toggleableButton.AddChild(abilityList);

                abilityList.Anchor = UIAnchorPosition.BottomLeft;
                abilityList.SetPositionFromAnchor(dim);
            };

            toggleableButton.OnDeselectAction = () =>
            {
                List<int> childIDs = new List<int>();
                toggleableButton.Children.ForEach(child =>
                {
                    if (child != toggleableButton.BaseComponent) 
                    {
                        childIDs.Add(child.ObjectID);
                    }
                });

                toggleableButton.RemoveChildren(childIDs);
            };

            footer.AddChild(toggleableButton, 100);

            Button advanceTurnButton = new Button(footer.Position + new Vector3(footer.GetDimensions().X / 4, 0, 0), new UIScale(0.9f, 0.15f), "Advance round", 0.075f);
            TextBox turnCounter = new TextBox(advanceTurnButton.Position + new Vector3(advanceTurnButton.GetDimensions().X / 1.3f, 0, 0), new UIScale(0.3f, 0.15f), "0", 0.075f, true);

            footer.AddChild(advanceTurnButton, 100);
            footer.AddChild(turnCounter, 100);


            //footer.PropertyAnimations.Add(testAnim);

            //advanceTurnButton.OnClickAction = () =>
            //{
            //    AdvanceRound();
            //    turnCounter.TextField.SetTextString(Round.ToString());
            //};



            EnergyDisplayBar energyDisplayBar = new EnergyDisplayBar(new Vector3(30, WindowConstants.ScreenUnits.Y - footer.GetDimensions().Y - 30, 0), new UIScale(1, 1), 10);
            //EnergyDisplayBar energyDisplayBar = new EnergyDisplayBar(new Vector3(30, WindowConstants.ScreenUnits.Y - 200, 0), new Vector2(1, 1), 10);
            energyDisplayBar.Clickable = true;

            EnergyDisplayBar = energyDisplayBar;

            AddUI(energyDisplayBar);



            //UIList testList = new UIList(new Vector3(500, 500, 0), new Vector2(1f, 0.2f), 0.4f) { Ascending = true, ZIndex = 100 };
            //int count = 0;
            //testList.AddItem(count.ToString(), () =>
            //{
            //    count++;
            //    testList.AddItem("List value " + count);
            //    Console.WriteLine("List value " + count);
            //});

            //AddUI(testList, 100);

            Input inputComp = new Input(footer.Position - footer.GetDimensions().X / 6 * Vector3.UnitX, new UIScale(1, 0.12f), "", 0.05f, false, new UIDimensions(10, 30));

            footer.AddChild(inputComp, 100);


            //Scrollable component demo
            //ScrollableArea scrollableComp = new ScrollableArea(WindowConstants.CenterScreen, new UIScale(1.5f, 1), WindowConstants.CenterScreen, new UIScale(5, 5));

            //UIList testList = new UIList(new Vector3(), new UIScale(0.75f, 0.15f), 0.05f);
            //testList.AddItem("Test", () =>
            //{
            //    testList.AddItem("more test");
            //});

            //scrollableComp.BaseComponent.AddChild(testList, 100);
            //scrollableComp.SetVisibleAreaSize(testList.ListItemSize + testList.Margin + testList.ItemMargins + new UIScale(0, testList.ListItemSize.Y * 4));
            //testList.SetPositionFromAnchor(scrollableComp.BaseComponent.GetAnchorPosition(UIAnchorPosition.TopLeft), UIAnchorPosition.TopLeft);

            //UIBlock scrollableParent = new UIBlock(WindowConstants.CenterScreen) { Draggable = true };
            //scrollableParent.AddChild(scrollableComp, 100);

            //scrollableComp.SetPosition(scrollableParent.GetAnchorPosition(UIAnchorPosition.TopLeft) + UIHelpers.BaseMargin);


            //testList.AddItem("TestTwo", () =>
            //{
            //    scrollableParent.SetPosition(scrollableParent.Position + new Vector3(0, 10, 0));
            //});

            //AddUI(scrollableParent, 1000);


            UnitStatusBar guyStatusBar = new UnitStatusBar(guy, _camera);
            AddUI(guyStatusBar);

            badGuy.Name = "Other guy";
            UnitStatusBar guyStatusBar2 = new UnitStatusBar(badGuy, _camera);
            AddUI(guyStatusBar2);


            badGuy.SetShields(5);


            InitiativeOrder = new List<Unit>() { guy, badGuy };

            StartRound();
            advanceTurnButton.OnClickAction = () =>
            {
                CompleteTurn();

                turnCounter.TextField.SetTextString(Round.ToString());
            };

            FillInTeamFog();
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

        private int tilePosition = 0;
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
                if (e.Key == Keys.Right)
                {
                    tilePosition += _tileMaps[0].Height;
                    Vector3 position = _tileMaps[0].GetPositionOfTile(tilePosition);
                    badGuy.BaseObjects[0].SetAnimation(AnimationType.Die, () => badGuy.BaseObjects[0].SetAnimation(AnimationType.Idle));
                }
                if (e.Key == Keys.Left)
                {
                    tilePosition -= _tileMaps[0].Height;
                    Vector3 position = _tileMaps[0].GetPositionOfTile(tilePosition);
                    badGuy.BaseObjects[0].SetAnimation(AnimationType.Die, () => badGuy.BaseObjects[0].SetAnimation(AnimationType.Idle));
                }
                if (e.Key == Keys.Up)
                {
                    tilePosition -= 1;
                    Vector3 position = _tileMaps[0].GetPositionOfTile(tilePosition);
                    badGuy.BaseObjects[0].SetAnimation(AnimationType.Die, () => badGuy.BaseObjects[0].SetAnimation(AnimationType.Idle));
                }
                if (e.Key == Keys.Down)
                {
                    tilePosition += 1;
                    Vector3 position = _tileMaps[0].GetPositionOfTile(tilePosition);
                    badGuy.BaseObjects[0].SetAnimation(AnimationType.Die, () => badGuy.BaseObjects[0].SetAnimation(AnimationType.Idle));
                }

                if (e.Key == Keys.Equal)
                {
                    EnergyDisplayBar.AddEnergy(1);
                }
                if (e.Key == Keys.Minus)
                {
                    EnergyDisplayBar.AddEnergy(-1);
                }
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

        public override void onUnitClicked(Unit unit)
        {
            base.onUnitClicked(unit);

            if (_selectedAbility == null)
            {
                if (!InCombat) 
                {
                    CurrentUnit = unit;
                    _selectedAbility = unit.GetFirstAbilityOfType(DefaultAbilityType);

                    if (_selectedAbility.Type != AbilityTypes.Empty)
                    {
                        SelectAbility(_selectedAbility);
                    }
                }
            }
            else 
            {
                _selectedAbility.OnUnitClicked(unit);
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

                Console.WriteLine(map.ConvertIndexToCoord(tile.TileIndex));
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
                selectedTiles = map.GetVisionInRadius(tile.TileIndex, 6);
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
                map.GetLineOfTiles(unit.TileMapPosition, tile.TileIndex).ForEach(t =>
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
                selectedTiles = map.GetPathToPoint(_units[0].TileMapPosition, tile.TileIndex, 100, new List<TileClassification>() { TileClassification.Ground }, _units);

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
