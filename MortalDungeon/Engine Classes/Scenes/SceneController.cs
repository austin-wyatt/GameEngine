using Empyrean.Engine_Classes.MiscOperations;
using Empyrean.Engine_Classes.Rendering;
using Empyrean.Engine_Classes.Scenes;
using Empyrean.Game.Tiles;
using Empyrean.Objects;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Empyrean.Engine_Classes.Scenes
{
    public class SceneController
    {
        public List<Scene> Scenes = new List<Scene>();

        public Camera Camera;

        public SceneController(Camera camera) 
        {
            Camera = camera;

            ObjectCulling.Initialize();
            ObjectCulling.UpdateValues(Camera);

            Camera.Update -= _onCameraUpdate;
            Camera.Update += _onCameraUpdate;
        }

        public int AddScene(Scene scene, int priority) 
        {
            scene.MessageCenter._sendMessage = ParseMessage;
            scene.Priority = priority;
            scene.Controller = this;

            Scenes.Add(scene);

            Scenes.Sort((a, b) => a.Priority - b.Priority);

            return scene.SceneID;
        }

        public void LoadScene(int id, Camera camera = null, MouseRay mouseRay = null) 
        {
            GetScene(id)?.Load(camera, mouseRay);

            CullObjectsInScene();
        }

        public void UnloadScene(int id) 
        {
            Scene scene = GetScene(id);

            if (scene != null) 
            {
                scene.Unload();


                //Renderer.UnloadAllTextures(); //Unloads all loaded textures, removes the scene, then reloads all textures.
                RemoveScene(id);                //If this ends up ruining performance an approach that selectively unloads textures based on the scene will need to created.
                //LoadTextures();               //Might also not be necessary to unload the textures because C# appears to garbage collect the unused textures
                                                //(There could be an issue in the future where a texture remains in the _loadedTextures and doesn't get loaded because
                                                // the actual texture got garbage collected while it remained "loaded")
            }
        }

        public void LoadTextures() 
        {
            for (int u = 0; u < Scenes.Count; u++)
            {
                if (Scenes[u].Loaded)
                {
                    //for (int i = 0; i < Scenes[u]._genericObjects.Count; i++)
                    //{
                    //    Renderer.LoadTextureFromGameObj(Scenes[u]._genericObjects[i]);

                    //    Scenes[u]._genericObjects[i].ParticleGenerators.ForEach(particleGen => //Load ParticleGenerator textures
                    //    {
                    //        Renderer.LoadTextureFromParticleGen(particleGen);
                    //    });
                    //}

                    Scenes[u]._text.ForEach(text => //Load Text textures
                    {
                        if (text.Letters.Count > 0)
                        {
                            Renderer.LoadTextureFromBaseObject(text.Letters[0].BaseObjects[0], false);
                        }
                    });

                    Scenes[u]._units.ForEach(obj =>
                    {
                        Renderer.LoadTextureFromGameObj(obj);
                    });

                    Scenes[u].UIManager.TopLevelObjects.ForEach(obj =>
                    {
                        Renderer.LoadTextureFromUIObject(obj);
                    });
                }
            }

            //TileMapManager.ActiveMaps.ForEach(obj =>
            //{
            //    if (!obj.Render)
            //        return;
            //});
        }

        public void RemoveScene(int id)
        {
            int sceneIndex = GetSceneIndex(id);

            if (sceneIndex != -1) 
            {
                Scenes[sceneIndex].Unload();
                Scenes.RemoveAt(sceneIndex);
            }
        }

        public Scene GetScene(int id) 
        {
            return Scenes.Find(s => s.SceneID == id);
        }

        public int GetSceneIndex(int id)
        {
            return Scenes.FindIndex(s => s.SceneID == id);
        }

        

        public void ParseMessage(Message msg)
        {
            switch (msg.MessageType) 
            {
                case MessageType.Request:
                    EvaluateBody(msg); //send requests to another location for processing
                    break;
                case MessageType.Response:
                    //Console.WriteLine("Response from id " + msg.Sender + ": " + msg.MessageBody.ToString()); //just print the response for now
                    break;
            }
        }

        private void EvaluateBody(Message msg) 
        {
            switch (msg.MessageBody) 
            {
                case MessageBody.Affirmative:
                    break;
                case MessageBody.Negative:
                    break;
                case MessageBody.StopRendering:
                case MessageBody.StartRendering:
                case MessageBody.InterceptClicks:
                case MessageBody.EndClickInterception:
                case MessageBody.Flag:
                    ForwardMessage(msg);
                    break;
                case MessageBody.LoadScene:
                    break;
                case MessageBody.UnloadScene:
                    UnloadScene(msg.TargetIDs[0]);
                    break;
                default:
                    ForwardMessage(msg);
                    break;
            }
        }

        private void ForwardMessage(Message msg) 
        {
            List<Scene> scenes = EvaluateSceneTargets(msg);

            scenes.ForEach(s => 
            {
                s.MessageCenter.ParseMessage(msg);
            });
        }

        /// <summary>
        /// Returns a list of scenes based on the contents of the SceneTargets field of a message.
        /// If SceneTargets is empty all scenes except for the sender will be included. 
        /// If SceneTargets has ID values it will add the scenes corresponding to those IDs.
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        private List<Scene> EvaluateSceneTargets(Message msg)
        {
            List<Scene> returnList = new List<Scene>();

            if (msg.SceneTargets.Length == 0)
            {
                Scenes.ForEach(scene =>
                {
                    if (scene.SceneID != msg.Sender) 
                    {
                        returnList.Add(scene);
                    }
                });
            }
            else
            {
                for (int i = 0; i < msg.SceneTargets.Length; i++)
                {
                    Scene scene = GetScene(msg.SceneTargets[i]);
                    if (scene.SceneID != -1) 
                    {
                        returnList.Add(scene);
                    }
                }
            }

            return returnList;
        }

        //separate public method for this just so we can easily keep track of where this functionality gets used
        public void CullObjects() 
        {
            CullObjectsInScene();
        }

        private void CullObjectsInScene() 
        {
            ObjectCulling._culledChunks = 0;

            //lock (TileMapManager._loadLock)
            //{
            //    for(int i = 0; i < TileMapManager.VisibleMapsList.Count; i++)
            //    {
            //        for (int j = 0; j < TileMapManager.VisibleMapsList[i].TileChunks.Count; j++)
            //        {
            //            ObjectCulling.CullTileChunk(TileMapManager.VisibleMapsList[i].TileChunks[j]);
            //        }
            //    }
            //}

            ObjectCulling.CullListOfUnits(TileMapManager.Scene._units);
        }

        private void _onCameraUpdate(Camera cam)
        {
            ObjectCulling.UpdateValues(Camera);
            CullObjectsInScene();
        }
    }

    public enum MessageType 
    {
        Request,
        Response
    }

    public enum MessageBody
    {
        Negative,
        Affirmative,
        StopRendering,
        StartRendering,
        InterceptClicks,
        EndClickInterception,
        InterceptKeyStrokes,
        EndKeyStrokeInterception,
        UnloadScene,
        LoadScene,

        Flag
    }

    public enum MessageTarget
    {
        All               = 0b10000000,
        UI                = 0b00000001,
        Unit              = 0b00000010,
        Tile              = 0b00000100,
        Text              = 0b00001000,
        GenericObject     = 0b00010000,
        LowPriorityObject = 0b00100000,
    }

    public enum TargetAmount 
    {
        All,
        None, //currently unsupported
        Some //currently unsupported
    }

    public enum MessageFlag 
    {
        None,
        OpenEscapeMenu
    }

    public class Message
    {
        public MessageType MessageType;
        public MessageBody MessageBody;
        public MessageTarget MessageTarget;
        public TargetAmount TargetAmount;

        public int Sender = -1;

        public int[] TargetIDs = new int[0]; //if specified, integer ids of the object type you are trying to affect
        public int[] SceneTargets = new int[0]; //if no scene targets are passed then the message will be sent to every scene besides the sender

        public MessageFlag Flag = MessageFlag.None;
        public Message(MessageType msgType, MessageBody msgBody, MessageTarget msgTarget, TargetAmount targetAmount = TargetAmount.All) 
        {
            MessageType = msgType;
            MessageBody = msgBody;
            MessageTarget = msgTarget;
            TargetAmount = targetAmount;
        }

        public Message CreateAffirmativeResponse(int senderID) 
        {
            return new Message(MessageType.Response, MessageBody.Affirmative, MessageTarget, TargetAmount) { Sender = senderID };
        }
        public Message CreateNegativeResponse(int senderID)
        {
            return new Message(MessageType.Response, MessageBody.Negative, MessageTarget, TargetAmount) { Sender = senderID };
        }
    }


}
