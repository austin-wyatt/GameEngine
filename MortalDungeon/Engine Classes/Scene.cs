using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Engine_Classes
{
    public class Scene
    {
        public List<BaseObject> _objects; //all objects associated with this Scene
        public List<BaseObject> _cameraAffectedObjects; //objects that are being transformed based on the camera's position
        public List<BaseObject> _renderedObjects; //objects that are currently being rendered
        public List<BaseObject> _staticObjects; //objects that are being rendered without being affected by the camera
        public List<BaseObject> _clickableObjects; //objects that will contain an OnClick effect

        //further renderable objects to be added here (and in the render method with the appropriate shaders). Water objects, skybox objects, parallax objects, etc

        Vector3 SceneSize; //the origin point will be the center of the screen and will extend half of the X parameter to the left and right, half of the Y parameter up and down, etc

        public Scene(Vector3 sceneSize, List<BaseObject> Objects, List<BaseObject> CamAffected, List<BaseObject> RenderedObjects, List<BaseObject> StaticObjects, List<BaseObject> ClickableObjects)
        {
            _objects = Objects;
            _cameraAffectedObjects = CamAffected;
            _renderedObjects = RenderedObjects;
            _staticObjects = StaticObjects;
            _clickableObjects = ClickableObjects;
            SceneSize = sceneSize;
        }

        public Scene() { }
    }


    public static class Scenes 
    {
        public static Scene TestScene = new Scene();

    }
}
