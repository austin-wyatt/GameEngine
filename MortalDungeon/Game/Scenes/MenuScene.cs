using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.MiscOperations;
using MortalDungeon.Game.Abilities;
using MortalDungeon.Game.GameObjects;
using MortalDungeon.Game.Objects;
using MortalDungeon.Game.UI;
using MortalDungeon.Game.Units;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using static MortalDungeon.Game.UI.GameUIObjects;

namespace MortalDungeon.Game.Scenes
{
    class MenuScene : CombatScene
    {
        private AbilityTypes SelectedAbilityType = AbilityTypes.Move;

        public MenuScene() 
        {
            InitializeFields();
        }

        public override void Load(Camera camera = null, BaseObject cursorObject = null, MouseRay mouseRay = null) 
        {
            base.Load(camera, cursorObject, mouseRay);


            TileMap tileMap = new TileMap(default) { Width = 50, Height = 50};

            tileMap.PopulateTileMap();
            _tileMaps.Add(tileMap);

            Guy guy = new Guy(tileMap.GetPositionOfTile(0) + Vector3.UnitZ * 0.2f, 0) { Clickable = true };
            guy.Team = UnitTeam.Ally;

            _units.Add(guy);

            Guy guy2 = new Guy(tileMap.GetPositionOfTile(3) + Vector3.UnitZ * 0.2f, 0) { Clickable = true };
            guy2.TileMapPosition = 3;
            guy2.Team = UnitTeam.Ally;

            _units.Add(guy2);

            for (int i = 0; i < 1; i++)
            {
                Fire fire = new Fire(new Vector3(1150 + i * 250, 950, 0.2f));

                _renderedObjects.Add(fire);
            }

            MountainTwo mountainBackground = new MountainTwo(new Vector3(30000, 0, -50));
            //mountainBackground.BaseObjects[0].Display.RotateX(-15);
            mountainBackground.BaseObjects[0].GetDisplay().ScaleAll(10);
            _renderedObjects.Add(mountainBackground);

            Text textTest = new Text("Test string", new Vector3(25, -2300, 0.1f), true);
            textTest.SetScale(20);

            _text.Add(textTest);


            float footerHeight = 500;
            Footer footer = new Footer(footerHeight);
            _UI.Add(footer);

            footer.Children[1].OnClickAction = () => 
            {
                onChangeAbilityType(AbilityTypes.Move);

            };

            footer.Children[2].OnClickAction = () =>
            {
                onChangeAbilityType(AbilityTypes.MeleeAttack);
            };

            footer.Children[3].OnClickAction = () =>
            {
                onChangeAbilityType(AbilityTypes.RangedAttack);
            };

            Button advanceTurnButton = new Button(footer.Children[3].Position + new Vector3(footer.Children[3].GetDimensions().X * 2, 0, 0), new Vector2(900, 150), "Advance round", 0.75f);
            TextBox turnCounter = new TextBox(advanceTurnButton.Position + new Vector3(advanceTurnButton.GetDimensions().X / 1.3f, 0, 0), new Vector2(300, 150), "0", 0.75f, true);

            footer.AddChild(advanceTurnButton);
            footer.AddChild(turnCounter);

            PropertyAnimation testAnim = new PropertyAnimation(turnCounter.TextField.Letters[0].LetterObject.BaseFrame, 50) { Playing = true, Repeat = true };
            Keyframe testFrame = new Keyframe(5, (baseFrame) => 
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
            energyDisplayBar.Clickable = true;

            EnergyDisplayBar = energyDisplayBar;

            _UI.Add(energyDisplayBar);

        }


        private Unit selectedUnit = default;
        private List<BaseTile> validTiles = new List<BaseTile>();
        private bool unitSelected = false;
        public override void onMouseUp(MouseButtonEventArgs e)
        {
            base.onMouseUp(e);
        }
        public override void onMouseDown(MouseButtonEventArgs e)
        {
            base.onMouseDown(e);
        }

        private int tilePosition = 0;
        public override void onKeyUp(KeyboardKeyEventArgs e)
        {
            Unit badGuy = _units.Find(g => g.Name == "Guy");
            //Console.WriteLine(badGuy.Position);
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
                EnergyDisplayBar.AddEnergy(1);
            }
            if (e.Key == Keys.Minus)
            {
                EnergyDisplayBar.AddEnergy(-1);
            }
            base.onKeyUp(e);
        }

