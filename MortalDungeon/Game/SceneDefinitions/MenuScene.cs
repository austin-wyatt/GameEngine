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
using System.Drawing;
using System.Threading.Tasks;
using MortalDungeon.Game.Player;
using MortalDungeon.Game.Items;
using MortalDungeon.Game.Events;
using System.Reflection;

namespace MortalDungeon.Game.SceneDefinitions
{
    class MenuScene : CombatScene
    {
        private EntityManagerUI _entityManager;
        private FeatureManagerUI _featureManagerUI;

        public QuestLog QuestLog;
        public AbilityTreeUI AbilityTreeUI;
        public InventoryUI InventoryUI;
        public EquipmentUI EquipmentUI;

        public MenuScene() : base()
        {
            InitializeFields();
        }

        public override void Load(Camera camera = null, MouseRay mouseRay = null) 
        {
            base.Load(camera, mouseRay);

            TileMapManager.Scene = this;
            PlayerParty.Scene = this;
            EventManager.Scene = this;
            VisionManager.Scene = this;


            _camera.RotateByAmount(0);

            UIBlock inputCapture = new UIBlock(WindowConstants.CenterScreen, new UIScale(10, 10));
            inputCapture.Scrollable = true;

            inputCapture.SetColor(_Colors.Transparent);

            inputCapture.Scroll += (s, e) =>
            {
                if (!ContextManager.GetFlag(GeneralContextFlags.DisallowCameraMovement) && TileMapsFocused)
                {
                    if (MouseState.ScrollDelta[1] < 0)
                    {
                        Vector3 movement = _camera.Front * 1.0f;
                        if (_camera.Position.Z - movement.Z < 26)
                        {
                            _camera.SetPosition(_camera.Position - movement); // Backwards
                                                                              //OnMouseMove();
                                                                              //OnCameraMoved();
                        }
                    }
                    else if (MouseState.ScrollDelta[1] > 0)
                    {
                        Vector3 movement = _camera.Front * 1.0f;
                        if (_camera.Position.Z + movement.Z > 1)
                        {
                            _camera.SetPosition(_camera.Position + movement); // Forward
                                                                              //OnMouseMove();
                                                                              //OnCameraMoved();
                        }
                    }
                }
            };

            UIManager.AddUIObject(inputCapture, -10000);


            _entityManager = new EntityManagerUI(this);


            //TileMapPoint cameraPos = _tileMapController.GlobalPositionToMapPoint(_camera.Position);
            TileMapPoint cameraPos = TileMapHelpers.GlobalPositionToMapPoint(_camera.Position);
            _camera.Update += (cam) =>
            {
                Window.RenderBegin -= UpdateUnitStatusBars;
                Window.RenderBegin += UpdateUnitStatusBars;

                if (BoxSelectHelper.BoxSelecting)
                {
                    BoxSelectHelper.DrawSelectionBox();
                }

                //TileMapPoint newPos = _tileMapController.GlobalPositionToMapPoint(cam.Position);
                TileMapPoint newPos = TileMapHelpers.GlobalPositionToMapPoint(cam.Position);

                if(newPos != cameraPos && newPos != null)
                {
                    //ContextManager.SetFlag(GeneralContextFlags.DisallowCameraMovement, true);
                    cameraPos = newPos;

                    var maps = TileMapManager.GetTileMapsInDiameter(cameraPos, 5);

                    if(maps.Count < 5 * 5 && !InCombat && 
                       !ContextManager.GetFlag(GeneralContextFlags.CameraPanning) && 
                       !ContextManager.GetFlag(GeneralContextFlags.TileMapManagerLoading) &&
                       !PlayerParty.CheckPartyMemberWillBeUnloaded())
                    {
                        ContextManager.SetFlag(GeneralContextFlags.DisallowCameraMovement, true);
                        Task.Run(() =>
                        {
                            TileMapManager.SetCenter(cameraPos);
                            TileMapManager.LoadMapsAroundCenter();
                            maps = TileMapManager.GetTileMapsInDiameter(cameraPos, 5);

                            SyncToRender(() =>
                            {
                                TileMapManager.SetVisibleMaps(maps);
                                ContextManager.SetFlag(GeneralContextFlags.DisallowCameraMovement, false);
                            });
                        });
                    }
                    else
                    {
                        SyncToRender(() =>
                        {
                            TileMapManager.SetVisibleMaps(maps);
                        });
                    }
                }
            };


            //_tileMapController.LoadSurroundingTileMaps(new TileMapPoint(0, 0));

            TileMapManager.LoadMapsAroundCenter();


            UnitTeam.PlayerUnits.SetRelation(UnitTeam.Skeletons, Relation.Hostile);
            UnitTeam.PlayerUnits.SetRelation(UnitTeam.PlayerUnits, Relation.Friendly);
            UnitTeam.PlayerUnits.SetRelation(UnitTeam.BadGuys, Relation.Neutral);

            QuestLog = new QuestLog(this);
            AbilityTreeUI = new AbilityTreeUI(this);
            InventoryUI = new InventoryUI(this);
            EquipmentUI = new EquipmentUI(this);

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
            AddUI(Footer, 100);

            OnNumberPressed += (args) =>
            {
                if(Footer.CurrentUnit == null) 
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


            //GameObject tent1 = new GameObject();
            //tent1.AddBaseObject(_3DObjects.CreateBaseObject(new SpritesheetObject(0, Textures.TentTexture), _3DObjects.TilePillar, default));

            //tent1.SetPosition(new Vector3(0, 200, 0f));

            //tent1.BaseObject.BaseFrame.SetScale(1.55f, 1.55f, 1);
            //_genericObjects.Add(tent1);


            RenderingConstants.LightPosition = new Vector3(50000, 0, 100);

            EnvironmentColor.OnChangeEvent += (color, _) =>
            {
                RenderingConstants.LightColor = EnvironmentColor.ToVector();
            };

            EnvironmentColor._onChange();


            ShowEnergyDisplayBars(false);
            Footer.EndTurnButton.SetRender(false);
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
                        DeselectUnits();
                        CurrentUnit.Select();
                    }
                    break;
                case Keys.E:
                    if (Footer.EndTurnButton != null && Footer.EndTurnButton.Render)
                    {
                        Footer.EndTurnButton.OnClick();
                    }
                    break;
                case Keys.KeyPad7:
                    _camera.RotateByAmount(-1);
                    break;
                case Keys.KeyPad8:
                    _camera.RotateByAmount(1);
                    break;
                case Keys.KeyPad4:
                    _camera.RotateByAmount(verticalStep: 1);
                    break;
                case Keys.KeyPad5:
                    _camera.RotateByAmount(verticalStep: -1);
                    break;
                case Keys.J:
                    if (QuestLog.Displayed)
                    {
                        QuestLog.RemoveWindow();
                    }
                    else
                    {
                        QuestLog.CreateWindow();
                    }
                    break;
                case Keys.K:
                    if (AbilityTreeUI.Displayed)
                    {
                        AbilityTreeUI.RemoveWindow();
                    }
                    else
                    {
                        AbilityTreeUI.CreateWindow();
                    }
                    break;
                case Keys.I:
                    if (InventoryUI.Displayed)
                    {
                        InventoryUI.RemoveWindow();
                    }
                    else
                    {
                        InventoryUI.CreateWindow();
                    }
                    break;
                case Keys.O:
                    if (EquipmentUI.Displayed)
                    {
                        EquipmentUI.RemoveWindow();
                    }
                    else
                    {
                        EquipmentUI.CreateWindow();
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

            float _cameraSpeed = 16.0f;
            float _zoomSpeed = 1.0f;

            if (!GetBit(_interceptKeystrokes, ObjectType.All) && _focusedObj == null)
            {
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

                if (!ContextManager.GetFlag(GeneralContextFlags.CameraPanning) && !ContextManager.GetFlag(GeneralContextFlags.DisallowCameraMovement) 
                    && TileMapsFocused)
                {
                    Vector3 camDelta = new Vector3();

                    if (KeyboardState.IsKeyDown(Keys.W))
                    {
                        camDelta += Vector3.NormalizeFast(new Vector3(_camera.Front.X, _camera.Front.Y, 0)) * _cameraSpeed * (float)args.Time;

                        //_camera.SetPosition(_camera.Position + Vector3.UnitY * _cameraSpeed * (float)args.Time);
                        //_camera.SetPosition(_camera.Position + Vector3.NormalizeFast(new Vector3(_camera.Front.X, _camera.Front.Y, 0)) * _cameraSpeed * (float)args.Time);
                        //OnMouseMove();
                        //OnCameraMoved();
                    }

                    if (KeyboardState.IsKeyDown(Keys.S))
                    {
                        camDelta -= Vector3.NormalizeFast(new Vector3(_camera.Front.X, _camera.Front.Y, 0)) * _cameraSpeed * (float)args.Time;

                        //_camera.Position -= _camera.Front * cameraSpeed * (float)args.Time; // Backwards
                        //_camera.Position -= _camera.Up * cameraSpeed * (float)args.Time; // Down
                        //_camera.SetPosition(_camera.Position - Vector3.UnitY * _cameraSpeed * (float)args.Time);
                        //_camera.SetPosition(_camera.Position - Vector3.NormalizeFast(new Vector3(_camera.Front.X, _camera.Front.Y, 0)) * _cameraSpeed * (float)args.Time);
                        //OnMouseMove();
                        //OnCameraMoved();
                    }
                    if (KeyboardState.IsKeyDown(Keys.A))
                    {
                        camDelta += Vector3.NormalizeFast(new Vector3(-_camera.Front.Y, _camera.Front.X, 0)) * _cameraSpeed * (float)args.Time;

                        //_camera.Position -= _camera.Right * _cameraSpeed * (float)args.Time; // Left
                        //_camera.SetPosition(_camera.Position - _camera.Right * _cameraSpeed * (float)args.Time);
                        //_camera.SetPosition(_camera.Position - Vector3.UnitX * _cameraSpeed * (float)args.Time);
                        //_camera.SetPosition(_camera.Position + Vector3.NormalizeFast(new Vector3(-_camera.Front.Y, _camera.Front.X, 0)) * _cameraSpeed * (float)args.Time);
                        //OnMouseMove();
                        //OnCameraMoved();
                    }
                    if (KeyboardState.IsKeyDown(Keys.D))
                    {
                        camDelta -= Vector3.NormalizeFast(new Vector3(-_camera.Front.Y, _camera.Front.X, 0)) * _cameraSpeed * (float)args.Time;

                        //_camera.Position += _camera.Right * _cameraSpeed * (float)args.Time; // Right
                        //_camera.SetPosition(_camera.Position + _camera.Right * _cameraSpeed * (float)args.Time);
                        //_camera.SetPosition(_camera.Position + Vector3.UnitX * _cameraSpeed * (float)args.Time);
                        //_camera.SetPosition(_camera.Position - Vector3.NormalizeFast(new Vector3(-_camera.Front.Y, _camera.Front.X, 0)) * _cameraSpeed * (float)args.Time);
                        //OnMouseMove();
                        //OnCameraMoved();
                    }
                    if (KeyboardState.IsKeyDown(Keys.Space))
                    {
                        //_camera.Position += _camera.Up * cameraSpeed * (float)args.Time; // Up
                    }

                    if(camDelta.X != 0 || camDelta.Y != 0)
                    {
                        _camera.SetPosition(_camera.Position + camDelta);
                    }
                }
            }
        }


        private int _counter = 0;
        private List<Vector3i> _cubeCoordinates = new List<Vector3i>();
        public override void OnTileClicked(TileMap map, BaseTile tile, MouseButton button, ContextManager<MouseUpFlags> flags)
        {
            base.OnTileClicked(map, tile, button, flags);

            var units = UnitPositionManager.GetUnitsOnTilePoint(tile);
            if (units.Count > 0)
            {
                foreach(var unit in units)
                {
                    OnUnitClicked(unit, button);
                }
            }
            else if (button == MouseButton.Left)
            {
                if (_selectedAbility != null)
                {
                    Task.Run(() =>
                    {
                        _selectedAbility.OnTileClicked(map, tile);
                    });
                }
                else
                {
                    Sound sound = new Sound(Sounds.Select) { Gain = 0.15f, Pitch = GlobalRandom.NextFloat(0.6f, 0.6f) };
                    sound.Play();

                    DeselectUnits();

                    if (KeyboardState.IsKeyDown(Keys.LeftAlt))
                    {

                    }
                    else if (KeyboardState.IsKeyDown(Keys.RightAlt))
                    {
                        TileMapManager.ActiveMaps.ForEach(m => m.PopulateFeatures());
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
                        List<BaseTile> neighborList = new List<BaseTile>();

                        neighborList.Clear();
                        map.GetNeighboringTiles(tile, neighborList, shuffle: false);

                        bool createPillar = false;

                        for (int i = 0; i < neighborList.Count; i++)
                        {
                            if (tile.Properties.Height > neighborList[i].Properties.Height)
                            {
                                createPillar = true;
                                break;
                            }
                        }

                        if (createPillar)
                        {

                        }
                    }
                    else if (KeyboardState.IsKeyDown(Keys.F))
                    {
                        List<BaseTile> tileList = new List<BaseTile>();

                        tile.TileMap.GetRingOfTiles(tile, tileList, 20);

                        List<List<Direction>> digitalLines = new List<List<Direction>>();

                        for (int i = 0; i < tileList.Count / 6; i++)
                        {
                            var line = tile.TileMap.GetLineOfTiles(tile, tileList[i]);

                            List<Direction> digitalLine = new List<Direction>();

                            BaseTile prev = null;

                            foreach (var t in line)
                            {
                                if (prev != null)
                                {
                                    digitalLine.Add(FeatureEquation.DirectionBetweenTiles(prev, t));
                                }

                                prev = t;

                                //t.SetColor(_Colors.Blue + new Vector4(0.1f * i, 0, 0, 0));
                                //t.Update();
                            }
                            digitalLines.Add(digitalLine);

                            foreach (var dir in digitalLine)
                            {
                                //Console.WriteLine(dir.ToString());
                            }
                        }

                        for (int i = 0; i < 6; i++)
                        {
                            foreach (var line in digitalLines)
                            {
                                BaseTile currentTile = tile;

                                foreach (var dir in line)
                                {
                                    currentTile = tile.TileMap.GetNeighboringTile(currentTile, (Direction)((int)(dir + i) % 6));

                                    currentTile.SetColor(_Colors.Red + new Vector4(0, 0, 0.15f * i, 0));
                                    currentTile.Update();
                                }
                            }
                        }
                    }
                    else if (KeyboardState.IsKeyDown(Keys.G))
                    {
                        LightObstructions.Add(new LightObstruction(tile));

                        tile.SetColor(_Colors.Blue);
                        tile.Update();
                    }
                    else if (KeyboardState.IsKeyDown(Keys.H))
                    {
                        tile.Properties.Height--;
                    }
                    else if (KeyboardState.IsKeyDown(Keys.J))
                    {
                        tile.Properties.Height++;
                    }
                    else if (KeyboardState.IsKeyDown(Keys.KeyPad2))
                    {
                        Task.Run(() =>
                        {
                            EventLog.AddEvent("Super very very very very long text\nmeant to hopefully cause whatever bug that has been\noccurring to occur");
                        });
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
                    else if (KeyboardState.IsKeyDown(Keys.Z))
                    {
                        PlayerParty.InitializeParty();
                    }
                    else if (KeyboardState.IsKeyDown(Keys.X))
                    {
                        PlayerParty.PlaceUnits(tile);
                        PlayerParty.ExitCombat();
                    }
                    else if (KeyboardState.IsKeyDown(Keys.C))
                    {
                        PlayerParty.GroupUnits(CurrentUnit);
                    }
                    else if (KeyboardState.IsKeyDown(Keys.V))
                    {
                        PlayerParty.UngroupUnits(PlayerParty.PrimaryUnit.Info.TileMapPosition, false);
                    }
                    else if (KeyboardState.IsKeyDown(Keys.B))
                    {
                        Dictionary<int, UIObject> textureHandles = new Dictionary<int, UIObject>();

                        void handleObject(UIObject obj)
                        {
                            foreach (var item in obj.Children)
                            {
                                handleObject(item);
                            }

                            foreach (var item in obj.BaseObjects)
                            {
                                if (item._currentAnimation.CurrentFrame.Material.Diffuse == null)
                                    continue;

                                int texId = item._currentAnimation.CurrentFrame.Material.Diffuse.Handle;
                                int texName = item._currentAnimation.CurrentFrame.Textures.TextureIds[0];

                                if (textureHandles.ContainsKey(texId) && texName < 0)
                                {

                                }
                                else
                                {
                                    textureHandles.TryAdd(texId, obj);
                                }
                            }
                        }

                        foreach (var item in UIManager.TopLevelObjects)
                        {
                            handleObject(item);
                        }
                    }
                    else if (KeyboardState.IsKeyDown(Keys.Comma))
                    {
                        UIManager.RegenerateRenderData();
                    }
                    else if (KeyboardState.IsKeyDown(Keys.N))
                    {
                        var item = ItemManager.GetItemByID(1);
                        PlayerParty.Inventory.AddItemToInventory(item);

                        //CurrentUnit.Info.Equipment.EquipItem(item, EquipmentSlot.Weapon_1);
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

                        _cubeCoordinates.Clear();
                    }
                    else if (ContextManager.GetFlag(GeneralContextFlags.PatternToolEnabled))
                    {
                        Vector3i cubeCoords = CubeMethods.OffsetToCube(new FeaturePoint(tile));
                        _cubeCoordinates.Add(cubeCoords);

                        tile.SetColor(_Colors.Red);
                        tile.Update();
                    }
                    else if (KeyboardState.IsKeyDown(Keys.F9))
                    {
                        //List<Unit> units = new List<Unit>();

                        //units.Add(_units.Find(u => u.Name == "Guy"));
                        //units.Add(_units.Find(u => u.Name == "Frend"));

                        //Quest activeQuest = QuestManager.Quests.Find(q => q.ID == 50);

                        //if (activeQuest != null)
                        //{
                        //    DialogueWindow.StartDialogue(DialogueSerializer.LoadDialogueFromFile(2), units);
                        //}
                        //else
                        //{
                        //    DialogueWindow.StartDialogue(DialogueSerializer.LoadDialogueFromFile(0), units);
                        //}

                        var test = from t in Assembly.GetExecutingAssembly().GetTypes()
                                   where t.IsClass && t.Namespace == "MortalDungeon.Definitions.EventActions" && !t.IsSealed
                                   select t;

                        var list = test.ToList();

                        foreach (var t in list)
                        {
                            Console.WriteLine(t.Name);
                        }
                    }
                }
            }
            else
            {
                //if (_selectedUnits.Count != 0)
                //{
                //    DeselectUnits();
                //}
                //else 
                //{
                    if (ContextManager.GetFlag(GeneralContextFlags.PatternToolEnabled))
                    {
                        Vector3i cubeCoords = CubeMethods.OffsetToCube(new FeaturePoint(tile));
                        _cubeCoordinates.Remove(cubeCoords);

                        tile.Color = _Colors.White;
                        tile.Update();

                        return;
                    }

                    tile.OnRightClick(flags);
                //}
            }
        }

        private TilePoint _wallTemp = null;
        private int temp = 0;
    }
}
