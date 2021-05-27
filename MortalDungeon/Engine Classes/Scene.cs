using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Engine_Classes
{
    public class Scene
    {
        public List<BaseObject> _objects = new List<BaseObject>(); //all objects associated with this Scene
        public List<BaseObject> _renderedObjects = new List<BaseObject>(); //objects that are currently being rendered
        public List<BaseObject> _clickableObjects = new List<BaseObject>(); //objects that will contain an OnClick effect
        public List<ParticleGenerator> _particleGenerators = new List<ParticleGenerator>();

        public bool Loaded = false;

        //further renderable objects to be added here (and in the render method with the appropriate shaders). Water objects, skybox objects, parallax objects, etc

        public Camera _camera;
        public BaseObject _cursorObject;
        public Vector2i ClientSize;

        public Vector3 ScenePosition;

        public KeyboardState KeyboardState;
        public MouseState MouseState;

        protected void InitializeFields()
        {
            _objects = new List<BaseObject>();
            _renderedObjects = new List<BaseObject>();
            _clickableObjects = new List<BaseObject>();
            _particleGenerators = new List<ParticleGenerator>();

            ScenePosition = new Vector3(0, 0, 0);
        }
        public virtual void Load(Vector2i clientSize, Camera camera = null, BaseObject cursorObject = null) //all object initialization should be handled here
        {
            Loaded = true;
            _camera = camera;
            _cursorObject = cursorObject;
            ClientSize = clientSize;
        }

        public virtual void Unload()
        {
            InitializeFields();

            Loaded = false;
        }


        protected Func<List<BaseObject>, List<BaseObject>> _cursorBoundsCheck = null;
        public void SetCursorDetectionFunc(Func<List<BaseObject>, List<BaseObject>> func) 
        {
            _cursorBoundsCheck = func;
        }



        //Scene will contain the location, sprites, textures, actions (ie what to do on a mouse click for example).
        //The scene will have an overall position that determines where in the game world it's placed. 

        #region Event handlers
        public virtual void onMouseUp(MouseButtonEventArgs e) { }

        public virtual void onMouseDown(MouseButtonEventArgs e) { }

        public virtual void onMouseMove(MouseMoveEventArgs e) { }

        public virtual void onKeyDown(KeyboardKeyEventArgs e) { }

        public virtual void onKeyUp(KeyboardKeyEventArgs e) { }

        public virtual void onUpdateFrame() { }
        #endregion

        //accesses the method used to determine whether the cursor is overlapping an object that is defined in the main file.
        protected List<BaseObject> ObjectCursorBoundsCheck(List<BaseObject> listObjects)
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
        #endregion
    }


    public static class Scenes 
    {
        //public static Scene TestScene = new Scene();

    }
}
