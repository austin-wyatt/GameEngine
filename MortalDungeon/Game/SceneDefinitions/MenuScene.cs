using Empyrean.Engine_Classes;
using Empyrean.Engine_Classes.MiscOperations;
using Empyrean.Game.Abilities;
using Empyrean.Game.Objects;
using Empyrean.Game.Units;
using Empyrean.Objects;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using Empyrean.Game.UI;
using Empyrean.Engine_Classes.Scenes;
using Empyrean.Game.Tiles;
using Empyrean.Engine_Classes.UIComponents;
using System.Linq;
using Empyrean.Game.Tiles.TileMaps;
using Empyrean.Game.Map;
using Empyrean.Game.Structures;
using Empyrean.Game.Objects.PropertyAnimations;
using Empyrean.Engine_Classes.Audio;
using Empyrean.Game.Entities;
using Empyrean.Game.UI.Dev;
using Empyrean.Engine_Classes.Rendering;
using System.Xml.Serialization;
using System.IO;
using Empyrean.Game.Save;
using Empyrean.Game.Serializers;
using System.Drawing;
using System.Threading.Tasks;
using Empyrean.Game.Player;
using Empyrean.Game.Items;
using Empyrean.Game.Events;
using System.Reflection;
using Empyrean.Game.Combat;
using System.Diagnostics;
using Empyrean.Game.Tiles.Meshes;
using Empyrean.Definitions.BlendControls;

