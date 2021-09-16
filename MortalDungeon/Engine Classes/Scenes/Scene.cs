using MortalDungeon.Engine_Classes.MiscOperations;
using MortalDungeon.Game.Lighting;
using MortalDungeon.Game.Objects.PropertyAnimations;
using MortalDungeon.Game.Tiles;
using MortalDungeon.Game.Units;
using MortalDungeon.Objects;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static MortalDungeon.Engine_Classes.Scenes.CombatScene;

namespace MortalDungeon.Engine_Classes.Scenes
{
    public enum ObjectType //corresponds to their bit position in the MessageTarget enum
    {
        UI = 0,
        Unit = 1,
        Tile = 2,
        Text = 3,
        GenericObject = 4,
        LowPriorityObject = 5,
        All = 7
    }

    public enum EventAction 
    {
        OneKeyDown,
        TwoKeyDown,
        ThreeKeyDown,
        FourKeyDown,
        FiveKeyDown,
        SixKeyDown,
        SevenKeyDown,
        EightKeyDown,

        CloseTooltip
    }

    public class SceneEventArgs
    {
        public Scene Scene;
        public EventAction EventAction;
        public SceneEventArgs(Scene scene, EventAction action) 
        {
            Scene = scene;
            EventAction = action;
        }
    }

    public class Scene
    {
        public SceneController Controller;

        public QueuedObjectList<GameObject> _genericObjects = new QueuedObjectList<GameObject>(); //GameObjects that are not Units and are being rendered independently
        public List<Text> _text = new List<Text>();
        public TileMapController _tileMapController = new TileMapController();
        public QueuedObjectList<Unit> _units = new QueuedObjectList<Unit>(); //The units to render
        public QueuedUIList<UIObject> _UI = new QueuedUIList<UIObject>();
        public QueuedList<ParticleGenerator> _particleGenerators = new QueuedList<ParticleGenerator>();

        public QueuedObjectList<GameObject> _lowPriorityObjects = new QueuedObjectList<GameObject>(); //the last objects that will be rendered in the scene

        public HashSet<UnitTeam> ActiveTeams = new HashSet<UnitTeam>();

        public List<Unit> _collatedUnits = new List<Unit>();
        private HashSet<Unit> _renderedUnits = new HashSet<Unit>();

        public static GameObject LightObstructionObject = null;
        public static GameObject LightObject = null;
        //public static List<GameObject> LightObjects = new List<GameObject>();
        public static bool RenderLight = true;


        public List<ITickable> TickableObjects = new List<ITickable>();

        public ContextManager<GeneralContextFlags> ContextManager = new ContextManager<GeneralContextFlags>();

        public Action ExitFunc = null; //function used to exit the application

        public bool Loaded = false;

        public Action PostTickAction;

        public int Priority = 0; //determines which scene will have their events evaluated first

        public int SceneID => _sceneID;
        protected int _sceneID = currentSceneID++;
        protected static int currentSceneID = 0;

        public MessageCenter MessageCenter = null;

        #region Messaging flags
        protected int _interceptClicks = 0b0; //see MessageTarget enum in SceneController for notable values

        protected int _disableRender = 0b0;

        protected int _interceptKeystrokes = 0b0;
        #endregion


        public Lighting Lighting;
        public QueuedList<LightObstruction> LightObstructions = new QueuedList<LightObstruction>();
        public QueuedList<LightGenerator> LightGenerators = new QueuedList<LightGenerator>();
        public QueuedList<VisionGenerator> UnitVisionGenerators = new QueuedList<VisionGenerator>();

        public Camera _camera;
        public BaseObject _cursorObject;
        public MouseRay _mouseRay;

        public Vector3 ScenePosition;

        public KeyboardState KeyboardState => Program.Window.KeyboardState;
        public MouseState MouseState => Program.Window.MouseState;

        protected Random rand = new Random();
        private Stopwatch _mouseTimer = new Stopwatch();
        protected Stopwatch _hoverTimer = new Stopwatch();
        protected GameObject _hoveredObject;

