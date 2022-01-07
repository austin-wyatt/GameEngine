using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.Audio;
using MortalDungeon.Engine_Classes.MiscOperations;
using MortalDungeon.Engine_Classes.Rendering;
using MortalDungeon.Engine_Classes.Scenes;
using MortalDungeon.Game.GameObjects;
using MortalDungeon.Game.Objects;
using MortalDungeon.Game.SceneDefinitions;
using MortalDungeon.Game.Serializers;
using MortalDungeon.Game.Tiles;
using MortalDungeon.Game.UI;
using MortalDungeon.Game.Units;
using MortalDungeon.Objects;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;



namespace MortalDungeon
{
    public static class GlobalRandom 
    {
        private static Random _random = new ConsistentRandom();

        public static int Next() 
        {
            return _random.Next();
        }

        public static int Next(int val)
        {
            return _random.Next(val);
        }

        public static float NextFloat() 
        {
            return (float)_random.NextDouble();
        }

        public static float NextFloat(float minValue, float maxValue)
        {
            float val = (float)_random.NextDouble();

            val *= maxValue - minValue;
            val += minValue;

            return val;
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
        public static bool ShowTicksPerSecond = false;
        public static bool EnableBoundsTestingTools = false;
        public static bool ShowCulledChunks = true;

        public static Stopwatch GlobalTimer = new Stopwatch();

        public static int MainThreadId = 0;

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

        public static void ConvertGlobalToLocalCoordinatesInPlace(ref Vector3 position)
        {
            position.X = (position.X / ScreenUnits.X) * 2 - 1;
            position.Y = ((position.Y / ScreenUnits.Y) * 2 - 1) * -1;
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

        public static Vector2 NormalizeGlobalCoordinates(Vector2 vec, Vector2i clientSize)
        {
            float X = (vec.X / clientSize.X) * 2 - 1; //converts it into local opengl coordinates
            float Y = ((vec.Y / clientSize.Y) * 2 - 1) * -1; //converts it into local opengl coordinates

            return new Vector2(X, Y);
        }
    }

    public class Window : GameWindow
    {
        private Vector2i WindowSize = new Vector2i();

        BaseObject _cursorObject;

        public static List<BaseObject> _renderedItems = new List<BaseObject>();

        public static Task GameLoop;

        private List<Vector2> _points = new List<Vector2>();
        private List<Vector3> _lines = new List<Vector3>();

        //private List<Scene> _scenes = new List<Scene>();

        private SceneController _sceneController;

        private Stopwatch _timer;
        private Stopwatch _gameTimer;


        private Camera _camera;
        private MouseRay _mouseRay;
        private bool _firstMove = true;
        private Vector2 _lastPos;


        private uint tick = 0;
        private uint lastTick = 0;

        private uint highFreqTick = 0;
        private uint highFreqLastTick = 0;

        private const float tickRate = (float)1 / WindowConstants.TickDenominator;
        private const float highFreqTickRate = (float)1 / 90;

        public CubeMap SkyBox = new CubeMap();

        public static Action CloseWindow = null;

        public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings) 
        {  }


        protected override void OnLoad()
        {
            WindowConstants.ClientSize.X = ClientSize.X;
            WindowConstants.ClientSize.Y = ClientSize.Y;

            WindowConstants.MainThreadId = Thread.CurrentThread.ManagedThreadId;

            CloseWindow = () => Close();

            //Set listeners
            MouseDown += Window_MouseDown;
            MouseMove += Window_MouseMove;
            MouseUp += Window_MouseUp;
            KeyUp += Window_KeyUp;
            KeyDown += Window_KeyDown;

            Renderer.Initialize();

            VisionMap.Initialize();
            FeatureManager.Initialize();

            SetWindowSize();
            _camera = new Camera(Vector3.UnitZ * 3, WindowConstants.ClientSize.X / (float)WindowConstants.ClientSize.Y);
            _camera.Pitch += 7;
            //_camera.Yaw += 2;

            _camera.UpdateProjectionMatrix();

            _mouseRay = new MouseRay(_camera);

            _cursorObject = new BaseObject(CURSOR_ANIMATION.List, 0, "cursor", new Vector3(MousePosition))
            {
                LockToWindow = true
            };
            _cursorObject.BaseFrame.ScaleAll(0.1f);

            _sceneController = new SceneController(_camera);

            _gameTimer = new Stopwatch();
            _gameTimer.Start();

            if (WindowConstants.EnableBoundsTestingTools)
            {
                Scene boundScene = new BoundsTestScene();

                int boundSceneID = _sceneController.AddScene(boundScene, 1);
                _sceneController.LoadScene(boundSceneID, _camera, _cursorObject, _mouseRay);
            }
            //else if (false) 
            //{
            //    Scene writeData = new WriteDataScene();

            //    int writeDataID = _sceneController.AddScene(writeData, 1);
            //    _sceneController.LoadScene(writeDataID, _camera, _cursorObject, _mouseRay);
            //}
            else
            {
                Scene menuScene = new MenuScene();

                int menuSceneID = _sceneController.AddScene(menuScene, 2);

                Scene escapeMenuScene = new EscapeMenuScene();

                int escapeMenuID = _sceneController.AddScene(escapeMenuScene, 1);

                _sceneController.LoadScene(menuSceneID, _camera, _cursorObject, _mouseRay);
                _sceneController.LoadScene(escapeMenuID, _camera, _cursorObject, _mouseRay);

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


            //hides mouse cursor
            CursorGrabbed = true;

            TickAllObjects();

            base.OnLoad();


            GameLoop = new Task(() =>
            {
                double timeValue;

                int waitTime = (int)(highFreqTickRate * 500);
                while (true) 
                {
                    timeValue = _gameTimer.Elapsed.TotalSeconds;

                    if (timeValue > highFreqTickRate) 
                    {
                        highFreqTick++;
                        _gameTimer.Restart();

                        for(int i = 0; i < _sceneController.Scenes.Count; i++)
                        {
                            for(int j = 0; j < _sceneController.Scenes[i].TimedTickableObjects.Count; j++) 
                            {
                                _sceneController.Scenes[i].TimedTickableObjects[j].Tick();
                            }
                        }
                    }

                    //if (timeValue > tickRate)
                    //{
                    //    _gameTimer.Restart();
                    //    tick++;
                    //}
                    if (highFreqTick % 4 == 0)
                    {
                        tick++;
                    }

                    Thread.Sleep(5);
                }
            }, TaskCreationOptions.LongRunning);

            GameLoop.Start();
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            TickAllObjects();

            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.Clear(ClearBufferMask.DepthBufferBit);

            Renderer.RenderStart();

            double timeValue;

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
            timeValue = _timer.Elapsed.TotalSeconds;
            Renderer.FPSCount++;

            if (timeValue > 1 && WindowConstants.ShowFPS)
            {
                Console.Write("FPS: " + Renderer.FPSCount + "   Draws: " + Renderer.DrawCount / Renderer.FPSCount);
                if (WindowConstants.ShowCulledChunks)
                {
                    Console.Write("   Culled Chunks: " + ObjectCulling._culledChunks);
                }

                //Console.Write("   High freq ticks: " + _highFreqTickCounter);
                //_highFreqTickCounter = 0;

                //Console.Write("   Ticks: " + _tickCounter);
                //_tickCounter = 0;

                Console.Write("   Objects drawn: " + Renderer.ObjectsDrawn / Renderer.FPSCount);

                Console.Write("\n");
                _timer.Restart();
                Renderer.FPSCount = 0;
                Renderer.DrawCount = 0;
                Renderer.ObjectsDrawn = 0;
            }

            //Tick counter
            //timeValue = _gameTimer.Elapsed.TotalSeconds;

            //if (timeValue > tickRate)
            //{
            //    _gameTimer.Restart();
            //    tick++;

            //    if (tick % 100 == 0) 
            //    {
            //        Console.WriteLine($"{tickRate}    {timeValue}");
            //    }
            //}

            Matrix4 viewMatrix = _camera.GetViewMatrix();
            Matrix4 projectionMatrix = _camera.ProjectionMatrix;
            Matrix4 cameraMatrix = viewMatrix * projectionMatrix;

            for (int i = 0; i < _points.Count; i++)
            {
                Shaders.POINT_SHADER.Use();

                float[] point = new float[] { _points[i].X, _points[i].Y };
                GL.BindBuffer(BufferTarget.ArrayBuffer, Renderer._vertexArrayObject);
                GL.BufferData(BufferTarget.ArrayBuffer, point.Length * sizeof(float), point, BufferUsageHint.DynamicDraw);
                GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, point.Length * sizeof(float), 0);

                GL.DrawArrays(PrimitiveType.Points, 0, 1);
            } //Points


            //all objects using the fast default shader are handled here
            Shaders.FAST_DEFAULT_SHADER.Use();
            Shaders.FAST_DEFAULT_SHADER.SetMatrix4("camera", cameraMatrix);

            //Shaders.FAST_DEFAULT_SHADER.SetVector3("dirLight.specular", new Vector3(1, 1, 1));
            Shaders.FAST_DEFAULT_SHADER.SetVector3("dirLight.ambient", new Vector3(RenderingConstants.LightColor));
            //Shaders.FAST_DEFAULT_SHADER.SetVector3("dirLight.diffuse", new Vector3(RenderingConstants.LightColor) / 4);

            //Shaders.FAST_DEFAULT_SHADER.SetVector3("dirLight.ambient", new Vector3(1, 1, 1));
            //Shaders.FAST_DEFAULT_SHADER.SetVector3("dirLight.diffuse", new Vector3(1, 1, 1));

            Shaders.FAST_DEFAULT_SHADER.SetVector3("dirLight.direction", new Vector3(0, 1, 0));
            //Shaders.FAST_DEFAULT_SHADER.SetVector3("dirLight.direction", new Vector3(0, 1, -1));
            Shaders.FAST_DEFAULT_SHADER.SetFloat("dirLight.enabled", 1);

            //Shaders.FAST_DEFAULT_SHADER.SetFloat("light.constant", 1.0f);
            //Shaders.FAST_DEFAULT_SHADER.SetFloat("light.linear", 0.022f);
            //Shaders.FAST_DEFAULT_SHADER.SetFloat("light.quadratic", 0.0019f);

            //Shaders.FAST_DEFAULT_SHADER.SetVector3("spotlight.position", _camera.Position);
            //Shaders.FAST_DEFAULT_SHADER.SetVector3("spotlight.direction", _camera.Front);
            //Shaders.FAST_DEFAULT_SHADER.SetFloat("spotlight.cutoff", (float)Math.Cos(MathHelper.DegreesToRadians(12.5f)));
            //Shaders.FAST_DEFAULT_SHADER.SetFloat("spotlight.outerCutoff", (float)Math.Cos(MathHelper.DegreesToRadians(17.5f)));
            //Shaders.FAST_DEFAULT_SHADER.SetFloat("spotlight.enabled", 1);

            //if (BoundsTestScene.SPECULAR_TEST != null)
            //{
            //    BoundsTestScene.SPECULAR_TEST.Use(TextureUnit.Texture15);
            //    Shaders.FAST_DEFAULT_SHADER.SetInt("material[0].specular", 15);
            //}

            Shaders.FAST_DEFAULT_SHADER.SetVector3("viewPosition", _camera.Position);

            _sceneController.Scenes.ForEach(scene =>
            {
                scene.OnRender();

                if (scene.ContextManager.GetFlag(GeneralContextFlags.UnitCollationRequired))
                {
                    scene.CollateUnits();
                }

                RenderingQueue.QueueObjectsForRender(scene.GetRenderTarget<GameObject>(ObjectType.GenericObject));
                RenderingQueue.QueueLowPriorityObjectsForRender(scene.GetRenderTarget<GameObject>(ObjectType.LowPriorityObject));

                bool updateTileMaps = scene.ContextManager.GetFlag(GeneralContextFlags.EnableTileMapUpdate);

                lock (scene._tileMapController._mapLoadLock)
                {
                    scene.GetRenderTarget<TileMap>(ObjectType.Tile).ForEach(tileMap =>
                    {
                    //Renderer.QueueTileObjectsForRender(tileMap.Tiles);
                    //tileMap.TileChunks.ForEach(chunk =>
                    //{
                    //    if (!chunk.Cull)
                    //    {
                    //        RenderingQueue.QueueStructuresForRender(chunk.Structures);
                    //        RenderingQueue.QueueObjectsForRender(chunk.GenericObjects);
                    //    }
                    //});

                    if (tileMap.DynamicTextureInfo.Initialize)
                        {
                            tileMap.InitializeTexturedQuad();
                            tileMap.DynamicTextureInfo.Initialize = false;
                        }

                        if (updateTileMaps && tileMap.DynamicTextureInfo.TextureChanged)
                        {
                            tileMap.UpdateDynamicTexture();
                        }

                        RenderingQueue.QueueTileQuadForRender(tileMap.TexturedQuad);
                    }); //TileMap
                }
                RenderingQueue.QueueTileObjectsForRender(scene._tileMapController.SelectionTiles.ToList());

                RenderingQueue.QueueTileObjectsForRender(scene._tileMapController.GetHoveredTile());



                RenderingQueue.QueueUnitsForRender(scene.GetRenderTarget<Unit>(ObjectType.Unit)); //Units

                lock (scene.UIManager._UILock) 
                {
                    RenderingQueue.QueueNestedUI(new List<UIObject>(scene.GetRenderTarget<UIObject>(ObjectType.UI))); //UI
                }

                scene.GetRenderTarget<Text>(ObjectType.Text).ForEach(text =>
                {
                    if (text.Render && text.Letters.Count > 0)
                    {
                        RenderingQueue.QueueLettersForRender(text.Letters);
                    };
                }); //Text


                scene.GetRenderTarget<GameObject>(ObjectType.GenericObject).ForEach(gameObject =>
                {
                    gameObject.ParticleGenerators.ForEach(generator =>
                    {
                        if (generator.Playing)
                        {
                            RenderingQueue.QueueParticlesForRender(generator);
                        }
                    });
                });

                scene._particleGenerators.ForEach(g =>
                {
                    if (g.Playing)
                    {
                        RenderingQueue.QueueParticlesForRender(g);
                    }
                });
            });

            Shaders.PARTICLE_SHADER.Use();
            Shaders.PARTICLE_SHADER.SetMatrix4("camera", cameraMatrix);

            if (SkyBox != null && SkyBox.Loaded)
            {
                RenderingQueue.RenderSkybox = () =>
                {
                    viewMatrix = _camera.GetViewMatrix().ClearTranslation();
                    cameraMatrix = viewMatrix * projectionMatrix;

                    GL.DepthFunc(DepthFunction.Lequal);

                    Shaders.SKYBOX_SHADER.Use();
                    Shaders.SKYBOX_SHADER.SetMatrix4("camera", cameraMatrix);
                    Renderer.RenderSkybox(SkyBox);

                    GL.DepthFunc(DepthFunction.Less);

                    RenderingQueue.RenderSkybox = null;
                };
            }

            RenderingQueue.RenderQueue();


            Shaders.DEFAULT_SHADER.Use();
            Shaders.DEFAULT_SHADER.SetMatrix4("camera", cameraMatrix);
            Shaders.DEFAULT_SHADER.SetFloat("alpha_threshold", RenderingConstants.DefaultAlphaThreshold);

            GL.Clear(ClearBufferMask.DepthBufferBit);

            _renderedItems.ForEach(obj =>
            {
                Renderer.RenderObject(obj);
            }); //Old handling, used for lines
            Renderer.RenderObject(_cursorObject);



            SwapBuffers();

            base.OnRenderFrame(args);

            _sceneController.Scenes.ForEach(scene =>
            {
                scene.OnRenderEnd();
            });

            Renderer.RenderEnd();
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            if (!IsFocused) // check to see if the window is focused
            {
                return;
            }

            _sceneController.Scenes.ForEach(scene =>
            {
                scene.OnUpdateFrame(args);
            });

            //TickAllObjects();

            var mouse = MouseState;
            float sensitivity = 0.04f;

            // Calculate the offset of the mouse position
            var deltaX = mouse.X - _lastPos.X;
            var deltaY = mouse.Y - _lastPos.Y;

            _cursorObject.SetPosition(new Vector2(_cursorObject.Position.X + deltaX, _cursorObject.Position.Y + deltaY));


            if (_firstMove)
            {
                _lastPos = new Vector2(mouse.X, mouse.Y);
                _firstMove = false;
            }

            if (mouse.IsButtonDown(MouseButton.Left) && KeyboardState.IsKeyDown(Keys.LeftControl))
            {
                // Apply the camera pitch and yaw (we clamp the pitch in the camera class)
                _sceneController.Scenes.ForEach(scene => scene.OnCameraMoved());

                _camera.Yaw += deltaX * sensitivity;

                //if (_camera.Yaw < -120)
                //{
                //    _camera.Yaw = -120;
                //}
                //else if (_camera.Yaw > -60)
                //{
                //    _camera.Yaw = -60;
                //}

                _camera.Pitch -= deltaY * sensitivity; // reversed since y-coordinates range from bottom to top

                //if (_camera.Pitch < -40)
                //{
                //    _camera.Pitch = -40;
                //}
                //else if (_camera.Pitch > 40)
                //{
                //    _camera.Pitch = 40;
                //}
            }

            _lastPos = new Vector2(mouse.X, mouse.Y);

            base.OnUpdateFrame(args);
        }

        private void LoadTextures()
        {
            Renderer.LoadTextureFromBaseObject(_cursorObject);

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
                _sceneController.Scenes.ForEach(scene =>
                {
                    scene.HighFreqTickableObjects.ForEach(obj =>
                    {
                        obj.Tick();
                    });
                });
            }

            if (tick != lastTick)
            {
                _tickCounter++;

                lastTick = tick;
                _sceneController.Scenes.ForEach(scene =>
                {
                    //if (scene.PauseTicks)
                    //    return;

                    Task renderedObjectTask = new Task(() =>
                    {
                        scene._genericObjects.ForEach(gameObject =>
                        {
                            gameObject.Tick();
                        });
                    });

                    Task unitTask = new Task(() =>
                    {
                        scene._units.ForEach(unit =>
                        {
                            unit.Tick();
                        });
                    });

                    Task tileMapTask = new Task(() =>
                    {
                        lock (scene._tileMapController._mapLoadLock)
                        {
                            scene._tileMapController.TileMaps.ForEach(tileMap =>
                            {
                                tileMap.Tick();
                            });
                        }
                    });

                    Task uiTask = new Task(() =>
                    {
                        lock (scene.UIManager._UILock)
                        {
                            foreach(var ui in scene.UIManager.TopLevelObjects)
                            {
                                ui.Tick();
                            }
                        }
                    });

                    Task tickableObjectsTask = new Task(() =>
                    {
                        scene.TickableObjects.ForEach(obj =>
                        {
                            obj.Tick();
                        });

                        scene._particleGenerators.ForEach(gen =>
                        {
                            gen.Tick();
                        });
                    });


                    renderedObjectTask.Start();
                    unitTask.Start();
                    tileMapTask.Start();
                    uiTask.Start();
                    tickableObjectsTask.Start();

                    renderedObjectTask.Wait();
                    unitTask.Wait();
                    tileMapTask.Wait();
                    uiTask.Wait();
                    tickableObjectsTask.Wait();

                    scene.PostTickAction();
                });
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

            GL.Viewport(0, 0, Size.X, Size.Y);
            SetWindowSize();

            Renderer.ResizeFBOs(WindowConstants.ClientSize);

            _sceneController.Scenes.ForEach(scene =>
            {
                scene.UIManager.TopLevelObjects.ForEach(ui =>
                {
                    ui.ForEach(obj => obj.OnResize());
                });

                scene._tileMapController.TileMaps.ForEach(map =>
                {
                    map.InitializeTexturedQuad();
                });
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
                        _points.Add(NormalizeGlobalCoordinates(new Vector2(_cursorObject.Position.X, _cursorObject.Position.Y), WindowConstants.ClientSize));

                        if (_points.Count > 1)
                        {
                            CreateNewLine(new Vector3(_points[^2].X, _points[^2].Y, 0), new Vector3(_points[^1].X, _points[^1].Y, 0), new Vector4(0, 0, 1, 1), 0.02f, false);
                        }

                        var temp = NormalizeGlobalCoordinates(new Vector2(_cursorObject.Position.X, _cursorObject.Position.Y), WindowConstants.ClientSize);
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
        }
        #endregion
    }
}
