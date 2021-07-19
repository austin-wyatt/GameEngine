using MortalDungeon.Engine_Classes.MiscOperations;
using MortalDungeon.Game.GameObjects;
using MortalDungeon.Game.Objects;
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

namespace MortalDungeon.Engine_Classes
{
    public class Scene
    {
        public List<GameObject> _renderedObjects = new List<GameObject>(); //GameObjects that are not Units and are being rendered independently
        public List<Text> _text = new List<Text>();
        public List<TileMap> _tileMaps = new List<TileMap>(); //The map/maps to render
        public List<Unit> _units = new List<Unit>(); //The units to render
        public List<UIObject> _UI = new List<UIObject>();

        public Action ExitFunc = null; //function used to exit the application

        public bool Loaded = false;

        //further renderable objects to be added here (and in the render method with the appropriate shaders). Water objects, skybox objects, parallax objects, etc

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
            _renderedObjects = new List<GameObject>();
            _text = new List<Text>();
            _tileMaps = new List<TileMap>(); //The map/maps to render
            _units = new List<Unit>(); //The units to render

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


        #region Event handlers
        public virtual void onMouseUp(MouseButtonEventArgs e) 
        {
            if (e.Button == MouseButton.Left && e.Action == InputAction.Release)
            {
                Vector2 MouseCoordinates = NormalizeGlobalCoordinates(new Vector2(_cursorObject.Position.X, _cursorObject.Position.Y), WindowConstants.ClientSize);

                _tileMaps.ForEach(map =>
                {
                    ObjectCursorBoundsCheck(map.Tiles).ForEach(foundObj =>
                    {
                        onTileClicked(map, foundObj);
                    });
                });

                ObjectCursorBoundsCheck(_units).ForEach(foundObj =>
                {
                    onUnitClicked(foundObj);
                });

                _UI.ForEach(uiObj =>
                {
                    if (uiObj.Clickable && uiObj.Render && !uiObj.Disabled)
                    {
                        uiObj.BoundsCheck(MouseCoordinates, _camera, (obj) => onUIClicked(obj));
                    }
                });
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
                                    Vector3 mouseCoordScreenSpace = WindowConstants.ConvertGlobalToScreenSpaceCoordinates(WindowConstants.ClientSize, _cursorObject.Position);
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


                return true;
            }
            else
                return false;
        }

        public virtual void onKeyDown(KeyboardKeyEventArgs e) { }

        public virtual void onKeyUp(KeyboardKeyEventArgs e) { }

        public virtual void onUpdateFrame() { }
        #endregion

        //accesses the method used to determine whether the cursor is overlapping an object that is defined in the main file.
        protected List<GameObject> ObjectCursorBoundsCheck(List<GameObject> listObjects)
        {
            Vector3 near = _mouseRay.UnProject(_cursorObject.Position.X, _cursorObject.Position.Y, 0, _camera, WindowConstants.ClientSize); // start of ray (near plane)
            Vector3 far = _mouseRay.UnProject(_cursorObject.Position.X, _cursorObject.Position.Y, 1, _camera, WindowConstants.ClientSize); // end of ray (far plane)

            List<GameObject> foundObjects = new List<GameObject>();

            listObjects.ForEach(listObj =>
            {
                listObj.BaseObjects.ForEach(obj =>
                {
                    if (obj.Bounds.Contains3D(near, far, _camera))
                    {
                        foundObjects.Add(listObj);
                    }
                });

            });

            return foundObjects;
        }
        protected List<Unit> ObjectCursorBoundsCheck(List<Unit> listObjects)
        {
            Vector3 near = _mouseRay.UnProject(_cursorObject.Position.X, _cursorObject.Position.Y, 0, _camera, WindowConstants.ClientSize); // start of ray (near plane)
            Vector3 far = _mouseRay.UnProject(_cursorObject.Position.X, _cursorObject.Position.Y, 1, _camera, WindowConstants.ClientSize); // end of ray (far plane)

            List<Unit> foundObjects = new List<Unit>();


            listObjects.ForEach(listObj =>
            {
                if (listObj.Clickable) 
                {
                    listObj.BaseObjects.ForEach(obj =>
                    {
                        if (obj.Bounds.Contains3D(near, far, _camera))
                        {
                            foundObjects.Add(listObj);
                        }
                    });
                }
            });

            return foundObjects;
        }
        protected List<BaseTile> ObjectCursorBoundsCheck(List<BaseTile> listObjects)
        {
            Vector3 near = _mouseRay.UnProject(_cursorObject.Position.X, _cursorObject.Position.Y, 0, _camera, WindowConstants.ClientSize); // start of ray (near plane)
            Vector3 far = _mouseRay.UnProject(_cursorObject.Position.X, _cursorObject.Position.Y, 1, _camera, WindowConstants.ClientSize); // end of ray (far plane)

            List<BaseTile> foundObjects = new List<BaseTile>();


            listObjects.ForEach(listObj =>
            {
                if (listObj.Clickable)
                {
                    listObj.BaseObjects.ForEach(obj =>
                    {
                        if (obj.Bounds.Contains3D(near, far, _camera))
                        {
                            foundObjects.Add(listObj);
                        }
                    });
                }
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


    public static class Scenes 
    {
        //public static Scene TestScene = new Scene();

    }
}