        protected virtual void InitializeFields()
        {
            _genericObjects = new QueuedObjectList<GameObject>();
            _text = new List<Text>();
            _units = new QueuedObjectList<Unit>(); //The units to render
            _UI = new QueuedUIList<UIObject>();
            _tileMapController = new TileMapController();

            MessageCenter = new MessageCenter(SceneID)
            {
                ParseMessage = ParseMessage
            };

            ScenePosition = new Vector3(0, 0, 0);

            Lighting = new Lighting(this);
        }


        public virtual void Load(Camera camera = null, BaseObject cursorObject = null, MouseRay mouseRay = null) //all object initialization should be handled here
        {
            Loaded = true;
            _camera = camera;
            _cursorObject = cursorObject;
            _mouseRay = mouseRay;

            _mouseTimer.Start();
        }

        public virtual void Unload()
        {
            InitializeFields();

            Loaded = false;
        }

        public void AddUI(UIObject ui, int zIndex = -1, bool immediate = true)
        {
            if (immediate)
            {
                _UI.AddImmediate(ui);
            }
            else
            {
                _UI.Add(ui);
            }

            if (zIndex != -1)
            {
                ui.ZIndex = zIndex;
            }
            else
            {
                ui.ZIndex = 0;
            }

            SortUIByZIndex();
        }

        public void RemoveUI(UIObject ui)
        {
            _UI.Remove(ui);
        }


        public void SortUIByZIndex()
        {
            _UI.Sort();
        }

        #region Messaging handlers
        public virtual void ParseMessage(Message msg)
        {
            //Console.WriteLine(msg.MessageType.ToString() + " from id " + msg.Sender + " to " + SceneID + ": " + msg.MessageBody.ToString() + " " + msg.MessageTarget.ToString());

            switch (msg.MessageBody)
            {
                case MessageBody.InterceptClicks:
                    InterceptClicks(true, msg.MessageTarget);
                    break;
                case MessageBody.EndClickInterception:
                    InterceptClicks(false, msg.MessageTarget);
                    break;
                case MessageBody.StartRendering:
                    DisableRendering(false, msg.MessageTarget);
                    break;
                case MessageBody.StopRendering:
                    DisableRendering(true, msg.MessageTarget);
                    break;
                case MessageBody.InterceptKeyStrokes:
                    InterceptKeystrokes(true, msg.MessageTarget);
                    break;
                case MessageBody.EndKeyStrokeInterception:
                    InterceptKeystrokes(false, msg.MessageTarget);
                    break;
            }

            OnMessageRecieved?.Invoke(msg);

            MessageCenter.SendMessage(msg.CreateAffirmativeResponse(SceneID));
        }
        public delegate void MessageEventHandler(Message msg);
        public event MessageEventHandler OnMessageRecieved;

        private void InterceptClicks(bool intercept, MessageTarget target)
        {
            if (intercept)
            {
                _interceptClicks = (int)target | _interceptClicks;
            }
            else
            {
                _interceptClicks &= ~(int)target;
            }
        }
        private void DisableRendering(bool disable, MessageTarget target)
        {
            if (disable)
            {
                _disableRender = (int)target | _disableRender;
            }
            else
            {
                _disableRender &= ~(int)target;
            }
        }
        private void InterceptKeystrokes(bool intercept, MessageTarget target)
        {
            if (intercept)
            {
                _interceptKeystrokes = (int)target | _interceptKeystrokes;
            }
            else
            {
                _interceptKeystrokes &= ~(int)target;
            }
        }
        #endregion


        private bool _updatingVisionMap = false;
        private bool _shouldRepeatVisionMap = false;
        public Task VisionMapTask = null;

