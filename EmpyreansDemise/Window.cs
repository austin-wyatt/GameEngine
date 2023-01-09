using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Empyrean.Engine_Classes;
using Empyrean.Engine_Classes.Audio;
using Empyrean.Engine_Classes.MiscOperations;
using Empyrean.Engine_Classes.Objects.UIComponents;
using Empyrean.Engine_Classes.Rendering;
using Empyrean.Engine_Classes.Scenes;
using Empyrean.Game.Scripting;
using Empyrean.Game.Objects;
using Empyrean.Game.SceneDefinitions;
using Empyrean.Game.Serializers;
using Empyrean.Game.Tiles;
using Empyrean.Game.UI;
using Empyrean.Game.Units;
using Empyrean.Objects;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Empyrean.Engine_Classes.Lighting;
using Monitor = System.Threading.Monitor;

namespace Empyrean
{
    public static class GlobalRandom 
    {
        private static Random _random = new ConsistentRandom();

        private static object _lock = new object();
        public static int Next() 
        {
            lock (_lock)
            {
                return _random.Next();
            }
        }

        public static int Next(int val)
        {
            lock (_lock)
            {
                return _random.Next(val);
            }
        }

        public static float NextFloat() 
        {
            lock (_lock)
            {
                return (float)_random.NextDouble();
            }
        }

        public static float NextFloat(float minValue, float maxValue)
        {
            lock (_lock)
            {
                float val = (float)_random.NextDouble();

                val *= maxValue - minValue;
                val += minValue;

                return val;
            }
        }

    }
    public static class WindowConstants
    {
        public static readonly Vector2 ScreenUnits = new Vector2(1000, 1000);
        public static Vector3 CenterScreen = new Vector3(ScreenUnits.X / 2, ScreenUnits.Y / 2, 0); //use to outline bounds;
        public static readonly Vector4 FullColor = new Vector4(1, 1, 1, 1);
        public const int TickDenominator = 45; // 1 divided by this determines the tick rate.
        public static Vector2i ClientSize;
        public static float AspectRatio => (float)ClientSize.X / ClientSize.Y;

        public static bool ShowFPS = true;
        public static bool ShowTicksPerSecond = true;
        public static bool EnableBoundsTestingTools = false;
        public static bool ShowCulledChunks = true;

        public static Stopwatch GlobalTimer = new Stopwatch();

        public static ViewportRectangle GameViewport;

        public static int MainThreadId = -1;
        /// <summary>
        /// Indicates whether the game is running or if it is being loaded as a dll (user has to set manually)
        /// </summary>
        public static bool GameRunning = true;

        public static bool DrawTools = false;


        //Global position = position of objects in the world
        //Local position = [-1:1] opengl coordinates
        //Screen space = [0:ScreenUnits]

        public static Vector3 ConvertGlobalToLocalCoordinates(Vector3 position)
        {
            Vector3 returnVec = new Vector3(position)
            {
                X = (position.X / ScreenUnits.X) * 2 - 1,
                Y = ((position.Y / ScreenUnits.Y) * 2 - 1) * -1,
                Z = position.Z
            };

            return returnVec;
        }

        public static Vector3 ConvertScreenSpaceToLocalCoordinates(Vector3 position)
        {
            Vector3 returnVec = new Vector3(position)
            {
                X = (position.X / ScreenUnits.X),
                Y = (position.Y / ScreenUnits.Y) -1,
                Z = position.Z
            };

            return returnVec;
        }

        public static void ConvertGlobalToLocalCoordinatesInPlace(ref Vector3 position)
        {
            position.X = (position.X / ScreenUnits.X) * 2 - 1;
            position.Y = ((position.Y / ScreenUnits.Y) * 2 - 1) * -1;
        }

        public static Vector3 ConvertLocalToGlobalCoordinates(Vector3 position)
        {
            return new Vector3(position.X * ScreenUnits.X, -position.Y * ScreenUnits.Y, position.Z);
        }

        //Ie convert mouse coordinates to workable screen coordinates
        public static Vector3 ConvertGlobalToScreenSpaceCoordinates(Vector3 position) 
        {
            Vector3 returnVec = new Vector3(position)
            {
                X = position.X / ClientSize.X * ScreenUnits.X,
                Y = position.Y / ClientSize.Y * ScreenUnits.Y,
                Z = position.Z
            };

            return returnVec;
        }

        public static Vector3 ConvertScreenSpaceToGlobalCoordinates(Vector3 position)
        {
            Vector3 returnVec = new Vector3(position)
            {
                X = position.X * ClientSize.X / ScreenUnits.X,
                Y = position.Y * ClientSize.Y / ScreenUnits.Y,
                Z = position.Z
            };

            return returnVec;
        }

