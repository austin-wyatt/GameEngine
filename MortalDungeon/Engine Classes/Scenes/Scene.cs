using MortalDungeon.Engine_Classes.MiscOperations;
using MortalDungeon.Game.Tiles;
using MortalDungeon.Game.Units;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace MortalDungeon.Engine_Classes.Scenes
{
    public enum ObjectType //corresponds to their bit position in the MessageTarget enum
    {
        UI = 0,
        Unit = 1,
        Tile = 2,
        Text = 3,
        GenericObject = 4,
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
    }

    public class Scene
    {
        public List<GameObject> _genericObjects = new List<GameObject>(); //GameObjects that are not Units and are being rendered independently
        public List<Text> _text = new List<Text>();
        public List<TileMap> _tileMaps = new List<TileMap>(); //The map/maps to render
        public List<Unit> _units = new List<Unit>(); //The units to render
        public List<UIObject> _UI = new List<UIObject>();

        public Action ExitFunc = null; //function used to exit the application

        public bool Loaded = false;

        public int SceneID => _sceneID;
        protected int _sceneID = currentSceneID++;
        protected static int currentSceneID = 0;

        public MessageCenter MessageCenter = null;

        #region Messaging flags
        protected int _interceptClicks = 0b0; //see MessageTarget enum in SceneController for notable values

        protected int _disableRender = 0b0;

        protected int _interceptKeystrokes = 0b0;
        #endregion


        public Dictionary<EventAction, Action> EventActions = new Dictionary<EventAction, Action>();


        public Camera _camera;
        public BaseObject _cursorObject;
        public MouseRay _mouseRay;

        public Vector3 ScenePosition;

        public KeyboardState KeyboardState = default;
        public MouseState MouseState = default;

        protected Random rand = new Random();
        private Stopwatch _mouseTimer = new Stopwatch();
        protected Stopwatch _hoverTimer = new Stopwatch();
        protected GameObject _hoveredObject;
        protected virtual void InitializeFields()
        {
            _genericObjects = new List<GameObject>();
            _text = new List<Text>();
            _tileMaps = new List<TileMap>(); //The map/maps to render
            _units = new List<Unit>(); //The units to render
            _UI = new List<UIObject>();

            MessageCenter = new MessageCenter(SceneID);

            MessageCenter.ParseMessage = ParseMessage;

            ScenePosition = new Vector3(0, 0, 0);
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

        public void AddUI(UIObject ui, int zIndex = -1) 
        {
            _UI.Add(ui);

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
            Console.WriteLine(msg.MessageType.ToString() + " from id " + msg.Sender + " to " + SceneID + ": " + msg.MessageBody.ToString() + " " + msg.MessageTarget.ToString());

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

            MessageCenter.SendMessage(msg.CreateAffirmativeResponse(SceneID));
        }

        private void InterceptClicks(bool intercept, MessageTarget target) 
        {
            if (intercept)
            {
                _interceptClicks = (int)target | _interceptClicks;
            }
            else
            {
                _interceptClicks = _interceptClicks & ~(int)target;
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
                _disableRender = _disableRender & ~(int)target;
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
                _interceptKeystrokes = _interceptKeystrokes & ~(int)target;
            }
        }
        #endregion

        #region Event handlers

        public virtual void onMouseUp(MouseButtonEventArgs e)
        {
            if (e.Button == MouseButton.Left && e.Action == InputAction.Release && !GetBit(_interceptClicks, ObjectType.All))
            {
                Vector2 MouseCoordinates = NormalizeGlobalCoordinates(new Vector2(_cursorObject.Position.X, _cursorObject.Position.Y), WindowConstants.ClientSize);
                bool clickProcessed = false;

                if (!GetBit(_interceptClicks, ObjectType.UI))
                    _UI.ForEach(uiObj =>
                    {
                        //if (uiObj.Clickable && uiObj.Render && !uiObj.Disabled)
                        if (uiObj.Render && !uiObj.Disabled)
                        {
                            uiObj.BoundsCheck(MouseCoordinates, _camera, (obj) =>
                            {
                                onUIClicked(obj);
                                clickProcessed = true;
                            });
                        }
                    });

                if (clickProcessed)
                    return; //stop further clicks from being processed

                Vector3 mouseRayNear = _mouseRay.UnProject(_cursorObject.Position.X, _cursorObject.Position.Y, 0, _camera, WindowConstants.ClientSize); // start of ray (near plane)
                Vector3 mouseRayFar = _mouseRay.UnProject(_cursorObject.Position.X, _cursorObject.Position.Y, 1, _camera, WindowConstants.ClientSize); // end of ray (far plane)

                if (!GetBit(_interceptClicks, ObjectType.Unit))
                    ObjectCursorBoundsCheck(_units, mouseRayNear, mouseRayFar).ForEach(foundObj =>
                    {
                        onUnitClicked(foundObj);
                        //clickProcessed = true;
                    });

                if (clickProcessed)
                    return; //stop further clicks from being processed

                if (!GetBit(_interceptClicks, ObjectType.Tile))
                    _tileMaps.ForEach(map =>
                    {
                        ObjectCursorBoundsCheck(map.Tiles, mouseRayNear, mouseRayFar).ForEach(foundObj =>
                        {
                            onTileClicked(map, foundObj);
                            clickProcessed = true;
                        });
                    });

                if (clickProcessed)
                    return; //stop further clicks from being processed
            }
            else if (e.Button == MouseButton.Right && e.Action == InputAction.Release) 
            {
                HandleRightClick();
            }
        }

        public virtual void HandleRightClick() { }

        protected UIObject _focusedObj = null;
        public virtual void onMouseDown(MouseButtonEventArgs e)
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
                        onObjectFocused();
                    }, UIEventType.Focus);
                });

                if (_focusedObj != null && (tempFocusedObj == null || tempFocusedObj.ObjectID != _focusedObj.ObjectID)) 
                {
                    _focusedObj.FocusEnd();
                    _focusedObj = tempFocusedObj;
                    onObjectFocusEnd();
                }
            }
        }

        protected UIObject _grabbedObj = null;
        public virtual bool onMouseMove()
        {
            if (_mouseTimer.ElapsedMilliseconds > 20) //check every 20 ms
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

        public virtual bool onKeyDown(KeyboardKeyEventArgs e) 
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
                            GetEventAction(EventAction.OneKeyDown)?.Invoke();
                            break;
                        case Keys.D2:
                            GetEventAction(EventAction.TwoKeyDown)?.Invoke();
                            break;
                        case Keys.D3:
                            GetEventAction(EventAction.ThreeKeyDown)?.Invoke();
                            break;
                        case Keys.D4:
                            GetEventAction(EventAction.FourKeyDown)?.Invoke();
                            break;
                        case Keys.D5:
                            GetEventAction(EventAction.FiveKeyDown)?.Invoke();
                            break;
                        case Keys.D6:
                            GetEventAction(EventAction.SixKeyDown)?.Invoke();
                            break;
                        case Keys.D7:
                            GetEventAction(EventAction.SevenKeyDown)?.Invoke();
                            break;
                        case Keys.D8:
                            GetEventAction(EventAction.EightKeyDown)?.Invoke();
                            break;
                    }
                }
            }

            return !interceptKeystrokes;
        }

        public virtual bool onKeyUp(KeyboardKeyEventArgs e) 
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

        public virtual void onUpdateFrame(FrameEventArgs args) 
        {
            if (_focusedObj != null && !_focusedObj.Focused)
            {
                _focusedObj = null;
                onObjectFocusEnd();
            }
            else if(_focusedObj != null) 
            {
                _focusedObj.OnUpdate(MouseState);
            }

            if (_hoverTimer.ElapsedMilliseconds > 500) 
            {
                _hoverTimer.Reset();
                _hoveredObject.OnTimedHover();
            }
        }

        public virtual void onUnitClicked(Unit unit) { }
        public virtual void onUIClicked(UIObject uiObj) { }
        public virtual void onTileClicked(TileMap map, BaseTile tile) { }

        public virtual void onCameraMoved()
        {
            _units.ForEach(u =>
            {
                if (u.StatusBarComp != null)
                {
                    u.StatusBarComp.OnCameraMove();
                }
            });
        }

        public virtual void onObjectFocused() 
        {
            Message msg = new Message(MessageType.Request, MessageBody.InterceptKeyStrokes, MessageTarget.All);
            MessageCenter.SendMessage(msg);
        }

        public virtual void onObjectFocusEnd() 
        {
            Message msg = new Message(MessageType.Request, MessageBody.EndKeyStrokeInterception, MessageTarget.All);
            MessageCenter.SendMessage(msg);
        }

        public virtual void onRenderEnd() { }
        #endregion

        #region event action lists
        



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

        protected Action GetEventAction(EventAction action) 
        {
            EventActions.TryGetValue(action, out Action val);

            return val;
        }
        #endregion

        #region Render helper functions

        public List<T> GetRenderTarget<T>(ObjectType type)
        {
            if (GetBit(_disableRender, ObjectType.All) || GetBit(_disableRender, type)) 
            {
                return new List<T>();
            }

            switch (type) 
            {
                case ObjectType.UI:
                    return _UI as List<T>;
                case ObjectType.Tile:
                    return _tileMaps as List<T>;
                case ObjectType.Unit:
                    return _units as List<T>;
                case ObjectType.Text:
                    return _text as List<T>;
                case ObjectType.GenericObject:
                    return _genericObjects as List<T>;
                default:
                    return new List<T>();
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