        public void UpdateVisionMap(Action onFinish = null, UnitTeam teamToUpdate = UnitTeam.Unknown)
        {
            if (ContextManager.GetFlag(GeneralContextFlags.DisableVisionMapUpdate)) 
            {
                onFinish?.Invoke();
                return;
            }

            if (_updatingVisionMap) 
            {
                _shouldRepeatVisionMap = true;
                return;
            }

            _updatingVisionMap = true;

            VisionMapTask = new Task(() =>
            {
                _updatingVisionMap = true;

                VisionMap.SetObstructions(LightObstructions, this);

                VisionMap.CalculateVision(UnitVisionGenerators, this, teamToUpdate);

                _updatingVisionMap = false;

                if (_shouldRepeatVisionMap)
                {
                    _shouldRepeatVisionMap = false;

                    UpdateVisionMap(null, teamToUpdate);

                    onFinish?.Invoke();
                    OnVisionMapUpdated();
                }
                else 
                {
                    onFinish?.Invoke();
                    OnVisionMapUpdated();
                }
            });

            if(VisionMapTask.Status == TaskStatus.Created)
                VisionMapTask.Start();
        }


        public virtual void OnVisionMapUpdated() { }

        public void CalculateActiveTeams() 
        {
            ActiveTeams.Clear();
            foreach (var unit in _units) 
            {
                ActiveTeams.Add(unit.AI.Team);
            }
        }

        #region Event handlers

        public virtual void OnRender()
        {
            _genericObjects.HandleQueuedItems();
            _UI.HandleQueuedItems();
            if (_units.HasQueuedItems()) 
            {
                _units.HandleQueuedItems();
                CalculateActiveTeams();
            }

            _particleGenerators.HandleQueuedItems();
            _lowPriorityObjects.HandleQueuedItems();

            if (LightObstructions.HasQueuedItems())
            {
                LightObstructions.HandleQueuedItems();
                ContextManager.SetFlag(GeneralContextFlags.UpdateLightObstructionMap, true);
                UpdateVisionMap();
            }

            if (LightGenerators.HasQueuedItems())
            {
                LightGenerators.HandleQueuedItems();
                ContextManager.SetFlag(GeneralContextFlags.UpdateLighting, true);
            }

            if (UnitVisionGenerators.HasQueuedItems())
            {
                UnitVisionGenerators.HandleQueuedItems();
                UpdateVisionMap();
            }

            for (int i = 0; i < _tileMapController.TileMaps.Count; i++)
            {
                _tileMapController.TileMaps[i].SelectionTiles.HandleQueuedItems();
            }
        }

        //The reason behind this is to have a consistent state for all objects to make decisions based off of. 
        //Ie, it curtails the problem of setting a flag earlier in the call chain and then checking it later expecting the old value
        public enum MouseUpFlags
        {
            ClickProcessed,
            ContextMenuOpen,
            AbilitySelected,
            TabMenuOpen
        }

        protected ContextManager<MouseUpFlags> MouseUpStateFlags = new ContextManager<MouseUpFlags>();
        public virtual void OnMouseUp(MouseButtonEventArgs e)
        {
            SetMouseStateFlags();
            CheckMouseUp(e);
        }

        protected virtual void SetMouseStateFlags()
        {
            MouseUpStateFlags.SetFlag(MouseUpFlags.ClickProcessed, false);
        }

