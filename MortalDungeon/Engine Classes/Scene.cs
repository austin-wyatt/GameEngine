using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Engine_Classes
{
    public class Scene
    {
        public List<BaseObject> _objects; //all objects associated with this Scene
        public List<BaseObject> _renderedObjects; //objects that are currently being rendered
        public List<BaseObject> _clickableObjects; //objects that will contain an OnClick effect

        //further renderable objects to be added here (and in the render method with the appropriate shaders). Water objects, skybox objects, parallax objects, etc

        public List<Texture> _textures = new List<Texture>(); //all of the textures that will need to be loaded for this scene

        public Camera _camera = null; //can be used to define a scene's custom view transformations to be applied instead of the main camera (this can be used for cases such as previews of units off screen, etc)

        public Vector3 ScenePosition;


        //Scene will contain the location, sprites, textures, actions (ie what to do on a mouse click for example).
        //The scene will have an overall position that determines where in the game world it's placed. 
        //public Scene(Vector3 scenePosition, List<BaseObject> Objects, List<BaseObject> RenderedObjects, List<BaseObject> ClickableObjects)
        //{
        //    _objects = Objects;
        //    _renderedObjects = RenderedObjects;
        //    _clickableObjects = ClickableObjects;
        //    ScenePosition = scenePosition;
        //}

        public Scene() { }
    }


    public static class Scenes 
    {
        public static Scene TestScene = new Scene();

    }
}
