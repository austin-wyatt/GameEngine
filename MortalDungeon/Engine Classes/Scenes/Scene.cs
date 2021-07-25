using MortalDungeon.Engine_Classes.MiscOperations;
using MortalDungeon.Game.GameObjects;
using MortalDungeon.Game.Objects;
using MortalDungeon.Game.Tiles;
using MortalDungeon.Game.UI;
using MortalDungeon.Game.Units;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using static MortalDungeon.Game.UI.GameUIObjects;

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

    public class Scene
    {
        public List<GameObject> _genericObjects = new List<GameObject>(); //GameObjects that are not Units and are being rendered independently
        public List<Text> _text = new List<Text>();
        public List<TileMap> _tileMaps = new List<TileMap>(); //The map/maps to render
        public List<Unit> _units = new List<Unit>(); //The units to render
        public List<UIObject> _UI = new List<UIObject>();

        public Action ExitFunc = null; //function used to exit the application

        public bool Loaded = false;

        public int SceneID = -1;
        public MessageCenter MessageCenter = new MessageCenter();

        #region Messaging flags
        protected int _interceptClicks = 0b0; //see MessageTarget enum in SceneController for notable values

        protected int _disableRender = 0b0;

        protected int _interceptKeystrokes = 0b0;
        #endregion

        public Camera _camera;
        public BaseObject _cursorObject;
        public MouseRay _mouseRay;

        public Vector3 ScenePosition;

        public KeyboardState KeyboardState = default;
        public MouseState MouseState = default;

        protected Random rand = new Random();
        private Stopwatch mouseTimer = new Stopwatch();
        protected void InitializeFields()
        {
            _genericObjects = new List<GameObject>();
            _text = new List<Text>();
            _tileMaps = new List<TileMap>(); //The map/maps to render
            _units = new List<Unit>(); //The units to render
            _UI = new List<UIObject>();

            MessageCenter.ParseMessage = ParseMessage;

            ScenePosition = new Vector3(0, 0, 0);
        }
        public virtual void Load(Camera camera = null, BaseObject cursorObject = null, MouseRay mouseRay = null) //all object initialization should be handled here
        {
            Loaded = true;
            _camera = camera;
            _cursorObject = cursorObject;
            _mouseRay = mouseRay;

            mouseTimer.Start();
        }

        public virtual void Unload()
        {
            InitializeFields();

            Loaded = false;
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

                if(!GetBit(_interceptClicks, ObjectType.UI))
                _UI.ForEach(uiObj =>
                {
                    if (uiObj.Clickable && uiObj.Render && !uiObj.Disabled)
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
                    clickProcessed = true;
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
        }

        public virtual void onMouseDown(MouseButtonEventArgs e)
        {
            if (e.Button == MouseButton.Left && e.Action == InputAction.Press)
            {
                Vector2 MouseCoordinates = NormalizeGlobalCoordinates(new Vector2(_cursorObject.Position.X, _cursorObject.Position.Y), WindowConstants.ClientSize);

                _UI.ForEach(uiObj =>
                {
                    if (uiObj.Clickable && uiObj.Render && !uiObj.Disabled)
                    {
                        uiObj.BoundsCheck(MouseCoordinates, _camera, null, UIHelpers.BoundsCheckType.MouseDown);
                    }
                });
            }
        }

        protected bool _objectGrabbed = false;
        protected UIObject grabbedObj = null;
        protected List<BaseTile> _hoverableTiles = new List<BaseTile>();
        public virtual bool onMouseMove(MouseMoveEventArgs e)
        {
            if (mouseTimer.ElapsedMilliseconds > 20) //check every 20 ms
            {
                mouseTimer.Restart();

                Vector2 MouseCoordinates = NormalizeGlobalCoordinates(new Vector2(_cursorObject.Position.X, _cursorObject.Position.Y), WindowConstants.ClientSize);

                if (MouseState != null && MouseState.IsButtonDown(MouseButton.Left))
                {
                    _UI.ForEach(uiObj =>
                    {
                        uiObj.ForEach((obj) =>
                        {
                            if (obj.Draggable)
                            {
                                if (obj.Grabbed)
                                {
                                    Vector3 mouseCoordScreenSpace = WindowConstants.ConvertGlobalToScreenSpaceCoordinates(_cursorObject.Position);
                                    //obj.SetPositionConditional(mouseCoordScreenSpace - obj._grabbedDeltaPos, uiObj =>
                                    //{
                                    //    if (obj.GetType().Name == uiObj.GetType().Name)
                                    //        return true;
                                    //    return false;
                                    //    //return true;
                                    //}, 1);
                                    if (grabbedObj != null)
                                        grabbedObj.SetPosition(mouseCoordScreenSpace - grabbedObj._grabbedDeltaPos);
                                    else
                                    {
                                        obj.GrabEnd();
                                    }
                                    //Console.WriteLine(grabbedObj.GetType().Name);
                                }
                                else if (!_objectGrabbed)
                                {
                                    obj.BoundsCheck(MouseCoordinates, _camera, (_) =>
                                    {
                                        _objectGrabbed = true;
                                        grabbedObj = _;
                                    }, UIHelpers.BoundsCheckType.Grab);
                                }
                            }
                        });

                    });
                } //resolve all ongoing grab effects

                if (MouseState != null && !MouseState.IsButtonDown(MouseButton.Left) && _objectGrabbed)
                {
                    //_UI.ForEach(uiObj => uiObj.ForEach(obj => grabbedObj.GrabEnd()));
                    grabbedObj.GrabEnd();
                    _objectGrabbed = false;
                    grabbedObj = null;
                } //resolve all grabbed effects

                _UI.ForEach(uiObj =>
                {
                    uiObj.ForEach(obj =>
                    {
                        if (obj.Hoverable)
                        {
                            obj.BoundsCheck(MouseCoordinates, _camera, null, UIHelpers.BoundsCheckType.Hover);
                        }
                    });
                }); //check hovered objects


                Vector3 mouseRayNear = _mouseRay.UnProject(_cursorObject.Position.X, _cursorObject.Position.Y, 0, _camera, WindowConstants.ClientSize); // start of ray (near plane)
                Vector3 mouseRayFar = _mouseRay.UnProject(_cursorObject.Position.X, _cursorObject.Position.Y, 1, _camera, WindowConstants.ClientSize); // end of ray (far plane)

                _tileMaps.ForEach(map =>
                {
                    _hoverableTiles.Clear();

                    map.EndHover();

                    ObjectCursorBoundsCheck(map.Tiles, mouseRayNear, mouseRayFar,(tile) =>
                    {
                        if(tile.Hoverable)
                            map.HoverTile(tile);
                    });
                });


                

                return true;
            }
            else
                return false;
        }

        public virtual void onKeyDown(KeyboardKeyEventArgs e) { }

        public virtual bool onKeyUp(KeyboardKeyEventArgs e) 
        {
            return !GetBit(_interceptKeystrokes, ObjectType.All);
        }

        public virtual void onUpdateFrame(FrameEventArgs args) { }
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

        public virtual void onUnitClicked(Unit unit) { }
        public virtual void onUIClicked(UIObject uiObj) { }
        public virtual void onTileClicked(TileMap map, BaseTile tile) { }

        public Scene() { }

        #region Misc helper functions
        protected Vector2 NormalizeGlobalCoordinates(Vector2 vec, Vector2i clientSize)
        {
            float X = (vec.X / clientSize.X) * 2 - 1; //converts it into local opengl coordinates
            float Y = ((vec.Y / clientSize.Y) * 2 - 1) * -1; //converts it into local opengl coordinates

            return new Vector2(X, Y);
        }
        protected BaseObject GetObjWithHighestZ(List<BaseObject> objs) //don't pass a list of 0 objects
        {
            BaseObject foundObject;
            foundObject = objs[0];
            objs.ForEach(obj =>
            {
                if (obj.Position.Z > foundObject.Position.Z)
                    foundObject = obj;
            });

            return foundObject;
        }
        protected GameObject GetObjWithHighestZ(List<GameObject> objs) //don't pass a list of 0 objects
        {
            GameObject foundObject;
            foundObject = objs[0];
            objs.ForEach(obj =>
            {
                if (obj.Position.Z > foundObject.Position.Z)
                    foundObject = obj;
            });

            return foundObject;
        }

        protected List<T> GetDerivedClassesFromList<T, Y>(List<Y> list) where T : Y, new()
        {
            List<T> foundObjs = new List<T>();
            T temp = new T();

            list.ForEach(obj => 
            {
                if (obj.GetType() == temp.GetType()) 
                {
                    foundObjs.Add((T)obj);
                }
            });

            return foundObjs;
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



    public class CombatScene : Scene 
    {
        public int Round = 0;
        public List<Unit> InitiativeOrder = new List<Unit>();
        public int UnitTakingTurn = 0; //the unit in the initiative order that is going
        public EnergyDisplayBar EnergyDisplayBar;

        /// <summary>
        /// Start the next round
        /// </summary>
        public virtual void AdvanceRound() 
        {
            Round++;

            StartRound();
        }

        /// <summary>
        /// End the current round and calculate anything that needs to be calculated at that point
        /// </summary>
        public virtual void CompleteRound() 
        {
            //do stuff that needs to be done when a round is completed

            AdvanceRound();
        }

        /// <summary>
        /// Makes any calculations that need to be made at the start of the round
        /// </summary>
        public virtual void StartRound() 
        {
            UnitTakingTurn = 0;

            //do calculations here (advance an event, show a cutscene, etc)

            StartTurn();
        }

        /// <summary>
        /// Start the turn for the unit that is currently up in the initiative order
        /// </summary>
        public virtual void StartTurn() 
        {
            //change the UI, move the camera, show which unit is selected, etc
        }

        /// <summary>
        /// Complete the current unit's turn and start the next unit's turn
        /// </summary>
        public virtual void CompleteTurn() 
        {
            UnitTakingTurn++;

            if (UnitTakingTurn == InitiativeOrder.Count) 
            {
                CompleteRound();
                return;
            }

            StartTurn(); //Advance to the next unit's turn
        }
    }


    public class MessageCenter
    {
        public Action<Message> ParseMessage = null;

        public Action<Message> SendMessage = null;

        public int SceneID = -1;

        public MessageCenter() { }

        public Message CreateMessage(MessageType msgType, MessageBody msgBody, MessageTarget msgTarget, TargetAmount targetAmount = TargetAmount.All)
        {
            return new Message(msgType, msgBody, msgTarget, targetAmount) { Sender = SceneID };
        }
    }
}