        protected virtual void CheckMouseUp(MouseButtonEventArgs e)
        {
            if ((e.Button == MouseButton.Left || e.Button == MouseButton.Right) && e.Action == InputAction.Release && !GetBit(_interceptClicks, ObjectType.All))
            {
                bool leftClick = e.Button == MouseButton.Left;

                Vector2 MouseCoordinates = NormalizeGlobalCoordinates(new Vector2(_cursorObject.Position.X, _cursorObject.Position.Y), WindowConstants.ClientSize);

                if (!GetBit(_interceptClicks, ObjectType.UI))
                    _UI.ForEach(uiObj =>
                    {
                        if (MouseUpStateFlags.GetFlag(MouseUpFlags.TabMenuOpen))
                        {
                            if (uiObj != TabMenu)
                            {
                                return;
                            }
                        }

                        if (uiObj.Render && !uiObj.Disabled)
                        {
                            uiObj.BoundsCheck(MouseCoordinates, _camera, (obj) =>
                            {
                                OnUIClicked(obj);
                                MouseUpStateFlags.SetFlag(MouseUpFlags.ClickProcessed, true);
                            }, leftClick ? UIEventType.Click : UIEventType.RightClick);
                        }
                    });

                if (MouseUpStateFlags.GetFlag(MouseUpFlags.ClickProcessed))
                    return; //stop further clicks from being processed


                #region UI MouseUpEvents
                if (MouseUpStateFlags.GetFlag(MouseUpFlags.ContextMenuOpen))
                {
                    ActOnMouseStateFlag(MouseUpFlags.ContextMenuOpen);
                }
                else if (MouseUpStateFlags.GetFlag(MouseUpFlags.TabMenuOpen))
                {
                    ActOnMouseStateFlag(MouseUpFlags.TabMenuOpen);
                }
                #endregion

                if (MouseUpStateFlags.GetFlag(MouseUpFlags.ClickProcessed))
                    return; //stop further clicks from being processed


                Vector3 mouseRayNear = _mouseRay.UnProject(_cursorObject.Position.X, _cursorObject.Position.Y, 0, _camera, WindowConstants.ClientSize); // start of ray (near plane)
                Vector3 mouseRayFar = _mouseRay.UnProject(_cursorObject.Position.X, _cursorObject.Position.Y, 1, _camera, WindowConstants.ClientSize); // end of ray (far plane)

                //if (!GetBit(_interceptClicks, ObjectType.Unit) && e.Button == MouseButton.Left)
                //    ObjectCursorBoundsCheck(_units, mouseRayNear, mouseRayFar).ForEach(foundObj =>
                //    {
                //        OnUnitClicked(foundObj, e.Button);
                //        MouseUpStateFlags.SetFlag(MouseUpFlags.ClickProcessed, true);
                //    });

                if (MouseUpStateFlags.GetFlag(MouseUpFlags.ClickProcessed))
                    return; //stop further clicks from being processed

                if (!GetBit(_interceptClicks, ObjectType.Tile))
                    for (int i = 0; i < _tileMapController.TileMaps.Count; i++)
                    {
                        if (!_tileMapController.TileMaps[i].Render)
                            continue;

                        for (int j = 0; !MouseUpStateFlags.GetFlag(MouseUpFlags.ClickProcessed) && j < _tileMapController.TileMaps[i].TileChunks.Count; j++)
                        {
                            if (!_tileMapController.TileMaps[i].TileChunks[j].Cull)
                            {
                                ObjectCursorBoundsCheck(_tileMapController.TileMaps[i].TileChunks[j].Tiles, mouseRayNear, mouseRayFar).ForEach(foundObj =>
                                {
                                    OnTileClicked(_tileMapController.TileMaps[i], foundObj, e.Button, MouseUpStateFlags);
                                    MouseUpStateFlags.SetFlag(MouseUpFlags.ClickProcessed, true);
                                });
                            }
                        }
                    }

                if (MouseUpStateFlags.GetFlag(MouseUpFlags.ClickProcessed))
                    return; //stop further clicks from being processed
            }
        }

        protected virtual void ActOnMouseStateFlag(MouseUpFlags flag) { }


        protected UIObject _focusedObj = null;
        public virtual void OnMouseDown(MouseButtonEventArgs e)
        {
            if (e.Button == MouseButton.Left && e.Action == InputAction.Press)
            {
                Vector2 MouseCoordinates = NormalizeGlobalCoordinates(new Vector2(_cursorObject.Position.X, _cursorObject.Position.Y), WindowConstants.ClientSize);

                UIObject tempFocusedObj = null;

                _UI.ForEach(uiObj =>
                {
                    uiObj.BoundsCheck(MouseCoordinates, _camera, null, UIEventType.MouseDown);

                    uiObj.BoundsCheck(MouseCoordinates, _camera, (obj) =>
                    {
                        tempFocusedObj = obj;

                        if (_focusedObj == null)
                        {
                            _focusedObj = tempFocusedObj;
                        }
                        OnObjectFocused();
                    }, UIEventType.Focus);
                });

                if (_focusedObj != null && (tempFocusedObj == null || tempFocusedObj.ObjectID != _focusedObj.ObjectID))
                {
                    _focusedObj.FocusEnd();
                    _focusedObj = tempFocusedObj;
                    OnObjectFocusEnd();
                }
            }
        }