        public static Vector3 ConvertLocalToScreenSpaceCoordinates(Vector2 position)
        {
            Vector3 returnVec = new Vector3(position)
            {
                X = (position.X + 1) / 2 * ScreenUnits.X,
                Y = ScreenUnits.Y - (position.Y + 1) / 2 * ScreenUnits.Y,
                Z = 0
            };

            return returnVec;
        }

        public static Vector3 ConvertLocalToScreenSpaceCoordinates(Vector3 position)
        {
            Vector3 returnVec = new Vector3(position)
            {
                X = (position.X + 1) / 2 * ScreenUnits.X,
                Y = ScreenUnits.Y - (position.Y + 1) / 2 * ScreenUnits.Y,
                Z = position.Z
            };

            return returnVec;
        }

        public static Vector3 ConvertLocalToScreenSpaceCoordinates(ref Vector3 position)
        {
            position.X = (position.X + 1) / 2 * ScreenUnits.X;
            position.Y = ScreenUnits.Y - (position.Y + 1) / 2 * ScreenUnits.Y;

            return position;
        }

        public static Vector2 NormalizeGlobalCoordinates(Vector2 vec, Vector2i clientSize)
        {
            float X = (vec.X / clientSize.X) * 2 - 1; //converts it into local opengl coordinates
            float Y = ((vec.Y / clientSize.Y) * 2 - 1) * -1; //converts it into local opengl coordinates

            return new Vector2(X, Y);
        }

        public static bool InMainThread(Thread currThread)
        {
            return currThread.ManagedThreadId == MainThreadId;
        }
    }

    public enum ContentContext
    {
        Game,
        Tools
    }

    public class Window : GameWindow
    {
        private Vector2i WindowSize = new Vector2i();

        public static Vector2 _cursorCoords;
        public static MouseCursor CursorObj;
        private static List<UIObject> _cursorList;

        public static ContentContext CurrentContext = ContentContext.Game;

        public static List<BaseObject> _renderedItems = new List<BaseObject>();

        public static Task GameLoop;

        private List<Vector2> _points = new List<Vector2>();
        private List<Vector3> _lines = new List<Vector3>();

        //private List<Scene> _scenes = new List<Scene>();

        public SceneController _sceneController;

        private Stopwatch _timer;
        private Stopwatch _gameTimer;
        

        public static Camera _camera;
        private MouseRay _mouseRay;
        private bool _firstMove = true;
        private Vector2 _lastPos;


        private uint tick = 0;
        private uint lastTick = 0;

        private uint highFreqTick = 0;
        private uint highFreqLastTick = 0;

        private const float tickRate = (float)1 / WindowConstants.TickDenominator;
        private const float highFreqTickRate = (float)1 / 135;

        public static CubeMap SkyBox = new CubeMap();

        public static Action CloseWindow = null;

        public static CombatScene Scene;

        /// <summary>
        /// Dictates how man high frequency ticks are made for each normal game tick
        /// </summary>
        public static int GameSpeed = 3;

        public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings) 
        {  }


