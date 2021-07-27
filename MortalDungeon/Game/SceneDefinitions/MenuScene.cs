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
        public MenuScene() 
        {
            InitializeFields();
        }

        Footer _footer;

        public override void Load(Camera camera = null, BaseObject cursorObject = null, MouseRay mouseRay = null) 
        {
            base.Load(camera, cursorObject, mouseRay);


            Texture fogTex = Texture.LoadFromFile("Resources/FogTexture.png", default, TextureName.FogTexture);

            fogTex.Use(TextureUnit.Texture1);



            TileMap tileMap = new TileMap(default) { Width = 50, Height = 50 };
            //TileMap tileMap = new TileMap(default) { Width = 5, Height = 5 };

            tileMap.PopulateTileMap();
            _tileMaps.Add(tileMap);

            tileMap.Tiles.ForEach(tile =>
            {
                tile.MultiTextureData.MixedTexture = fogTex;
                tile.MultiTextureData.MixedTextureLocation = TextureUnit.Texture1;
                tile.MultiTextureData.MixedTextureName = TextureName.FogTexture;

                tile.SetFog(true);
                tile.SetExplored(false);
            });

            Guy guy = new Guy(tileMap.GetPositionOfTile(0) + Vector3.UnitZ * 0.2f, 0) { Clickable = true };
            guy.Team = UnitTeam.Ally;
            guy.CurrentTileMap = tileMap;
            CurrentUnit = guy;

            _units.Add(guy);

            Guy guy2 = new Guy(tileMap.GetPositionOfTile(3) + Vector3.UnitZ * 0.2f, 0) { Clickable = true };
            guy2.TileMapPosition = 3;
            guy2.Team = UnitTeam.Ally;
            guy2.CurrentTileMap = tileMap;

            _units.Add(guy2);

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
            AddUI(footer);

            _footer = footer;

            //footer.Buttons[0].OnClickAction = () =>
            //{
            //    onChangeAbilityType(AbilityTypes.Move);

            //};

            //footer.Buttons[1].OnClickAction = () =>
            //{
            //    onChangeAbilityType(AbilityTypes.MeleeAttack);
            //};

            //footer.Buttons[2].OnClickAction = () =>
            //{
            //    onChangeAbilityType(AbilityTypes.RangedAttack);
            //};

            ToggleableButton toggleableButton = new ToggleableButton(footer.Position + new Vector3(-footer.GetDimensions().X / 2 + 30, 0, 0), new Vector2(0.15f, 0.1f), "^", 1);

            toggleableButton.OnSelectAction = () =>
            {
                Vector3 buttonDimensions = toggleableButton.GetDimensions();
                UIList abilityList = new UIList(toggleableButton.Position + new Vector3(-buttonDimensions.X / 2 + 5, 0, 0), 
                    new Vector2(0.75f, 0.15f), 0.5f) { Ascending = true};


                foreach (Ability ability in CurrentUnit.Abilities.Values) 
                {
                    abilityList.AddItem(ability.Name, () => 
                    {
                        SelectAbility(ability);
                    });
                }

                toggleableButton.AddChild(abilityList);
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

            Button advanceTurnButton = new Button(footer.Position + new Vector3(footer.GetDimensions().X / 4, 0, 0), new Vector2(0.9f, 0.15f), "Advance round", 0.75f);
            TextBox turnCounter = new TextBox(advanceTurnButton.Position + new Vector3(advanceTurnButton.GetDimensions().X / 1.3f, 0, 0), new Vector2(0.3f, 0.15f), "0", 0.75f, true);

            footer.AddChild(advanceTurnButton, 100);
            footer.AddChild(turnCounter, 100);

            PropertyAnimation testAnim = new PropertyAnimation(turnCounter.TextField.Letters[0].LetterObject.BaseFrame, 50) { Playing = true, Repeat = true };
            Keyframe testFrame = new Keyframe(1, (baseFrame) =>
            {
                AdvanceRound();
                turnCounter.TextField.SetTextString(Round.ToString());
            });

            testAnim.Keyframes.Add(testFrame);

            footer.PropertyAnimations.Add(testAnim);

            advanceTurnButton.OnClickAction = () =>
            {
                AdvanceRound();
                turnCounter.TextField.SetTextString(Round.ToString());
            };



            EnergyDisplayBar energyDisplayBar = new EnergyDisplayBar(new Vector3(30, WindowConstants.ScreenUnits.Y - footer.GetDimensions().Y - 30, 0), new Vector2(1, 1), 10);
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

            if (!GetBit(_interceptKeystrokes, ObjectType.All))
            {

                if (MouseState.ScrollDelta[1] < 0)
                {
                    Vector3 movement = _camera.Front * _cameraSpeed / 2;
                    if (_camera.Position.Z - movement.Z < 26)
                    {
                        _camera.SetPosition(_camera.Position - movement); // Backwards
                        onMouseMove();
                    }
                }
                else if (MouseState.ScrollDelta[1] > 0)
                {
                    Vector3 movement = _camera.Front * _cameraSpeed / 2;
                    if (_camera.Position.Z + movement.Z > 0)
                    {
                        _camera.SetPosition(_camera.Position + movement); // Forward
                        onMouseMove();
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
                }

                if (KeyboardState.IsKeyDown(Keys.S))
                {
                    //_camera.Position -= _camera.Front * cameraSpeed * (float)args.Time; // Backwards
                    //_camera.Position -= _camera.Up * cameraSpeed * (float)args.Time; // Down
                    _camera.SetPosition(_camera.Position - Vector3.UnitY * _cameraSpeed * (float)args.Time);
                    onMouseMove();
                }
                if (KeyboardState.IsKeyDown(Keys.A))
                {
                    //_camera.Position -= _camera.Right * _cameraSpeed * (float)args.Time; // Left
                    _camera.SetPosition(_camera.Position - _camera.Right * _cameraSpeed * (float)args.Time);
                    onMouseMove();
                }
                if (KeyboardState.IsKeyDown(Keys.D))
                {
                    //_camera.Position += _camera.Right * _cameraSpeed * (float)args.Time; // Right
                    _camera.SetPosition(_camera.Position + _camera.Right * _cameraSpeed * (float)args.Time);
                    onMouseMove();
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