        protected UIObject _grabbedObj = null;
        public virtual bool OnMouseMove()
        {
            if (_mouseTimer.ElapsedMilliseconds > 50) //check every 20 ms
            {
                _mouseTimer.Restart();
                _hoverTimer.Reset();

                Vector2 MouseCoordinates = NormalizeGlobalCoordinates(new Vector2(_cursorObject.Position.X, _cursorObject.Position.Y), WindowConstants.ClientSize);

                if (MouseState != null && MouseState.IsButtonDown(MouseButton.Left))
                {
                    if (_grabbedObj == null)
                    {
                        _UI.ForEach(uiObj =>
                        {
                            uiObj.BoundsCheck(MouseCoordinates, _camera, (obj) => _grabbedObj = obj, UIEventType.Grab);
                        });
                    }
                    else
                    {
                        Vector3 mouseCoordScreenSpace = WindowConstants.ConvertGlobalToScreenSpaceCoordinates(_cursorObject.Position);
                        //_grabbedObj.SetPosition(mouseCoordScreenSpace - _grabbedObj._grabbedDeltaPos);
                        _grabbedObj.SetDragPosition(mouseCoordScreenSpace - _grabbedObj._grabbedDeltaPos);
                    }
                } //resolve all ongoing grab effects

                if (MouseState != null && !MouseState.IsButtonDown(MouseButton.Left) && _grabbedObj != null)
                {
                    _grabbedObj.GrabEnd();
                    _grabbedObj = null;
                } //resolve all grabbed effects

                _UI.ForEach(uiObj =>
                {
                    uiObj.BoundsCheck(MouseCoordinates, _camera, null, UIEventType.Hover);

                    uiObj.BoundsCheck(MouseCoordinates, _camera, (obj) =>
                    {
                        if (obj.HasTimedHoverEffect)
                        {
                            _hoverTimer.Restart();
                            _hoveredObject = obj;
                        }
                    }, UIEventType.TimedHover);
                }); //check hovered objects




                Vector3 mouseRayNear = _mouseRay.UnProject(_cursorObject.Position.X, _cursorObject.Position.Y, 0, _camera, WindowConstants.ClientSize); // start of ray (near plane)
                Vector3 mouseRayFar = _mouseRay.UnProject(_cursorObject.Position.X, _cursorObject.Position.Y, 1, _camera, WindowConstants.ClientSize); // end of ray (far plane)

                EvaluateObjectHover(mouseRayNear, mouseRayFar);

                return true;
            }
            else
                return false;
        }

        public virtual void EvaluateObjectHover(Vector3 mouseRayNear, Vector3 mouseRayFar)
        {
            //_tileMaps.ForEach(map =>
            //{
            //    map.EndHover();

            //    ObjectCursorBoundsCheck(map.Tiles, mouseRayNear, mouseRayFar, (tile) =>
            //    {
            //        if (tile.Hoverable)
            //        { 
            //            map.HoverTile(tile);
            //        }
            //        _hoverTimer.Restart();
            //    });
            //});

            //ObjectCursorBoundsCheck(_units, mouseRayNear, mouseRayFar, (unit) =>
            //{
            //    if (unit.Hoverable) 
            //    {
            //        unit.OnHover();
            //    }
            //    _hoverTimer.Restart();
            //}, notFound => notFound.HoverEnd());

        }