        protected override void OnLoad()
        {
            WindowConstants.ClientSize.X = ClientSize.X;
            WindowConstants.ClientSize.Y = ClientSize.Y;

            WindowConstants.GameViewport.Width = ClientSize.X;
            WindowConstants.GameViewport.Height = ClientSize.Y;

            Thread.CurrentThread.Priority = ThreadPriority.Highest;
            WindowConstants.MainThreadId = Thread.CurrentThread.ManagedThreadId;

            CloseWindow = () => Close();

            //Set listeners
            MouseDown += Window_MouseDown;
            MouseMove += Window_MouseMove;
            MouseUp += Window_MouseUp;
            KeyUp += Window_KeyUp;
            KeyDown += Window_KeyDown;

            Renderer.Initialize();
            CalculationThread.Initialize();

            FeatureManager.Initialize();

            //LuaManager.Initialize();

            JSManager.Initialize();

            AnimationSerializer.Initialize();

            SetWindowSize();

            CursorObj = new MouseCursor();
            _cursorList = new List<UIObject> { CursorObj };

            _camera = new Camera(Vector3.UnitZ * 3, WindowConstants.ClientSize.X / (float)WindowConstants.ClientSize.Y);
            _camera.Pitch += 7;
            //_camera.Yaw += 2;

            _camera.UpdateProjectionMatrix();

            _mouseRay = new MouseRay(_camera);

            _cursorCoords = new Vector2(0, 0);

            _sceneController = new SceneController(_camera);

            _gameTimer = new Stopwatch();
            _gameTimer.Start();

            if (WindowConstants.EnableBoundsTestingTools)
            {
                Scene boundScene = new BoundsTestScene();

                int boundSceneID = _sceneController.AddScene(boundScene, 1);
                _sceneController.LoadScene(boundSceneID, _camera, _mouseRay);
            }
            //else if (false) 
            //{
            //    Scene writeData = new WriteDataScene();

            //    int writeDataID = _sceneController.AddScene(writeData, 1);
            //    _sceneController.LoadScene(writeDataID, _camera, _cursorObject, _mouseRay);
            //}
            else
            {
                CombatScene menuScene = new MenuScene();

                int menuSceneID = _sceneController.AddScene(menuScene, 2);

                //Scene escapeMenuScene = new EscapeMenuScene();

                //int escapeMenuID = _sceneController.AddScene(escapeMenuScene, 1);

                _sceneController.LoadScene(menuSceneID, _camera, _mouseRay);
                //_sceneController.LoadScene(escapeMenuID, _camera, _mouseRay);

                SkyBox.ImagePaths = new string[]
                {
                    "Resources/skybox/forest/right.jpg",
                    "Resources/skybox/forest/left.jpg",
                    "Resources/skybox/forest/top.jpg",
                    "Resources/skybox/forest/bottom.jpg",
                    "Resources/skybox/forest/front.jpg",
                    "Resources/skybox/forest/back.jpg"
                };

                SkyBox.LoadImages();
            }


            _sceneController.LoadTextures();
            LoadTextures();

            _timer = new Stopwatch();
            _timer.Start();

            WindowConstants.GlobalTimer.Restart();

            CursorGrabbed = false;
            CursorVisible = false;

            TickAllObjects();

            base.OnLoad();


            GameLoop = new Task(() =>
            {
                double timeValue;

                List<double> times = new List<double>(1000);

                while (true)
                {
                    timeValue = _gameTimer.Elapsed.TotalSeconds;

                    //times.Add(timeValue);

                    //if (times.Count > 500)
                    //{
                    //    for (int i = 0; i < times.Count; i++)
                    //    {
                    //        Console.WriteLine(times[i]);
                    //    }

                    //    times.Clear();
                    //}

                    if (timeValue > highFreqTickRate)
                    {
                        highFreqTick++;
                        _gameTimer.Restart();

                        for (int i = 0; i < _sceneController.Scenes.Count; i++)
                        {
                            for (int j = 0; j < _sceneController.Scenes[i].TimedTickableObjects.Count; j++)
                            {
                                _sceneController.Scenes[i].TimedTickableObjects[j].Tick();
                            }
                        }

                        if (highFreqTick % GameSpeed == 0)
                        {
                            tick++;
                        }

                        TickAllObjects();
                    }

                    //if (timeValue > tickRate)
                    //{
                    //    _gameTimer.Restart();
                    //    tick++;
                    //}

                    Thread.Sleep(1);
                }
            }, TaskCreationOptions.LongRunning);

            GameLoop.Start();
        }

        public static object _renderLock = new object();
        protected override void OnRenderFrame(FrameEventArgs args)
        {
            Monitor.Enter(_renderLock);

            base.OnRenderFrame(args);

            OnRenderBegin();

            InvokeQueuedRenderAction();

            GL.ClearColor(Renderer.ClearColor.X, Renderer.ClearColor.Y, Renderer.ClearColor.Z, Renderer.ClearColor.W);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);

            double timeValue;

            //Renderer.CheckError();

            //if (frames > 10000)
            //{
            //    frames = 0;
            //    time = args.Time;
            //}

            //frames++;
            //time = (time + args.Time) * 0.5;

            //if (args.Time / time > 1.75)
            //{
            //    Console.WriteLine($"75% variation in FPS detected. Average time {time}. Detected time {args.Time}.");
            //}


            ////FPS
            if (WindowConstants.ShowFPS)
            {
                timeValue = _timer.Elapsed.TotalSeconds;
                Renderer.FPSCount++;

                if (timeValue > 1)
                {
                    Console.Write("FPS: " + Renderer.FPSCount + "   Draws: " + Renderer.DrawCount / Renderer.FPSCount);
                    if (WindowConstants.ShowCulledChunks)
                    {
                        Console.Write("   Culled Chunks: " + ObjectCulling._culledChunks);
                    }

                    if (WindowConstants.ShowTicksPerSecond || true)
                    {
                        Console.Write("   High freq ticks: " + _highFreqTickCounter);
                        _highFreqTickCounter = 0;

                        Console.Write("   Ticks: " + _tickCounter);
                        _tickCounter = 0;
                    }


                    Console.Write("   Objects drawn: " + Renderer.ObjectsDrawn / Renderer.FPSCount);

                    Console.Write("\n");

                    //Console.WriteLine("Updates per second: " + UpdateCount);

                    _timer.Restart();
                    Renderer.FPSCount = 0;
                    Renderer.DrawCount = 0;
                    Renderer.ObjectsDrawn = 0;
                    UpdateCount = 0;
                }
            }

