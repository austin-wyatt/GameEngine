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
using MortalDungeon.Game.Structures;
using MortalDungeon.Game.Objects.PropertyAnimations;
using MortalDungeon.Game.Lighting;
using MortalDungeon.Engine_Classes.Audio;
using MortalDungeon.Game.Entities;
using MortalDungeon.Game.UI.Dev;
using MortalDungeon.Engine_Classes.Rendering;
using System.Xml.Serialization;
using System.IO;
using MortalDungeon.Game.Save;
using MortalDungeon.Game.Serializers;

namespace MortalDungeon.Game.SceneDefinitions
{
    class MenuScene : CombatScene
    {
        private EntityManagerUI _entityManager;
        private FeatureManagerUI _featureManagerUI;

        public MenuScene() : base()
        {
            InitializeFields();
        }

        public override void Load(Camera camera = null, BaseObject cursorObject = null, MouseRay mouseRay = null) 
        {
            base.Load(camera, cursorObject, mouseRay);


            UIBlock inputCapture = new UIBlock(WindowConstants.CenterScreen, new UIScale(10, 10));
            inputCapture.Scrollable = true;

            inputCapture.SetColor(Colors.Transparent);

            inputCapture.Scroll += (s, e) =>
            {
                if (MouseState.ScrollDelta[1] < 0)
                {
                    Vector3 movement = _camera.Front * 1.0f;
                    if (_camera.Position.Z - movement.Z < 26)
                    {
                        _camera.SetPosition(_camera.Position - movement); // Backwards
                        OnMouseMove();
                        OnCameraMoved();
                    }
                }
                else if (MouseState.ScrollDelta[1] > 0)
                {
                    Vector3 movement = _camera.Front * 1.0f;
                    if (_camera.Position.Z + movement.Z > 1)
                    {
                        _camera.SetPosition(_camera.Position + movement); // Forward
                        OnMouseMove();
                        OnCameraMoved();
                    }
                }
            };

            UIManager.AddUIObject(inputCapture, -10000);


            _entityManager = new EntityManagerUI(this);

            PathParams riverParams = new PathParams(new FeaturePoint(-1000, 27), new FeaturePoint(1000, 50), 3);
            riverParams.AddStop(new FeaturePoint(25, 45));
            riverParams.AddStop(new FeaturePoint(40, 30));
            riverParams.AddStop(new FeaturePoint(80, 35));

            River_1 river = new River_1(riverParams);


            PathParams pathParams = new PathParams(new FeaturePoint(-45, -3000), new FeaturePoint(0, 1000), 3);
            pathParams.AddMeanderingPoints(20, 0.25f, 1, 0, 123456);

            Path_1 path = new Path_1(pathParams);


            ForestParams forestParams = new ForestParams(new FeaturePoint(0, 0), 200, 0.1);
            Forest_1 forest = new Forest_1(forestParams);

            GraveyardParams graveyardParams = new GraveyardParams(new FeaturePoint(0, 100), 30, 15, 0.1f);
            Graveyard_1 graveyard = new Graveyard_1(graveyardParams);


            BanditCamp camp = new BanditCamp(new BanditCampParams() { Origin = new FeaturePoint(-15, -100) });




            //river.GenerateFeature();
            //_tileMapController.AddFeature(river);

            //path.GenerateFeature();
            //_tileMapController.AddFeature(path);

            //graveyard.GenerateFeature();
            //_tileMapController.AddFeature(graveyard);

            //forest.GenerateFeature();
            //_tileMapController.AddFeature(forest);

            //camp.GenerateFeature();
            //_tileMapController.AddFeature(camp);



            //TestTileMap tileMap = new TestTileMap(default, new TileMapPoint(0, 0), _tileMapController) { Width = 50, Height = 50 };
            //tileMap.PopulateTileMap();

            //_tileMapController.AddTileMap(new TileMapPoint(0, 0), tileMap);
            //_tileMapController.ApplyLoadedFeaturesToMap(tileMap);

            //_tileMapController.LoadSurroundingTileMaps(tileMap.TileMapCoords);
            _tileMapController.LoadSurroundingTileMaps(new TileMapPoint(0, 0));



            UnitTeam.PlayerUnits.SetRelation(UnitTeam.Skeletons, Relation.Hostile);
            UnitTeam.PlayerUnits.SetRelation(UnitTeam.PlayerUnits, Relation.Friendly);
            UnitTeam.PlayerUnits.SetRelation(UnitTeam.BadGuys, Relation.Neutral);



            //UIBlock statusBarContainer = new UIBlock(new Vector3());
            //statusBarContainer.MultiTextureData.MixTexture = false;
            //statusBarContainer.SetAllInline(0);
            //statusBarContainer.SetColor(Colors.Transparent);

            //for (int j = 0; j < 20; j++)
            //{
            //    for (int i = 0; i < 10; i++)
            //    {
            //        Skeleton skele = new Skeleton(tileMap[10 + j, i * 2].Position + new Vector3(0, -tileMap.Tiles[0].GetDimensions().Y / 2, 0.2f), this, tileMap[10 + j, i * 2]) { Clickable = true };
            //        //Skeleton skele = new Skeleton(tileMap[10, i * 2].Position + new Vector3(0, -tileMap.Tiles[0].GetDimensions().Y / 2, 0.2f), this, tileMap[10, i * 2]) { Clickable = true };
            //        //skele.SetTeam(UnitTeam.PlayerUnits);
            //        skele.SetTeam((UnitTeam)(new Random().Next() % 3 + 1));
            //        skele.SetColor(new Vector4(0.76f, 0.14f, 0.26f, 1));

            //        skele.SelectionTile.UnitOffset.Y += tileMap.Tiles[0].GetDimensions().Y / 2;
            //        skele.SelectionTile.SetPosition(skele.Position);

            //        skele.AI.ControlType = ControlType.Controlled;


            //        //UnitStatusBar skeleStatusBar = new UnitStatusBar(skele, _camera);

            //        //statusBarContainer.AddChild(skeleStatusBar);

            //        _units.Add(skele);
            //    }
            //}

            //for (int i = 0; i < 1; i++)
            //{
            //    Fire fire = new Fire(new Vector3(1150 + i * 250, 950, 0.2f));

            //    _genericObjects.Add(fire);
            //}

            //MountainTwo mountainBackground = new MountainTwo(new Vector3(30000, 0, -50));
            //mountainBackground.BaseObjects[0].GetDisplay().ScaleAll(10);
            //_genericObjects.Add(mountainBackground);
            //mountainBackground.SetPosition(new Vector3(guy.Position.X, guy.Position.Y - 200, -50));

            //_camera.SetPosition(new Vector3(guy.Position.X / WindowConstants.ScreenUnits.X * 2, guy.Position.Y / WindowConstants.ScreenUnits.Y * -2, _camera.Position.Z));


            float footerHeight = 300;

            Footer = new GameFooter(footerHeight, this);
            AddUI(Footer);

            OnNumberPressed += (args) =>
            {
                if(Footer._currentUnit == null) 
                {
                    Unit unit = _units.Find(u => u.AI.Team == UnitTeam.PlayerUnits);

                    if(unit != null) 
                    {
                        Footer.UpdateFooterInfo(unit);
                    }
                }
            };


            EnergyDisplayBar energyDisplayBar = new EnergyDisplayBar(this, new Vector3(30, WindowConstants.ScreenUnits.Y - Footer.GetDimensions().Y - 30, 0), new UIScale(1, 1), 10);

            EnergyDisplayBar = energyDisplayBar;

            AddUI(energyDisplayBar);


            EnergyDisplayBar actionEnergyBar = new EnergyDisplayBar(this, new Vector3(30, WindowConstants.ScreenUnits.Y - Footer.GetDimensions().Y - 100, 0), new UIScale(1, 1), 4, (int)IconSheetIcons.Channel, Spritesheets.IconSheet);

            ActionEnergyBar = actionEnergyBar;

            AddUI(actionEnergyBar);


            TurnDisplay = new TurnDisplay();
            TurnDisplay.SetPosition(WindowConstants.CenterScreen - new Vector3(0, WindowConstants.ScreenUnits.Y / 2 - 50, 0));

            AddUI(TurnDisplay);


            if (true) 
            {
                //Guy guy = new Guy(this, tileMap[0, 0]) { Clickable = true };
                //guy.SetTeam(UnitTeam.PlayerUnits);
                //CurrentUnit = guy;

                //guy.pack_name = "player_party";



                //guy.Info.PrimaryUnit = true;

                //var creationInfo = UnitCreationInfoSerializer.LoadUnitCreationInfoFromFile(2);

                //Unit guy = creationInfo.CreateUnit(this);


                //Guy badGuy = new Guy(this, tileMap[1, 1]) { Clickable = true };
                //badGuy.SetTeam(UnitTeam.PlayerUnits);

                //badGuy.AI.ControlType = ControlType.Controlled;

                //badGuy.Name = "Frend";
                //badGuy.pack_name = "player_party";


                //Skeleton skeleton = new Skeleton(this, tileMap[20, 5]) { };
                //skeleton.Name = "John";

                //skeleton.SetTeam(UnitTeam.Skeletons);
                //skeleton.AI.ControlType = ControlType.Basic_AI;


                //badGuy.SetShields(5);
                //guy.SetShields(5);


                //Entity guyEntity = new Entity(guy);
                //guyEntity.Load(new FeaturePoint(0, 0));
                //EntityManager.AddEntity(guyEntity);

                //Entity badGuyEntity = new Entity(badGuy);
                //badGuyEntity.Load(new FeaturePoint(badGuy.Info.TileMapPosition));
                //EntityManager.AddEntity(badGuyEntity);

                //badGuyEntity.Handle.SetColor(new Vector4(0.76f, 0.14f, 0.26f, 1));

                //Entity skeletonEntity = new Entity(skeleton);
                //skeletonEntity.Load(new FeaturePoint(skeleton.Info.TileMapPosition));
                //EntityManager.AddEntity(skeletonEntity);
            }


            //GameObject tent1 = new GameObject();
            //tent1.AddBaseObject(_3DObjects.CreateBaseObject(new SpritesheetObject(0, Textures.TentTexture), _3DObjects.Tent, default));

            //tent1.SetPosition(new Vector3(2000, 0, 0.2f));

            //tent1.BaseObject.BaseFrame.SetScale(0.5f, 0.5f, 0.25f);
            //_genericObjects.Add(tent1);


            RenderingConstants.LightPosition = new Vector3(50000, 0, 100);

            EnvironmentColor.OnChangeEvent += (color, _) =>
            {
                RenderingConstants.LightColor = EnvironmentColor.ToVector();
            };

            EnvironmentColor._onChange();


            ShowEnergyDisplayBars(false);
            Footer.EndTurnButton.SetRender(false);
            FillInTeamFog();
            EvaluateCombat();
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

            switch (e.Key) 
            {
                case Keys.F11:
                    _3DObjects.PrintObjectVertices(_3DObjects.Cube);
                    break;
                case Keys.F1:
                    if (_entityManager.Displayed)
                    {
                        RemoveUI(_entityManager.Window);
                        _entityManager.Displayed = false;
                    }
                    else 
                    {
                        _entityManager.PopulateEntityList();
                        AddUI(_entityManager.Window, 100000);
                        _entityManager.Displayed = true;
                    }
                    break;
                case Keys.F2:

                    if (_featureManagerUI != null)
                    {
                        RemoveUI(_featureManagerUI.Window);

                        _featureManagerUI = null;
                    }
                    else
                    {
                        _featureManagerUI = new FeatureManagerUI(this, () =>
                        {
                            RemoveUI(_featureManagerUI.Window);
                            _featureManagerUI = null;
                        });

                        AddUI(_featureManagerUI.Window, 100000);
                    }
                    break;
                case Keys.M:
                    if(WorldMap.Displayed)
                    {
                        UIManager.RemoveUIObject(WorldMap.Window);
                        WorldMap.Displayed = false;
                    }
                    else
                    {
                        WorldMap.PopulateFeatures();
                        UIManager.AddUIObject(WorldMap.Window, 99999);
                        WorldMap.Displayed = true;
                    }
                    break;
                case Keys.Q:
                    if (CurrentUnit != null && CurrentUnit.Render && CurrentUnit.Info._movementAbility != null && !CurrentUnit.Info._movementAbility.Moving) 
                    {
                        SmoothPanCameraToUnit(CurrentUnit, 1);
                        DeselectAllUnits();
                        CurrentUnit.Select();
                    }
                    break;
                case Keys.E:
                    if (Footer.EndTurnButton != null && Footer.EndTurnButton.Render)
                    {
                        Footer.EndTurnButton.OnClick();
                    }
                    break;
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

            float _cameraSpeed = 8.0f;
            float _zoomSpeed = 1.0f;

            if (!GetBit(_interceptKeystrokes, ObjectType.All) && _focusedObj == null)
            {
                //if (MouseState.ScrollDelta[1] < 0)
                //{
                //    Vector3 movement = _camera.Front * _zoomSpeed;
                //    if (_camera.Position.Z - movement.Z < 26)
                //    {
                //        _camera.SetPosition(_camera.Position - movement); // Backwards
                //        OnMouseMove();
                //        OnCameraMoved();
                //    }
                //}
                //else if (MouseState.ScrollDelta[1] > 0)
                //{
                //    Vector3 movement = _camera.Front * _zoomSpeed;
                //    if (_camera.Position.Z + movement.Z > 1)
                //    {
                //        _camera.SetPosition(_camera.Position + movement); // Forward
                //        OnMouseMove();
                //        OnCameraMoved();
                //    }
                //}

                if (_camera.Position.Z > 20)
                {
                    _cameraSpeed += 16;
                }
                else if (_camera.Position.Z > 15)
                {
                    _cameraSpeed += 8;
                }
                else if (_camera.Position.Z <= 8)
                {
                    _cameraSpeed -= 2;
                }

                if (KeyboardState.IsKeyDown(Keys.LeftShift))
                {
                    _cameraSpeed *= 20;
                }

                if (!ContextManager.GetFlag(GeneralContextFlags.CameraPanning))
                {
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
                        //_camera.SetPosition(_camera.Position - _camera.Right * _cameraSpeed * (float)args.Time);
                        _camera.SetPosition(_camera.Position - Vector3.UnitX * _cameraSpeed * (float)args.Time);
                        OnMouseMove();
                        OnCameraMoved();
                    }
                    if (KeyboardState.IsKeyDown(Keys.D))
                    {
                        //_camera.Position += _camera.Right * _cameraSpeed * (float)args.Time; // Right
                        //_camera.SetPosition(_camera.Position + _camera.Right * _cameraSpeed * (float)args.Time);
                        _camera.SetPosition(_camera.Position + Vector3.UnitX * _cameraSpeed * (float)args.Time);
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


        private int _counter = 0;
        private List<Vector3i> _cubeCoordinates = new List<Vector3i>();
        public override void OnTileClicked(TileMap map, BaseTile tile, MouseButton button, ContextManager<MouseUpFlags> flags)
        {
            base.OnTileClicked(map, tile, button, flags);

            if (button == MouseButton.Left)
            {
                if (tile.UnitOnTile != null)
                {
                    if (KeyboardState.IsKeyDown(Keys.F10))
                    {
                        var unit = tile.UnitOnTile;
                        Console.WriteLine($"{unit.Name}: FeatureID {unit.FeatureID}  Feature Hash {unit.ObjectHash}");
                    }


                    OnUnitClicked(tile.UnitOnTile, button);
                }
                else if (_selectedAbility != null)
                {
                    _selectedAbility.OnTileClicked(map, tile);
                }
                else
                {
                    Sound sound = new Sound(Sounds.Select) { Gain = 0.15f, Pitch = GlobalRandom.NextFloat(0.6f, 0.6f) };
                    sound.Play();

                    if (KeyboardState.IsKeyDown(Keys.LeftAlt))
                    {

                    }
                    else if (KeyboardState.IsKeyDown(Keys.RightAlt))
                    {
                        _tileMapController.TileMaps.ForEach(m => m.PopulateFeatures());
                    }
                    else if (KeyboardState.IsKeyDown(Keys.RightShift))
                    {
                        Unit unit = _units[0];
                        map.GetLineOfTiles(unit.Info.TileMapPosition, tile.TilePoint).ForEach(t =>
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
                    else if (KeyboardState.IsKeyDown(Keys.F))
                    {
                        QuestManager.StartQuest(0);
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
                        Window._renderedItems.Clear();

                        Object3D obj = OBJParser.ParseOBJ("Resources/Wall.obj");

                        //SpritesheetObject spritesheetObject = new SpritesheetObject(50, Spritesheets.StructureSheet);
                        SpritesheetObject spritesheetObject = new SpritesheetObject(50, Spritesheets.StructureSheet, 1, 1);

                        RenderableObject testObj = new RenderableObject(spritesheetObject.Create3DObjectDefinition(obj), new Vector4(1, 1, 1, 1), Shaders.DEFAULT_SHADER);

                        //BaseObject baseObject = new BaseObject(testObj, 0, "test", WindowConstants.CenterScreen + new Vector3(0, 0, 4f));
                        BaseObject baseObject = _3DObjects.CreateBaseObject(spritesheetObject, _3DObjects.WallCornerObj, WindowConstants.CenterScreen + new Vector3(0, 0, 4f));
                        baseObject.BaseFrame.CameraPerspective = true;

                        //Engine_Classes.Rendering.Renderer.LoadTextureFromBaseObject(baseObject);

                        //Window._renderedItems.Add(baseObject);

                        GameObject gameObj = new GameObject();
                        gameObj.AddBaseObject(baseObject);

                        _genericObjects.Add(gameObj);
                    }
                    else if (KeyboardState.IsKeyDown(Keys.GraveAccent))
                    {
                        if (_wallTemp == null)
                        {
                            _wallTemp = tile.TilePoint;
                        }
                        else
                        {
                            //TestTileMap temp = map as TestTileMap;
                            //List<BaseTile> tiles = new List<BaseTile>();

                            TileMap.PathToPointParameters param = new TileMap.PathToPointParameters(_wallTemp, tile.TilePoint, 100)
                            {
                                TraversableTypes = new List<TileClassification>() { TileClassification.Ground, TileClassification.Terrain, TileClassification.Water }
                            };

                            List<BaseTile> tiles = map.GetPathToPoint(param);

                            //map.GetRingOfTiles(_wallTemp, tiles, 10);

                            //tiles.Insert(tiles.Count, tiles[0]);
                            //tiles.Remove(tiles[0]);
                            //tiles.Insert(tiles.Count, tiles[0]);
                            //tiles.Remove(tiles[0]);

                            Wall.CreateWalls(map, tiles, Wall.WallMaterial.Iron);

                            (List<Wall> walls, bool circular) = Wall.FindAdjacentWalls(_wallTemp.ParentTileMap[_wallTemp].Structure as Wall);
                            Wall.UnifyWalls(walls, circular);

                            walls.ForEach(w => w.Name = "fence");

                            //Console.WriteLine(VisionMap.TargetInVision(_wallTemp, tile.TilePoint, 10, this));

                            _wallTemp = null;
                        }
                    }
                    else if (KeyboardState.IsKeyDown(Keys.LeftControl))
                    {
                        //if (tile.Structure != null)
                        //{
                        //    Wall wall = tile.Structure as Wall;
                        //    wall.CreateDoor(tile);
                        //}
                    }
                    else if (KeyboardState.IsKeyDown(Keys.F1))
                    {
                        //List<BaseTile> tiles = new List<BaseTile>();

                        //tiles = map.GetVisionInRadius(tile.TilePoint, 6);

                        //tiles.ForEach(tile =>
                        //{
                        //    tile.Properties.Type = (TileType)(rand.Next(3) + (int)TileType.Stone_1);

                        //    if (rand.NextDouble() > 0.9)
                        //    {
                        //        tile.Properties.Type = TileType.Gravel;
                        //    }

                        //    tile.Update();
                        //});
                    }
                    else if (KeyboardState.IsKeyDown(Keys.F2))
                    {
                        List<BaseTile> tiles = new List<BaseTile>();

                        tiles = map.GetVisionInRadius(tile.TilePoint, 6);

                        tiles.ForEach(tile =>
                        {
                            if (rand.NextDouble() > 0.8)
                            {
                                tile.Properties.Type = TileType.Dirt;
                            }

                            tile.Update();
                        });
                    }
                    else if (KeyboardState.IsKeyDown(Keys.F3))
                    {
                        SaveState state = SaveState.CreateSaveState(this);
                        SaveState.WriteSaveStateToFile("Data/save_state", state);
                    }
                    else if (KeyboardState.IsKeyDown(Keys.F4))
                    {
                        SaveState state = SaveState.LoadSaveStateFromFile("Data/save_state");

                        SaveState.LoadSaveState(this, state);
                    }
                    else if (KeyboardState.IsKeyDown(Keys.F6))
                    {
                        string text = "";
                        EventSeverity severity = EventSeverity.Info;

                        int rand = GlobalRandom.Next(5);

                        switch (rand)
                        {
                            case 0:
                                text = "You approach a goblin. He looks confused.";
                                text = "Oh no! You've left your shoelaces untied and you have tripped. You and everyone you know dies instantly.";
                                break;
                            case 1:
                                text = "Oh no! You've left your shoelaces untied and you have tripped. You and everyone you know dies instantly.";
                                severity = EventSeverity.Severe;
                                break;
                            case 2:
                                text = "Oh! A stroke of good luck. You've found a nickel wedged betwixt some rocks.";
                                severity = EventSeverity.Positive;
                                break;
                            case 3:
                                text = "You don't believe that you've been in this area before. Perhaps it's best to proceed cautiously.";
                                severity = EventSeverity.Caution;
                                break;
                            case 4:
                                text = "You sense that the foes around here aren't what you're used to. Is whatever you're doing worth it?";
                                severity = EventSeverity.Caution;
                                break;

                        }

                        Footer.EventLog.AddEvent(text, severity);
                    }
                    else if (ContextManager.GetFlag(GeneralContextFlags.PatternToolEnabled) && KeyboardState.IsKeyDown(Keys.F7))
                    {
                        Console.Write("TilePattern = new List<Vector3i> {");
                        if (_cubeCoordinates.Count == 0)
                            return;

                        Vector3i initial = _cubeCoordinates[0];

                        Console.Write($"new Vector3i({0}, {0}, {0}), ");

                        for (int i = 1; i < _cubeCoordinates.Count; i++) 
                        {
                            Vector3i diff = initial - _cubeCoordinates[i];
                            Console.Write($"new Vector3i({diff.X}, {diff.Y}, {diff.Z}){(i == _cubeCoordinates.Count - 1 ? "" : ", ")}");
                        }
                        Console.Write("};\n");
                    }
                    else if (ContextManager.GetFlag(GeneralContextFlags.PatternToolEnabled)) 
                    {
                        Vector3i cubeCoords = CubeMethods.OffsetToCube(new FeaturePoint(tile));
                        _cubeCoordinates.Add(cubeCoords);

                        tile.Color = Colors.Red;
                        tile.Update();
                    }
                    else if (KeyboardState.IsKeyDown(Keys.F9))
                    {
                        List<Unit> units = new List<Unit>();

                        units.Add(_units.Find(u => u.Name == "Guy"));
                        units.Add(_units.Find(u => u.Name == "Frend"));

                        Quest activeQuest = QuestManager.Quests.Find(q => q.ID == 50);

                        if (activeQuest != null)
                        {
                            DialogueWindow.StartDialogue(DialogueSerializer.LoadDialogueFromFile(2), units);
                        }
                        else
                        {
                            DialogueWindow.StartDialogue(DialogueSerializer.LoadDialogueFromFile(0), units);
                        }
                    }
                }
            }
            else
            {
                if (_selectedUnits.Count != 0)
                {
                    DeselectUnits();
                }
                else 
                {
                    if (ContextManager.GetFlag(GeneralContextFlags.PatternToolEnabled))
                    {
                        Vector3i cubeCoords = CubeMethods.OffsetToCube(new FeaturePoint(tile));
                        _cubeCoordinates.Remove(cubeCoords);

                        tile.Color = Colors.White;
                        tile.Update();

                        return;
                    }

                    tile.OnRightClick(flags);
                }
            }
        }

        private TilePoint _wallTemp = null;
        private int temp = 0;
    }
}