        public virtual bool OnKeyDown(KeyboardKeyEventArgs e)
        {
            bool interceptKeystrokes = GetBit(_interceptKeystrokes, ObjectType.All);

            if (!interceptKeystrokes)
            {
                _UI.ForEach(obj =>
                {
                    obj.OnKeyDown(e);
                });

                if (!e.IsRepeat)
                {
                    switch (e.Key)
                    {
                        case Keys.D1:
                            NumberPressed(new SceneEventArgs(this, EventAction.OneKeyDown));
                            break;
                        case Keys.D2:
                            NumberPressed(new SceneEventArgs(this, EventAction.TwoKeyDown));
                            break;
                        case Keys.D3:
                            NumberPressed(new SceneEventArgs(this, EventAction.ThreeKeyDown));
                            break;
                        case Keys.D4:
                            NumberPressed(new SceneEventArgs(this, EventAction.FourKeyDown));
                            break;
                        case Keys.D5:
                            NumberPressed(new SceneEventArgs(this, EventAction.FiveKeyDown));
                            break;
                        case Keys.D6:
                            NumberPressed(new SceneEventArgs(this, EventAction.SixKeyDown));
                            break;
                        case Keys.D7:
                            NumberPressed(new SceneEventArgs(this, EventAction.SevenKeyDown));
                            break;
                        case Keys.D8:
                            NumberPressed(new SceneEventArgs(this, EventAction.EightKeyDown));
                            break;
                    }
                }
            }

            return !interceptKeystrokes;
        }

        public virtual bool OnKeyUp(KeyboardKeyEventArgs e)
        {
            bool interceptKeystrokes = GetBit(_interceptKeystrokes, ObjectType.All);

            if (!interceptKeystrokes)
            {
                _UI.ForEach(obj =>
                {
                    obj.OnKeyUp(e);
                });
            }

            return !interceptKeystrokes;
        }

        public virtual void OnUpdateFrame(FrameEventArgs args)
        {
            if (_focusedObj != null && !_focusedObj.Focused)
            {
                _focusedObj = null;
                OnObjectFocusEnd();
            }
            else if (_focusedObj != null)
            {
                _focusedObj.OnUpdate(MouseState);
            }

            if (_hoverTimer.ElapsedMilliseconds > 500)
            {
                _hoverTimer.Reset();
                _hoveredObject.OnTimedHover();
            }
        }

        public virtual void OnUnitClicked(Unit unit, MouseButton button) { }
        public virtual void OnUIClicked(UIObject uiObj) { }
        public virtual void OnTileClicked(TileMap map, BaseTile tile, MouseButton button, ContextManager<MouseUpFlags> flags) { }

        public virtual void OnCameraMoved()
        {
            _units.ForEach(u =>
            {
                if (u.StatusBarComp != null)
                {
                    u.StatusBarComp.OnCameraMove();
                }
            });
        }

        public virtual void OnObjectFocused()
        {
            Message msg = new Message(MessageType.Request, MessageBody.InterceptKeyStrokes, MessageTarget.All);
            MessageCenter.SendMessage(msg);
        }

        public virtual void OnObjectFocusEnd()
        {
            Message msg = new Message(MessageType.Request, MessageBody.EndKeyStrokeInterception, MessageTarget.All);
            MessageCenter.SendMessage(msg);
        }

        public virtual void OnRenderEnd() { }
        #endregion

        #region event actions

        public delegate void SceneEventHandler(SceneEventArgs args);

        public event SceneEventHandler OnNumberPressed;

        public event SceneEventHandler OnUIForceClose;

        protected void NumberPressed(SceneEventArgs args)
        {
            OnNumberPressed?.Invoke(args);
        }
        protected void UIForceClose(SceneEventArgs args) 
        {
            OnUIForceClose?.Invoke(args);
        }


        #endregion

        //accesses the method used to determine whether the cursor is overlapping an object that is defined in the main file.
        protected List<T> ObjectCursorBoundsCheck<T>(List<T> listObjects, Vector3 near, Vector3 far, Action<T> foundAction = null, Action<T> notFoundAction = null) where T : GameObject
        {
            List<T> foundObjects = new List<T>();

            listObjects.ForEach(listObj =>
            {
                listObj.BaseObjects.ForEach(obj =>
                {
                    if (obj.Bounds.Contains3D(near, far, _camera))
                    {
                        foundObjects.Add(listObj);
                        foundAction?.Invoke(listObj);
                    }
                    else
                    {
                        notFoundAction?.Invoke(listObj);
                    }
                });

            });

            return foundObjects;
        }
        //Game logic goes here
        public virtual void Logic()
        {

        }



        public Scene() { }