            //GL.Viewport(WindowConstants.GameViewport.X, WindowConstants.GameViewport.Y, WindowConstants.GameViewport.Width, WindowConstants.GameViewport.Height);
            //Renderer.ViewportRectangle = WindowConstants.GameViewport;

            Matrix4 viewMatrix = _camera.GetViewMatrix();
            Matrix4 projectionMatrix = _camera.ProjectionMatrix;
            Matrix4.Mult(viewMatrix, projectionMatrix, out Matrix4 cameraMatrix);

            Matrix4.Mult(viewMatrix.ClearTranslation(), projectionMatrix, out Matrix4 skyboxMatrix);

            Shaders.SKYBOX_SHADER.Use();
            Shaders.SKYBOX_SHADER.SetMatrix4("camera", ref skyboxMatrix);

            for (int i = 0; i < _points.Count; i++)
            {
                Shaders.POINT_SHADER.Use();

                float[] point = new float[] { _points[i].X, _points[i].Y };
                GL.BindBuffer(BufferTarget.ArrayBuffer, Renderer._vertexArrayObject);
                GL.BufferData(BufferTarget.ArrayBuffer, point.Length * sizeof(float), point, BufferUsageHint.DynamicDraw);
                GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, point.Length * sizeof(float), 0);

                GL.DrawArrays(PrimitiveType.Points, 0, 1);
            } //Points

            #region shader uniforms
            Shaders.DEFERRED_LIGHTING_SHADER.Use();
            Shaders.DEFERRED_LIGHTING_SHADER.SetMatrix4("camera", ref cameraMatrix);
            LightingManager.SetLightingParametersForShader(Shaders.DEFERRED_LIGHTING_SHADER, use: false);

            Shaders.CHUNK_SHADER.Use();
            Shaders.CHUNK_SHADER.SetMatrix4("camera", ref cameraMatrix);
            //LightingManager.SetLightingParametersForShader(Shaders.CHUNK_SHADER, use: false);


            //all objects using the fast default shader are handled here
            Shaders.FAST_DEFAULT_SHADER_DEFERRED.Use();
            Shaders.FAST_DEFAULT_SHADER_DEFERRED.SetMatrix4("camera", ref cameraMatrix);
            //LightingManager.SetLightingParametersForShader(Shaders.FAST_DEFAULT_SHADER_DEFERRED, use: false);


            Shaders.PARTICLE_SHADER.Use();
            Shaders.PARTICLE_SHADER.SetMatrix4("camera", ref cameraMatrix);
            LightingManager.SetBasicLightingParametersForShader(Shaders.PARTICLE_SHADER, use: false);

            Shaders.FAST_DEFAULT_SHADER_IMMEDIATE.Use();
            Shaders.FAST_DEFAULT_SHADER_IMMEDIATE.SetMatrix4("camera", ref cameraMatrix);
            LightingManager.SetBasicLightingParametersForShader(Shaders.FAST_DEFAULT_SHADER_IMMEDIATE, use: false);

            #endregion

            //Tile maps
            RenderingQueue.ClearTileInstancedRenderData();

            foreach (var map in TileMapManager.VisibleMaps)
            {
                for (int i = 0; i < map.TileChunks.Count; i++)
                {
                    if (!map.TileChunks[i].Cull)
                    {
                        RenderingQueue.QueueChunkRenderData(map.TileChunks[i]);
                    }
                }
            }

            for (int i = 0; i < _sceneController.Scenes.Count; i++)
            {
                var scene = _sceneController.Scenes[i];
                if (scene.SceneContext == ContentContext.Game)
                {
                    GL.Viewport(WindowConstants.GameViewport.X, WindowConstants.GameViewport.Y, WindowConstants.GameViewport.Width, WindowConstants.GameViewport.Height);
                    Renderer.ViewportRectangle = WindowConstants.GameViewport;
                }
                else
                {
                    GL.Viewport(0, 0, WindowConstants.ClientSize.X, WindowConstants.ClientSize.Y);
                    Renderer.ViewportRectangle.X = 0;
                    Renderer.ViewportRectangle.Y = 0;
                    Renderer.ViewportRectangle.Width = WindowConstants.ClientSize.X;
                    Renderer.ViewportRectangle.Height = WindowConstants.ClientSize.Y;
                }

                scene.OnRender();

                //scene.InvokeQueuedRenderAction();

                if (scene.ContextManager.GetFlag(GeneralContextFlags.UnitCollationRequired))
                {
                    scene.CollateUnits();
                }

                RenderingQueue.QueueObjectsForRender(scene.GetRenderTarget<GameObject>(ObjectType.GenericObject));
                RenderingQueue.QueueLowPriorityObjectsForRender(scene.GetRenderTarget<GameObject>(ObjectType.LowPriorityObject));

                //bool updateTileMaps = scene.ContextManager.GetFlag(GeneralContextFlags.EnableTileMapUpdate);


                if (scene.SceneContext == ContentContext.Game)
                {
                    var combatScene = scene as CombatScene;
                    if (combatScene != null)
                    {
                        RenderingQueue.SetFogQuad(combatScene._fogQuad);
                    }

                    RenderingQueue.QueueUnitsForRender(scene.GetRenderTarget<Unit>(ObjectType.Unit)); //Units
                }


                lock (scene.UIManager._UILock)
                {
                    RenderingQueue.QueueNestedUI(scene.GetRenderTarget<UIObject>(ObjectType.UI)); //UI
                }

                //lock (scene.UIManager._renderGroupLock)
                //{
                //    RenderingQueue.QueueUIInstancedRenderData(scene.UIManager);
                //}


                //scene.GetRenderTarget<GameObject>(ObjectType.GenericObject).ForEach(gameObject =>
                //{
                //    gameObject.ParticleGenerators.ForEach(generator =>
                //    {
                //        if (generator.Playing)
                //        {
                //            RenderingQueue.QueueParticlesForRender(generator);
                //        }
                //    });
                //});

                for (int j = 0; j < scene._particleGenerators.Count; j++)
                {
                    if (scene._particleGenerators[j].Playing)
                    {
                        RenderingQueue.QueueParticlesForRender(scene._particleGenerators[j]);
                    }
                }

                if (scene.IndividualMeshes.Count > 0)
                {
                    List<IndividualMesh> meshes = scene.IndividualMeshes.GetList();

                    Shaders.INDIVIDUAL_MESH_SHADER.Use();
                    Shaders.INDIVIDUAL_MESH_SHADER.SetMatrix4("camera", ref cameraMatrix);
                    LightingManager.SetLightingParametersForShader(Shaders.INDIVIDUAL_MESH_SHADER, use: false);

                    RenderingQueue.QueueIndividualMeshesForRender(meshes);
                }


                RenderingQueue.RenderQueue();
            }



            if(CurrentContext == ContentContext.Tools)
            {
                GL.Viewport(0, 0, WindowConstants.ClientSize.X, WindowConstants.ClientSize.Y);
                Renderer.ViewportRectangle.X = 0;
                Renderer.ViewportRectangle.Y = 0;
                Renderer.ViewportRectangle.Width = WindowConstants.ClientSize.X;
                Renderer.ViewportRectangle.Height = WindowConstants.ClientSize.Y;
            }


            Renderer.RenderObjectsInstancedGeneric(_cursorList, ref Renderer._instancedRenderArray);

            Shaders.DEFAULT_SHADER.Use();
            Shaders.DEFAULT_SHADER.SetMatrix4("camera", ref cameraMatrix);
            Shaders.DEFAULT_SHADER.SetFloat("alpha_threshold", RenderingConstants.DefaultAlphaThreshold);

            GL.Clear(ClearBufferMask.DepthBufferBit);

            for(int i = 0; i < _renderedItems.Count; i++)
            {
                Renderer.RenderObject(_renderedItems[i]);
            } //Old handling, used for lines

            for (int i = 0; i < _sceneController.Scenes.Count; i++)
            {
                _sceneController.Scenes[i].OnRenderEnd();
                _sceneController.Scenes[i].InvokeQueuedRenderAction();
            }

            OnRenderEnd();

            //_renderDiagnostics.Add(new RenderDiagnosticData()
            //{
            //    TimeInRender = RenderTime,
            //    Timestamp = Program.ProgramTimer.ElapsedMilliseconds,
            //    RunningSlowly = IsRunningSlowly
            //});


            SwapBuffers();

            //For whatever reason, calling GL.Finish almost completely eliminates 
            //a set of sporadic frame drops that would commonly occur when clicking/moving
            //the mouse/pressing a key. Although, when the program has just begun running
            //a few of these frame drops can still occur. After 30 seconds to a minute
            //they should stop appearing.
            //^ Leaving this for posterity. The problem was actually the Nvidia setting "Threaded optimization"
            //being set to "Auto". After setting it to "Off" the frame drop problem disappeared completely 
            //GL.Finish();

            System.Threading.Monitor.Exit(_renderLock);
        }

        public struct RenderDiagnosticData
        {
            public double TimeInRender;
            public double Timestamp;
            public bool RunningSlowly;
        }
        public static List<RenderDiagnosticData> _renderDiagnostics = new List<RenderDiagnosticData>(10000);


        public delegate void WindowRenderEventArgs();
        public static event WindowRenderEventArgs RenderEnd;
        public static event WindowRenderEventArgs RenderBegin;

        private static void OnRenderEnd()
        {
            Renderer.RenderEnd();
            RenderEnd?.Invoke();
        }

        private static void OnRenderBegin()
        {
            Renderer.RenderStart();
            RenderBegin?.Invoke();
        }

        private int UpdateCount = 0;
        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            if (!IsFocused) // check to see if the window is focused
            {
                return;
            }

            UpdateCount++;

            //var mouse = MouseState;
            float sensitivity = 0.04f;

            float mouseSpeed = 1000;

            // Calculate the offset of the mouse position
            var deltaX = MouseState.X - _lastPos.X;
            var deltaY = MouseState.Y - _lastPos.Y;

            //_cursorCoords = new Vector2(Math.Clamp(_cursorCoords.X + deltaX, 0, WindowConstants.ClientSize.X), 
            //    Math.Clamp(_cursorCoords.Y + deltaY, 0, WindowConstants.ClientSize.Y));

            _cursorCoords = new Vector2(Math.Clamp(MouseState.X, 0, WindowConstants.ClientSize.X),
                Math.Clamp(MouseState.Y, 0, WindowConstants.ClientSize.Y));

            var coords = WindowConstants.ConvertGlobalToScreenSpaceCoordinates(new Vector3(_cursorCoords));

            CursorObj.SetZPosition(-0.99f);
            CursorObj.SetPositionFromAnchor(coords, UIAnchorPosition.TopLeft);

            if (_firstMove)
            {
                _lastPos = new Vector2(MouseState.X, MouseState.Y);
                _firstMove = false;
            }

            //if (MouseState.IsButtonDown(MouseButton.Left) && KeyboardState.IsKeyDown(Keys.LeftControl))
            //{
            //    // Apply the camera pitch and yaw (we clamp the pitch in the camera class)
            //    _sceneController.Scenes.ForEach(scene => scene.OnCameraMoved());

            //    //_camera.Yaw += deltaX * sensitivity;
            //    _camera.Pitch -= deltaY * sensitivity; // reversed since y-coordinates range from bottom to top
            //}
            //else if(MouseState.IsButtonDown(MouseButton.Left) && KeyboardState.IsKeyDown(Keys.LeftAlt))
            //{
            //    //_camera.Yaw += deltaX * sensitivity;
            //}

            _lastPos = new Vector2(MouseState.X, MouseState.Y);

            for(int i = 0; i < _sceneController.Scenes.Count; i++)
            {
                _sceneController.Scenes[i].OnUpdateFrame(args);
            }

            base.OnUpdateFrame(args);
        }

        private void LoadTextures()
        {
            Renderer.LoadTextureFromUIObject(CursorObj);

            _renderedItems.ForEach(obj =>
            {
                Renderer.LoadTextureFromBaseObject(obj);
            });
        }

        private int _tickCounter = 0;
        private int _highFreqTickCounter = 0;
        private void TickAllObjects() 
        {
            if (highFreqTick != highFreqLastTick) 
            {
                _highFreqTickCounter++;

                highFreqLastTick = highFreqTick;
                for(int i = 0; i < _sceneController.Scenes.Count; i++)
                {
                    _sceneController.Scenes[i].OnHighFreqTick();
                }
            }

            if (tick != lastTick)
            {
                _tickCounter++;

                lastTick = tick;

                for(int i = 0; i < _sceneController.Scenes.Count; i++)
                {
                    _sceneController.Scenes[i].OnTick();
                    _sceneController.Scenes[i].PostTickAction();
                }

                //_sceneController.Scenes.ForEach(scene =>
                //{
                //    //if (scene.PauseTicks)
                //    //    return;

                //    scene.OnTick();

                //    scene.PostTickAction();
                //});
            }
        }
        private void SetWindowSize() 
        {
            WindowSize.X = ClientSize.X;
            WindowSize.Y = ClientSize.Y;

            if (_camera != null) 
            {
                _camera.AspectRatio = WindowSize.X / (float)WindowSize.Y;
                _camera.UpdateProjectionMatrix();
            }
        }
        protected override void OnResize(ResizeEventArgs e)
        {
            // When the window gets resized, we have to call GL.Viewport to resize OpenGL's viewport to match the new size.
            // If we don't, the NDC will no longer be correct.
            base.OnResize(e);

            WindowConstants.ClientSize.X = ClientSize.X;
            WindowConstants.ClientSize.Y = ClientSize.Y;

            WindowConstants.GameViewport.Width = ClientSize.X;
            WindowConstants.GameViewport.Height = ClientSize.Y;

            SetWindowSize();

            Renderer.ResizeFBOs(WindowConstants.ClientSize);

            _sceneController.Scenes.ForEach(scene =>
            {
                scene.UIManager.TopLevelObjects.ForEach(ui =>
                {
                    ui.ForEach(obj => obj.OnResize());
                });

                //scene._tileMapController.TileMaps.ForEach(map =>
                //{
                //    map.InitializeTexturedQuad();
                //});
            });
        }

        protected override void OnUnload()
        {
            // Unbind all the resources by binding the targets to 0/null.
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.UseProgram(0);

            // Delete all the resources.
            Renderer.ClearData();

            for(int i = 0; i < ShaderList.AllShaders.Count; i++)
            {
                GL.DeleteProgram(ShaderList.AllShaders[i].Handle);
            }

            base.OnUnload();
        }

        protected override void OnMaximized(MaximizedEventArgs e)
        {
            base.OnMaximized(e);
        }

        protected override void OnMinimized(MinimizedEventArgs e)
        {
            base.OnMinimized(e);
        }

        private Vector2 NormalizeGlobalCoordinates(Vector2 vec, Vector2i clientSize) 
        {
            float X = (vec.X / clientSize.X) * 2 - 1; //converts it into local opengl coordinates
            float Y = ((vec.Y / clientSize.Y) * 2 - 1) * -1; //converts it into local opengl coordinates

            return new Vector2(X, Y);
        }

        private void CreateNewLine(Vector3 line1, Vector3 line2, Vector4 color = new Vector4(), float thickness = 0.01f, bool camPerspective = true)
        {
            if (!camPerspective) //hack because I don't want to figure out what the real issue is here
            {
                line1 += new Vector3(1, -1, 0);
                line2 += new Vector3(1, -1, 0);
            }

            LineObject lineObj = new LineObject(line1, line2, thickness);

            
            //RenderableObject testLine = new RenderableObject(lineObj.CreateLineDefinition(), color, ObjectRenderType.Texture, Shaders.DEFAULT_SHADER);
            BaseObject lineObject = new BaseObject(new LINE_ANIMATION(lineObj).List, 999, "line", line1);
            //lineObject.BaseFrame.ColorProportion = 1.0f;
            lineObject.BaseFrame.CameraPerspective = camPerspective;
            lineObject.BaseFrame.SetBaseColor(color);

            if(camPerspective)
                lineObject.MoveObject(-line1);

            _renderedItems.Add(lineObject);
            LoadTextures();
        }

        #region Event handlers
        private void Window_MouseDown(MouseButtonEventArgs obj)
        {
            for (int i = 0; i < _sceneController.Scenes.Count; i++)
            {
                if(_sceneController.Scenes[i].Loaded)
                    _sceneController.Scenes[i].OnMouseDown(obj);
            }

            CursorObj.BaseObject.SetAnimation(AnimationType.Die);
        }

        private void Window_MouseMove(MouseMoveEventArgs obj)
        {
            for (int i = 0; i < _sceneController.Scenes.Count; i++)
            {
                if (_sceneController.Scenes[i].Loaded)
                    _sceneController.Scenes[i].OnMouseMove();
            }
        }

        private void Window_MouseUp(MouseButtonEventArgs obj)
        {
            for (int i = 0; i < _sceneController.Scenes.Count; i++) 
            {
                if (_sceneController.Scenes[i].Loaded)
                    _sceneController.Scenes[i].OnMouseUp(obj);
            }

            CursorObj.BaseObject.SetAnimation(AnimationType.Idle);
        }

        private void Window_KeyUp(KeyboardKeyEventArgs obj)
        {
            for (int i = 0; i < _sceneController.Scenes.Count; i++)
            {
                if (_sceneController.Scenes[i].Loaded)
                    _sceneController.Scenes[i].OnKeyUp(obj);
            }


            if (WindowConstants.EnableBoundsTestingTools)
            {
                switch (obj.Key)
                {
                    case (Keys.Q):
                        _points.Add(NormalizeGlobalCoordinates(new Vector2(_cursorCoords.X, _cursorCoords.Y), WindowConstants.ClientSize));

                        if (_points.Count > 1)
                        {
                            CreateNewLine(new Vector3(_points[^2].X, _points[^2].Y, 0), new Vector3(_points[^1].X, _points[^1].Y, 0), new Vector4(0, 0, 1, 1), 0.02f, false);
                        }

                        var temp = NormalizeGlobalCoordinates(new Vector2(_cursorCoords.X, _cursorCoords.Y), WindowConstants.ClientSize);
                        //Console.WriteLine("Normalized cursor coordinates: " + temp.X + ", " + temp.Y);
                        break;
                    case (Keys.R):
                        _points.Clear();
                        _renderedItems.Clear();

                        //Console.WriteLine(_camera.Position.Z);
                        break;
                    case (Keys.P):
                        Console.Write("new float[]{\n");
                        _points.ForEach(p =>
                        {
                            Console.Write(p.X + "f, " + p.Y + "f, 0.0f, \n");
                        });
                        Console.Write("}");
                        break;
                    case (Keys.O):
                        _lines.Clear();

                        List<int> indexesToRemove = new List<int>();

                        int index = 0;
                        _renderedItems.ForEach(obj =>
                        {
                            if (obj.Name == "line")
                            {
                                indexesToRemove.Add(index);
                            }
                            index++;
                        });

                        for (int i = indexesToRemove.Count - 1; i >= 0; i--)
                        {
                            _renderedItems.RemoveAt(indexesToRemove[i]);
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        private void Window_KeyDown(KeyboardKeyEventArgs obj) 
        {
            for (int i = 0; i < _sceneController.Scenes.Count; i++)
            {
                if (_sceneController.Scenes[i].Loaded)
                    _sceneController.Scenes[i].OnKeyDown(obj);
            }

            switch (obj.Key)
            {
                case Keys.Home:
                    //CurrentContext = CurrentContext == ContentContext.Game ? ContentContext.Tools : ContentContext.Game;
                    break;
                case Keys.PageUp:
                    //float width = (float)WindowConstants.ClientSize.X / 1.5f;
                    //float height = (float)WindowConstants.ClientSize.Y / 1.5f;

                    //CurrentContext = ContentContext.Tools;
                    //WindowConstants.GameViewport.X = WindowConstants.ClientSize.X - (int)width - 10;
                    //WindowConstants.GameViewport.Y = 10;
                    //WindowConstants.GameViewport.Width = (int)width;
                    //WindowConstants.GameViewport.Height = (int)height;

                    ////Scene toolScene = new ToolScene();

                    ////int toolSceneID = _sceneController.AddScene(toolScene, 1);
                    ////_sceneController.LoadScene(toolSceneID, _camera, _mouseRay);

                    //open tools
                    break;
                case Keys.PageDown:
                    CurrentContext = ContentContext.Game;
                    WindowConstants.GameViewport.X = 0;
                    WindowConstants.GameViewport.Y = 0;
                    WindowConstants.GameViewport.Width = WindowConstants.ClientSize.X;
                    WindowConstants.GameViewport.Height = WindowConstants.ClientSize.Y;
                    //close tools

                    //var scene = _sceneController.Scenes.Find(s => s.SceneContext == ContentContext.Tools);

                    //if(scene != null)
                    //{
                    //    _sceneController.RemoveScene(scene.SceneID);
                    //}
                    break;
            }
        }
        #endregion

        private const int ACTION_QUEUE_TIMEOUT_MS = 100;
        private static object _renderActionQueueLock = new object();
        private static Queue<Action> _renderActionQueue = new Queue<Action>();
        public static void QueueToRenderCycle(Action action)
        {
            //if(Thread.CurrentThread.ManagedThreadId == WindowConstants.MainThreadId)
            //{
            //    action.Invoke();
            //}
            //else
            //{
            //    lock (_renderActionQueueLock)
            //    {
            //        _renderActionQueue.Enqueue(action);
            //    }
            //}

            if(Monitor.TryEnter(_renderActionQueueLock, ACTION_QUEUE_TIMEOUT_MS))
            {
                _renderActionQueue.Enqueue(action);
                Monitor.Exit(_renderActionQueueLock);
            }
        }

        public static void InvokeQueuedRenderAction()
        {
            if (_renderActionQueue.Count > 0)
            {
                if (Monitor.TryEnter(_renderActionQueueLock, ACTION_QUEUE_TIMEOUT_MS))
                {
                    int count = _renderActionQueue.Count;

                    for (int i = 0; i < count; i++)
                    {
                        _renderActionQueue.Dequeue().Invoke();
                    }

                    Monitor.Exit(_renderActionQueueLock);
                }
            }
        }
    }
}