        public override bool onMouseMove(MouseMoveEventArgs e)
        {

            return base.onMouseMove(e);
        }

        public override void onUnitClicked(Unit unit)
        {
            base.onUnitClicked(unit);

            selectedUnit = unit;
            Ability selectedAbility = unit.GetFirstAbilityOfType(SelectedAbilityType);


            if (_tileMaps[0].IsValidTile(unit.TileMapPosition) && selectedAbility.Type != AbilityTypes.Empty)
            {
                _tileMaps[0].SetDefaultTileValues();
                //_tileMaps[0][selectedUnit.TileMapPosition].SetColor(new Vector4(1.0f, 0, 0.5f, 1));



                //validTiles = _tileMaps[0].GetTilesInRadius(unit.TileMapPosition, 10);
                validTiles = selectedAbility.GetValidTileTargets(_tileMaps[0], _units);
                validTiles.ForEach(tile =>
                {
                    tile.SetColor(tile.DefaultColor - new Vector4(0.1f, 0.5f, 0.5f, 0));
                });

                unitSelected = true;
            }
        }

        public override void onTileClicked(TileMap map, BaseTile tile) 
        {
            base.onTileClicked(map, tile);

            if (unitSelected)
            {
                if (validTiles.Exists(t => t.TileIndex == tile.TileIndex)) 
                {
                    Ability selectedAbility = selectedUnit.GetFirstAbilityOfType(SelectedAbilityType);
                    selectedAbility.SelectedTile = tile;
                    selectedAbility.EnactEffect();


                    _tileMaps[0].SetDefaultTileValues();
                    unitSelected = false;
                    onChangeAbilityType(AbilityTypes.Empty);
                }
            }
            else if (KeyboardState.IsKeyDown(Keys.LeftControl)) 
            {
                tile.SetFogColor();
                tile.TileClassification = TileClassification.AttackableTerrain;
                tile.BlocksVision = true;
                tile.SetAnimation(AnimationType.Idle);
                Console.WriteLine(map.ConvertIndexToCoord(tile.TileIndex));
            }
            else if (KeyboardState.IsKeyDown(Keys.LeftAlt))
            {
                //map.SetDefaultTileValues();
                map.Tiles.ForEach(t => {
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
                validTiles = map.GetVisionInRadius(tile.TileIndex, 6);
                validTiles.ForEach(t =>
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
                validTiles.ForEach(t =>
                {
                    if (t.CurrentAnimation != BaseTileAnimationType.SolidWhite) 
                    {
                        t.SetColor(t.DefaultColor);
                        t.SetAnimation(t.DefaultAnimation);
                    }

                });
                validTiles = map.GetPathToPoint(_units[0].TileMapPosition, tile.TileIndex, 100, new List<TileClassification>() { TileClassification.Ground }, _units);

                if (validTiles.Count == 0)
                {
                    validTiles.Add(tile);
                    tile.SetColor(new Vector4(1f, 0f, 0f, 1));
                }
                else 
                {
                    validTiles.ForEach(t =>
                    {
                        t.SetColor(new Vector4(0.75f, 0.5f, 0.5f, 1));
                        t.SetAnimation(t.DefaultAnimation);
                    });
                }
            }

        }

        public void onChangeAbilityType(AbilityTypes type) 
        {
            SelectedAbilityType = type;

            List<Button> buttons = GetDerivedClassesFromList<Button, UIObject>(_UI[0].Children); //footer buttons

            int selectedIndex = 0;
            switch (type) 
            {
                case AbilityTypes.Move:
                    selectedIndex = 0;
                    break;
                case AbilityTypes.MeleeAttack:
                    selectedIndex = 1;
                    break;
                case AbilityTypes.RangedAttack:
                    selectedIndex = 2;
                    break;
                case AbilityTypes.Empty:
                    selectedIndex = -1;
                    break;
            }

            int count = 0;
            buttons.ForEach(button =>
            {
                if (count == selectedIndex)
                {
                    button.SetColor(button.BaseColor - new Vector4(0.5f, 0.5f, 0.5f, 0));
                    button.Selected = true;
                }
                else 
                {
                    button.Selected = false;
                    button.SetColor(button.BaseColor);
                }

                count++;
            });
        }
    }


}