        #region Misc helper functions
        protected Vector2 NormalizeGlobalCoordinates(Vector2 vec, Vector2i clientSize)
        {
            float X = (vec.X / clientSize.X) * 2 - 1; //converts it into local opengl coordinates
            float Y = ((vec.Y / clientSize.Y) * 2 - 1) * -1; //converts it into local opengl coordinates

            return new Vector2(X, Y);
        }
        protected bool GetBit(int b, int bitNumber)
        {
            return (b & (1 << bitNumber)) != 0;
        }

        protected bool GetBit(int b, ObjectType bitNumber)
        {
            return (b & (1 << (int)bitNumber)) != 0;
        }
        #endregion

        #region Render helper functions

        public List<T> GetRenderTarget<T>(ObjectType type)
        {
            if (GetBit(_disableRender, ObjectType.All) || GetBit(_disableRender, type))
            {
                return new List<T>();
            }

            return type switch
            {
                ObjectType.UI => _UI as List<T>,
                ObjectType.Tile => _tileMapController.TileMaps as List<T>,
                ObjectType.Unit => _collatedUnits as List<T>,
                ObjectType.Text => _text as List<T>,
                ObjectType.GenericObject => _genericObjects as List<T>,
                ObjectType.LowPriorityObject => _lowPriorityObjects as List<T>,
                _ => new List<T>(),
            };
        }

        public void CollateUnit(Unit unit)
        {
            lock(_renderedUnits)
            if (!_renderedUnits.TryGetValue(unit, out Unit found))
            {
                _renderedUnits.Add(unit);
                ContextManager.SetFlag(GeneralContextFlags.UnitCollationRequired, true);
            }
        }

        public void DecollateUnit(Unit unit)
        {
            lock (_renderedUnits)
            if (_renderedUnits.TryGetValue(unit, out Unit found))
            {
                _renderedUnits.Remove(unit);
                ContextManager.SetFlag(GeneralContextFlags.UnitCollationRequired, true);
            }
        }

        public void CollateUnits()
        {
            _collatedUnits = _renderedUnits.ToList();
            ContextManager.SetFlag(GeneralContextFlags.UnitCollationRequired, false);
        }

        public virtual void UpdateLightObstructionMap()
        {
            Lighting.UpdateObstructionMap(LightObstructions, ref Rendering.Renderer._instancedRenderArray);
        }

        public virtual void UpdateLightTexture()
        {
            Lighting.UpdateLightTexture(LightGenerators, ref Rendering.Renderer._instancedRenderArray);
        }

        public void QueueLightUpdate() 
        {
            ContextManager.SetFlag(GeneralContextFlags.UpdateLighting, true);
        }
        public void QueueLightObstructionUpdate()
        {
            RefillLightObstructions();
            ContextManager.SetFlag(GeneralContextFlags.UpdateLightObstructionMap, true);
        }

        public void RefillLightObstructions()
        {
            LightObstructions.Clear();

            for (int i = 0; i < _tileMapController.TileMaps.Count; i++)
            {
                for (int j = 0; j < _tileMapController.TileMaps[i].TileChunks.Count; j++)
                {
                    for (int k = 0; k < _tileMapController.TileMaps[i].TileChunks[j].Structures.Count; k++)
                    {
                        Game.Structures.Structure structure = _tileMapController.TileMaps[i].TileChunks[j].Structures[k];

                        if (structure.LightObstruction.Valid)
                        {
                            LightObstructions.Add(structure.LightObstruction);
                        }
                    }
                }
            }
        }
        #endregion
    }


    public class MessageCenter
    {
        public Action<Message> ParseMessage = null;

        public Action<Message> _sendMessage = null;

        public int SceneID => _sceneID;
        private int _sceneID = -1;

        public MessageCenter(int id) 
        {
            _sceneID = id;
        }

        public Message CreateMessage(MessageType msgType, MessageBody msgBody, MessageTarget msgTarget, TargetAmount targetAmount = TargetAmount.All)
        {
            return new Message(msgType, msgBody, msgTarget, targetAmount) { Sender = SceneID };
        }

        public void SendMessage(Message msg) 
        {
            msg.Sender = SceneID;
            _sendMessage?.Invoke(msg);
        }
    }
}
