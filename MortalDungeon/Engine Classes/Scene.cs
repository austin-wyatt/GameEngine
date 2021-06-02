using MortalDungeon.Game.GameObjects;
using MortalDungeon.Game.Objects;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace MortalDungeon.Engine_Classes
{
    public class Scene
    {
        public List<GameObject> _renderedObjects = new List<GameObject>(); //GameObjects that are not Units and are being rendered independently
        public List<GameObject> _clickableObjects = new List<GameObject>(); //objects that will contain an OnClick effect
        public List<Text> _text = new List<Text>();
        public List<TileMap> _tileMaps = new List<TileMap>(); //The map/maps to render
        public List<Unit> _units = new List<Unit>(); //The units to render

        public bool Loaded = false;

        //further renderable objects to be added here (and in the render method with the appropriate shaders). Water objects, skybox objects, parallax objects, etc

        public Camera _camera;
        public BaseObject _cursorObject;
        public Vector2i ClientSize;

        public Vector3 ScenePosition;

        public KeyboardState KeyboardState;
        public MouseState MouseState;

        protected Random rand = new Random();
        private Stopwatch mouseTimer = new Stopwatch();
        protected void InitializeFields()
        {
            _renderedObjects = new List<GameObject>();
            _clickableObjects = new List<GameObject>();
            _text = new List<Text>();
            _tileMaps = new List<TileMap>(); //The map/maps to render
            _units = new List<Unit>(); //The units to render

        ScenePosition = new Vector3(0, 0, 0);
        }
        public virtual void Load(Vector2i clientSize, Camera camera = null, BaseObject cursorObject = null) //all object initialization should be handled here
        {
            Loaded = true;
            _camera = camera;
            _cursorObject = cursorObject;
            ClientSize = clientSize;

            mouseTimer.Start();
        }

        public virtual void Unload()
        {
            InitializeFields();

            Loaded = false;
        }


        protected Func<List<GameObject>, List<GameObject>> _cursorBoundsCheck = null;
        public void SetCursorDetectionFunc(Func<List<GameObject>, List<GameObject>> func) 
        {
            _cursorBoundsCheck = func;
        }



        //Scene will contain the location, sprites, textures, actions (ie what to do on a mouse click for example).
        //The scene will have an overall position that determines where in the game world it's placed. 

        #region Event handlers
        public virtual void onMouseUp(MouseButtonEventArgs e) { }

        public virtual void onMouseDown(MouseButtonEventArgs e) { }

        public virtual bool onMouseMove(MouseMoveEventArgs e) 
        {
            if (mouseTimer.ElapsedMilliseconds > 20)
            {
                mouseTimer.Restart();
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
            return _cursorBoundsCheck?.Invoke(listObjects);
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
        #endregion
    }


    public static class Scenes 
    {
        //public static Scene TestScene = new Scene();

    }
}