namespace Empyrean.Game.SceneDefinitions
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

            GlobalInfo.LoadDefaultGlobalInfo();

            DataObjects.DataManagerInitializer.Initialize("default_save");

            _camera.RotateByAmount(0);

            UIBlock inputCapture = new UIBlock(WindowConstants.CenterScreen, new UIScale(10, 10));
            inputCapture.Scrollable = true;

            inputCapture.SetColor(_Colors.Transparent);

            inputCapture.Scroll += (s, e) =>
            {
                if (!ContextManager.GetFlag(GeneralContextFlags.DisallowCameraMovement) && TileMapsFocused && !KeyboardState.IsKeyDown(Keys.LeftControl))
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


            _cameraPos = TileMapHelpers.GlobalPositionToMapPoint(_camera.Position);


            //origin campsite is at ID 4
            FeaturePoint campsiteOrigin = GlobalInfo.GetPOI(4).Origin;

            TileMapManager.SetCenter(campsiteOrigin.ToTileMapPoint());
            //_tileMapController.LoadSurroundingTileMaps(new TileMapPoint(0, 0));
            TileMapManager.LoadMapsAroundCenter();

            PlayerParty.InitializeParty();
            Tile tile = TileMapHelpers.GetTile(campsiteOrigin);
            if (tile != null)
            {
                _camera.SetPosition(WindowConstants.ConvertGlobalToLocalCoordinates(tile._position));
                PlayerParty.PlaceUnits(tile);
            }

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

            //OnNumberPressed += (args) =>
            //{
            //    if(Footer.CurrentUnit == null) 
            //    {
            //        Unit unit = _units.Find(u => u.AI.Team == UnitTeam.PlayerUnits);

            //        if(unit != null) 
            //        {
            //            Footer.UpdateFooterInfo(unit);
            //        }
            //    }
            //};


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
                case Keys.KeyPad1:
                   
                    if (ContextManager.GetFlag(GeneralContextFlags.EditingFeature))
                    {
                        ContextManager.SetFlag(GeneralContextFlags.EditingFeature, false);
                    }
                    else
                    {
                        ContextManager.SetFlag(GeneralContextFlags.EditingFeature, true);
                    }
                    break;
                case Keys.Y:
                    Console.WriteLine($"Gen 0: {GC.CollectionCount(0)} Gen 1: {GC.CollectionCount(1)} Gen 2: {GC.CollectionCount(2)}");

                    //int color = 15;

                    //Console.WriteLine(BlendControl.GetCombinedColor(PaletteLocation.Red2, 255, color));
                    break;
                case Keys.U:
                    double averageTime = 0;

                    for(int i = 0; i < Window._renderDiagnostics.Count; i++)
                    {
                        averageTime += Window._renderDiagnostics[i].TimeInRender;
                    }

                    averageTime /= Window._renderDiagnostics.Count;

                    for(int i = 0; i < Window._renderDiagnostics.Count; i++)
                    {
                        if(Window._renderDiagnostics[i].TimeInRender > averageTime * 1.5f)
                        {
                            Console.WriteLine($"Above average render time {Window._renderDiagnostics[i].TimeInRender}s at " +
                                $"{Window._renderDiagnostics[i].Timestamp}" +
                                $" Running slowly: {Window._renderDiagnostics[i].RunningSlowly}");
                        }
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
                        //even out the amount moved if we're moving diagonally
                        if(camDelta.X != 0 && camDelta.Y != 0)
                        {
                            camDelta.X *= 0.707106f; //cos(45)
                            camDelta.Y *= 0.707106f; //sin(45)
                        }
                        _camera.SetPosition(_camera.Position + camDelta);
                    }
                }
            }
        }

        private HashSet<Cube> _floodFillSet = new HashSet<Cube>();
        private IndividualMesh TEMP_MESH;

        private int _counter = 0;
        private List<Vector3i> _cubeCoordinates = new List<Vector3i>();
        public override void OnTileClicked(TileMap map, Tile tile, MouseButton button, ContextManager<MouseUpFlags> flags)
        {
            base.OnTileClicked(map, tile, button, flags);

            var units = UnitPositionManager.GetUnitsOnTilePoint(tile);
            if (units.Count > 0)
            {
                if (KeyboardState.IsKeyDown(Keys.Period))
                {
                    var timer = Stopwatch.StartNew();

                    HashSet<NavTileWithParent> floodFillSet = new HashSet<NavTileWithParent>();
                    TileMapManager.NavMesh.NavFloodFill(tile.ToFeaturePoint(), NavType.Base, ref floodFillSet,
                        units.First().Info._movementAbility.Range, units.First());

                    Console.WriteLine("Flood fill completed in " + timer.Elapsed.TotalMilliseconds + "ms");

                    float range = units.First().Info._movementAbility.Range;

                    foreach (var floodTile in floodFillSet)
                    {
                        floodTile.NavTile.Tile.SetColor(new Vector4(1, floodTile.PathCost / range, 0, 1));
                    }

                    for(int i = 0; i < 1; i++)
                    {
                        var tempTile = floodFillSet.ElementAt(GlobalRandom.Next(floodFillSet.Count - 1));
                        while (tempTile.Parent != null)
                        {
                            tempTile.NavTile.Tile.SetColor(new Vector4(tempTile.PathCost / range, 1, 0, 1));
                            tempTile = tempTile.Parent;
                        }
                    }
                }
                else if (KeyboardState.IsKeyDown(Keys.Backslash))
                {
                    Unit unit = units.First();
                    VisionManager.CalculateVisionForUnit(unit);
                }
                else if (KeyboardState.IsKeyDown(Keys.Slash))
                {
                    Unit unit = units.First();
                    Stopwatch timer = Stopwatch.StartNew();

                    CombatState.CalculateUnimpededLinesToUnit(unit).Wait();

                    if(CombatState.UnimpededUnitSightlines.TryGetValue(unit, out var lineOfTiles))
                    {
                        foreach(var line in lineOfTiles)
                        {
                            for(int i = 0; i < line.Tiles.Count; i++)
                            {
                                if(i >= line.AbilityLineHeightIndex)
                                {
                                    line.Tiles[i].SetColor(new Vector4((float)i / line.Tiles.Count, 0, 1, 1));
                                }
                            }
                        }
                    }


                    Console.WriteLine("Ring calculated in " + timer.Elapsed.TotalMilliseconds + "ms");
                }

                foreach (var unit in units)
                {
                    OnUnitClicked(unit, button);
                }
            }
            else if (button == MouseButton.Left)
            {
                if (_selectedAbility != null && !AbilityInProgress)
                {
                    Ability ability = _selectedAbility;
                    Task.Run(() =>
                    {
                        ability.OnTileClicked(map, tile);
                    });
                }
                else
                {
                    Sound sound = new Sound(Sounds.Select) { Gain = 0.15f, Pitch = GlobalRandom.NextFloat(0.6f, 0.6f) };
                    sound.Play();

                    if (KeyboardState.IsKeyDown(Keys.H))
                    {
                        if(_selectedUnits.Count > 0)
                        {
                            List<Vector3i> line = new List<Vector3i>();

                            CubeMethods.GetLineLerp(CubeMethods.OffsetToCube(_selectedUnits[0].Info.TileMapPosition.ToFeaturePoint()), 
                                CubeMethods.OffsetToCube(tile.ToFeaturePoint()), line);

                            foreach(var cube in line)
                            {
                                FeaturePoint point = CubeMethods.CubeToFeaturePoint(cube);

                                Tile foundTile = TileMapHelpers.GetTile(point);

                                if(foundTile != null)
                                {
                                    foundTile.SetColor(_Colors.LessAggressiveRed);
                                }
                            }
                        }
                    }

                    if (KeyboardState.IsKeyDown(Keys.P))
                    {
                        PlayerParty.PlaceUnits(tile);
                    }
                    else if (KeyboardState.IsKeyDown(Keys.LeftShift))
                    {
                        List<Tile> neighborList = new List<Tile>();

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
                        var tilePosA = CurrentUnit.Info.TileMapPosition.Position;
                        var tilePosB = tile.Position;

                        var hypotenuse = Vector2.Distance(tilePosA.Xy, tilePosB.Xy);

                        var sideA = Vector2.Distance(new Vector2(tilePosA.X, tilePosB.Y), tilePosA.Xy);

                        //var angle = Vector3.CalculateAngle(nodeA.obj.Position, nodeB.obj.Position);
                        float angle = (float)MathHelper.Asin(sideA / hypotenuse);

                        UIBlock line = new UIBlock(scaleAspectRatio: false, cameraPerspective: true);

                        int sign = ((tilePosA.X > tilePosB.X) ? 1 : -1) * ((tilePosA.Y > tilePosB.Y) ? -1 : 1);

                        line.BaseObject.BaseFrame.RotateZ(MathHelper.RadiansToDegrees(angle) * sign);

                        line.SetPosition((tilePosA + tilePosB) / 2);

                        line.SetPosition(line.Position + new Vector3(0, 0, 0.1f));

                        float yVal = 0.05f * (float)Math.Abs((Math.PI - angle) / Math.PI);
                        float xVal = hypotenuse / WindowConstants.ScreenUnits.X * 2;

                        line.SetSize(new UIScale(xVal, yVal));

                        line.SetAllInline(0);
                        line.SetColor(_Colors.Black);
                        line.BaseObject.EnableLighting = true;

                        _genericObjects.Add(line);

                        _tileMapController.DeselectTiles();
                        var tiles = tile.TileMap.GetLineOfTiles(CurrentUnit.Info.TileMapPosition, tile);
                        _tileMapController.SelectTiles(tiles);
                    }
                    else if (KeyboardState.IsKeyDown(Keys.G))
                    {
                        if (CurrentUnit == null)
                            return;

                        _tileMapController.DeselectTiles(TileSelectionType.Stone_2);

                        if (TileMapManager.NavMesh.GetPathToPoint(CurrentUnit.Info.TileMapPosition.ToFeaturePoint(), 
                            tile.ToFeaturePoint(), NavType.Base, out var list, pathingUnit: CurrentUnit))
                        {
                            foreach(var pathTile in list)
                            {
                                _tileMapController.SelectTile(pathTile, TileSelectionType.Stone_2);
                            }
                        }
                    }
                    else if (KeyboardState.IsKeyDown(Keys.J))
                    {
                        TileMapManager.NavMesh.CalculateNavTiles();
                    }
                    else if (KeyboardState.IsKeyDown(Keys.KeyPad2))
                    {
                        Console.WriteLine(FeatureEquation.GetDistanceBetweenPoints(CurrentUnit.Info.TileMapPosition.ToFeaturePoint(), tile.ToFeaturePoint()));
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
                                TraversableTypes = new List<TileClassification>() { TileClassification.Ground, TileClassification.ImpassableGround, TileClassification.Water }
                            };

                            List<Tile> tiles = map.GetPathToPoint(param);

                            //map.GetRingOfTiles(_wallTemp, tiles, 10);

                            //tiles.Insert(tiles.Count, tiles[0]);
                            //tiles.Remove(tiles[0]);
                            //tiles.Insert(tiles.Count, tiles[0]);
                            //tiles.Remove(tiles[0]);

                            Wall.CreateWalls(map, tiles, Wall.WallMaterial.Iron);

                            (List<Wall> walls, bool circular) = Wall.FindAdjacentWalls(_wallTemp.ParentTileMap.GetTile(_wallTemp).Structure as Wall);
                            Wall.UnifyWalls(walls, circular);

                            walls.ForEach(w => w.Name = "fence");

                            //Console.WriteLine(VisionMap.TargetInVision(_wallTemp, tile.TilePoint, 10, this));

                            _wallTemp = null;
                        }
                    }
                    else if (KeyboardState.IsKeyDown(Keys.LeftControl))
                    {
                        tile.SetHeight(tile.Properties.Height + 1);
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
                        IndividualMesh mesh = new IndividualMesh();

                        List<Tile> tileList = tile.TileMap.GetTilesInRadius(tile, 1);

                        mesh.FillFromTiles(tileList, quadTexCoords: true);

                        mesh.Texture = new SimpleTexture("Resources/Textures/Spiderweb.png", 50000) { WrapType = TextureWrapType.ClampToEdge };
                        mesh.LoadTexture();

                        Vector3 pos = WindowConstants.ConvertGlobalToLocalCoordinates(tile._position);
                        pos.Z += 0.01f;
                        mesh.SetTranslation(pos);

                        IndividualMeshes.Add(mesh);

                        TEMP_MESH = mesh;
                    }
                    else if (KeyboardState.IsKeyDown(Keys.V))
                    {
                        //TEMP_MESH.TextureTransformations.TranslateBy(new Vector2(0.1f, 0.1f));
                        //TEMP_MESH.TextureTransformations.SetShear(new Vector2(0f, 0));
                        //TEMP_MESH.TextureTransformations.SetScale(new Vector2(1f, 1f));
                        //TEMP_MESH.TextureTransformations.RotateBy(0.1f);
                        TEMP_MESH.TextureTransformations.ScaleBy(new Vector2(0.9f, 0.9f), new Vector2(0.5f, 0.5f));
                        //TEMP_MESH.TextureTransformations.SetTranslation(new Vector2(-0.5f, -0.5f));
                        //TEMP_MESH.TextureTransformations.SetScale(new Vector2(2.5f, 2.5f));

                        //Console.WriteLine(TEMP_MESH.TextureTransformations.Transformations * new Vector3(1, 1, 1));
                    }
                    else if (KeyboardState.IsKeyDown(Keys.B))
                    {
                        TEMP_MESH.TextureTransformations.TranslateBy(new Vector2(0, -0.1f));
                    }
                    else if (KeyboardState.IsKeyDown(Keys.Comma))
                    {
                        Conditional test = new Conditional();
                        test.Condition = "(Quest 103 completed OR Quest 103 inProgress)";
                        test.PrepareForSerialization();

                        Console.WriteLine(test.Check());
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

                        EventLog.AddEvent(text, severity);
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
                    }
                    else if (KeyboardState.IsKeyDown(Keys.F9))
                    {
                        //var test = from t in Assembly.GetExecutingAssembly().GetTypes()
                        //           where t.IsClass && t.Namespace == "Empyrean.Definitions.EventActions" && !t.IsSealed
                        //           select t;

                        //var list = test.ToList();

                        //foreach (var t in list)
                        //{
                        //    Console.WriteLine(t.Name);
                        //}

                        Console.WriteLine($"Tile: {tile.ToFeaturePoint().ToString()}");
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

                        tile.SetColor(_Colors.White);

                        return;
                    }

                    tile.OnRightClick(flags);
                //}
            }
        }


        private bool _updateUnitStatusBars = false;
        public override void OnRenderEnd()
        {
            base.OnRenderEnd();

            //Anything that needs to be predictably queued to the render cycle should be done in this method
            //otherwise we create a large amount of Action objects by calling the RenderDispatcher
            if (_updateUnitStatusBars)
            {
                UpdateUnitStatusBars();
                _updateUnitStatusBars = false;
            }
        }


        private TileMapPoint _cameraPos;

        public override void OnCameraMoved()
        {
            base.OnCameraMoved();

            _updateUnitStatusBars = true;

            if (BoxSelectHelper.BoxSelecting)
            {
                BoxSelectHelper.DrawSelectionBox();
            }

            //TileMapPoint newPos = TileMapHelpers.GlobalPositionToMapPoint(_camera.Position);
            bool inNewPos = TileMapHelpers.TestCameraTileMapPosition(_cameraPos, _camera.Position);

            if (inNewPos)
            {
                TileMapPoint newPos = TileMapHelpers.GlobalPositionToMapPoint(_camera.Position);

                //ContextManager.SetFlag(GeneralContextFlags.DisallowCameraMovement, true);
                _cameraPos = newPos;

                var maps = TileMapManager.GetTileMapsInDiameter(_cameraPos, 5);

                if (maps.Count < 5 * 5 && !InCombat &&
                   !ContextManager.GetFlag(GeneralContextFlags.CameraPanning) &&
                   !ContextManager.GetFlag(GeneralContextFlags.TileMapManagerLoading) &&
                   !PlayerParty.CheckPartyMemberWillBeUnloaded())
                {
                    maps.Clear();
                    TileMap.TileMapListPool.FreeObject(ref maps);

                    ContextManager.SetFlag(GeneralContextFlags.DisallowCameraMovement, true);
                    Task.Run(() =>
                    {
                        TileMapManager.SetCenter(_cameraPos);
                        TileMapManager.LoadMapsAroundCenter();
                        maps = TileMapManager.GetTileMapsInDiameter(_cameraPos, 5);

                        //do a synchronous merge + normals pass on visible maps here
                        //other maps should be queued/calculated asynchronously unless they 
                        //are made visible (in which case they should be prioritized and finished synchronously asap)
                        Window.QueueToRenderCycle(() =>
                        {
                            TileMapManager.SetVisibleMaps(maps);
                            ContextManager.SetFlag(GeneralContextFlags.DisallowCameraMovement, false);

                            maps.Clear();
                            TileMap.TileMapListPool.FreeObject(ref maps);
                        });
                    });
                }
                else
                {
                    Window.QueueToRenderCycle(() =>
                    {
                        TileMapManager.SetVisibleMaps(maps);

                        maps.Clear();
                        TileMap.TileMapListPool.FreeObject(ref maps);
                    });
                }
            }
        }

        private TilePoint _wallTemp = null;
        private int temp = 0;
    }
}
