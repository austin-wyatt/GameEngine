using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.MiscOperations;
using MortalDungeon.Game.Objects;
using MortalDungeon.Game.Scenes;
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
    public static class WindowConstants
    {
        public static readonly Vector2 ScreenUnits = new Vector2(1000, 1000);
        public static Vector3 CenterScreen = new Vector3(ScreenUnits.X / 2, ScreenUnits.Y / 2, 0); //use to outline bounds;
        public static readonly Vector4 FullColor = new Vector4(1, 1, 1, 1);
        public const int TickDenominator = 45; // 1 divided by this determines the tick rate.
        public static Vector2i ClientSize;
        private static Vector3 tempVector = new Vector3();
        public static Vector3 ConvertGlobalToLocalCoordinates(Vector3 position)
        {
            tempVector.X = (position.X / ScreenUnits.X) * 2 - 1;
            tempVector.Y = ((position.Y / ScreenUnits.Y) * 2 - 1) * -1;
            tempVector.Z = position.Z;

            return tempVector;
        }

        public static Vector3 ConvertGlobalToScreenSpaceCoordinates(Vector2i clientSize, Vector3 position) 
        {
            tempVector.X = position.X / clientSize.X * ScreenUnits.X;
            tempVector.Y = position.Y / clientSize.Y * ScreenUnits.Y;
            tempVector.Z = position.Z;

            return tempVector;
        }

        public static Vector3 ConverLocalToScreenSpaceCoordinates(Vector3 position)
        {
            tempVector.X = (position.X + 1) / 2 * ScreenUnits.X;
            tempVector.Y = (position.Y + 1) / 2 * ScreenUnits.Y;
            tempVector.Z = position.Z;

            return tempVector;
        }
        public static Vector3 ConvertLocalToScreenSpaceCoordinates(Vector2 position)
        {
            tempVector.X = (position.X + 1) / 2 * ScreenUnits.X;
            tempVector.Y = ScreenUnits.Y - (position.Y + 1) / 2 * ScreenUnits.Y;
            tempVector.Z = 0;

            return tempVector;
        }
    }
    public class Window : GameWindow
    {
        private Vector2i WindowSize = new Vector2i();

        BaseObject _cursorObject;

        private List<BaseObject> _renderedItems = new List<BaseObject>();

        private List<Vector2> _points = new List<Vector2>();
        private List<Vector3> _lines = new List<Vector3>();

        private Renderer _renderer = new Renderer();

        private List<Scene> _scenes = new List<Scene>();

        private Stopwatch _timer;
        private Stopwatch _gameTimer;


        private Camera _camera;
        private MouseRay _mouseRay;
        private bool _firstMove = true;
        private Vector2 _lastPos;


        private bool Rendering = false;

        private uint tick = 0;
        private uint lastTick = 0;

        private const float tickRate = (float)1 / WindowConstants.TickDenominator;
        public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings) 
        {  }

        protected override void OnLoad()
        {
            //Set listeners
            MouseDown += Window_MouseDown;
            MouseMove += Window_MouseMove;
            MouseUp += Window_MouseUp;
            KeyUp += Window_KeyUp;
            KeyDown += Window_KeyDown;

            _renderer.Load();
            WindowConstants.ClientSize.X = ClientSize.X;
            WindowConstants.ClientSize.Y = ClientSize.Y;

            SetWindowSize();
            _camera = new Camera(Vector3.UnitZ * 3, WindowConstants.ClientSize.X / (float)WindowConstants.ClientSize.Y);
            _camera.Pitch += 7;

            _mouseRay = new MouseRay(_camera);

            _cursorObject = new BaseObject(CURSOR_ANIMATION.List, 0, "cursor", new Vector3(MousePosition));
            _cursorObject.LockToWindow = true;
            _cursorObject.BaseFrame.ScaleAll(0.1f);



            Scene escapeMenuScene = new EscapeMenuScene(() => Close());
            escapeMenuScene.Load(_camera, _cursorObject, _mouseRay);

            _scenes.Add(escapeMenuScene);


            Scene menuScene = new MenuScene();
            menuScene.Load(_camera, _cursorObject, _mouseRay);

            _scenes.Add(menuScene);


            LoadTextures();

            _timer = new Stopwatch();
            _timer.Start();

            _gameTimer = new Stopwatch();
            _gameTimer.Start();

            //hides mouse cursor
            CursorGrabbed = true;

            base.OnLoad();
        }

        
        protected override void OnRenderFrame(FrameEventArgs args)
        {
            Rendering = true;

            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.Clear(ClearBufferMask.DepthBufferBit);

            double timeValue;

            ////FPS
            timeValue = _timer.Elapsed.TotalSeconds;
            _renderer.FPSCount++;

            if (timeValue > 1 && _renderer.DisplayFPS)
            {
                Console.WriteLine("FPS: " + _renderer.FPSCount + "   Draws: " + _renderer.DrawCount / _renderer.FPSCount);
                _timer.Restart();
                _renderer.FPSCount = 0;
                _renderer.DrawCount = 0;
            }

            //Tick counter
            timeValue = _gameTimer.Elapsed.TotalSeconds;

            if (timeValue > tickRate)
            {
                _gameTimer.Restart();
                tick++;
            }

            Matrix4 viewMatrix = _camera.GetViewMatrix();
            Matrix4 projectionMatrix = _camera.GetProjectionMatrix();
            Matrix4 cameraMatrix = viewMatrix * projectionMatrix;

            for (int i = 0; i < _points.Count; i++)
            {
                Shaders.POINT_SHADER.Use();

                float[] point = new float[] { _points[i].X, _points[i].Y };
                GL.BindBuffer(BufferTarget.ArrayBuffer, _renderer._vertexArrayObject);
                GL.BufferData(BufferTarget.ArrayBuffer, point.Length * sizeof(float), point, BufferUsageHint.DynamicDraw);
                GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, point.Length * sizeof(float), 0);

                GL.DrawArrays(PrimitiveType.Points, 0, 1);
            } //Points

            Shaders.DEFAULT_SHADER.Use();
            Shaders.DEFAULT_SHADER.SetMatrix4("camera", cameraMatrix);
            Shaders.DEFAULT_SHADER.SetFloat("alpha_threshold", 0.25f);

            _renderedItems.ForEach(obj =>
            {
                _renderer.RenderObject(obj);
            }); //Old handling, used for lines
            _renderer.RenderObject(_cursorObject);


            //all objects using the fast default shader are handled here
            Shaders.FAST_DEFAULT_SHADER.Use();
            Shaders.FAST_DEFAULT_SHADER.SetMatrix4("camera", cameraMatrix);
            Shaders.FAST_DEFAULT_SHADER.SetFloat("alpha_threshold", 0.25f);
            _scenes.ForEach(scene =>
            {
                _renderer.RenderObjectsInstanced(scene._renderedObjects);

                scene._tileMaps.ForEach(tileMap =>
                {
                    _renderer.RenderObjectsInstancedGeneric(tileMap.Tiles);
                });

                _renderer.RenderObjectsInstancedGeneric(scene._units);
                
            }); //GameObjects


            _scenes.ForEach(scene =>
            {
                scene._renderedObjects.ForEach(gameObject =>
                {
                    gameObject.ParticleGenerators.ForEach(generator =>
                    {

                        if (generator.Playing)
                        {
                            _renderer.RenderParticlesInstanced(generator);
                        }
                    });
                });
            }); //Particles

            _scenes.ForEach(scene =>
            {
                scene._text.ForEach(text =>
                {
                    if (text.Render && text.Letters.Count > 0) 
                    {
                        _renderer.RenderLettersInstanced(text.Letters);
                    };
                });
            }); //Text

            _scenes.ForEach(scene =>
            {
                _renderer.RenderUIElements(scene._UI);
            }); //UI



            SwapBuffers();

            base.OnRenderFrame(args);

            Rendering = false;
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            if (!IsFocused) // check to see if the window is focused
            {
                return;
            }
            
            _scenes.ForEach(scene =>
            {
                scene.MouseState = MouseState;
                scene.KeyboardState = KeyboardState;

                scene.onUpdateFrame();
            });

            TickAllObjects();

            var mouse = MouseState;
            float cameraSpeed = 4.0f;
            //float sensitivity = 0.2f;

            // Calculate the offset of the mouse position
            var deltaX = mouse.X - _lastPos.X;
            var deltaY = mouse.Y - _lastPos.Y;

            _cursorObject.SetPosition(new Vector2(_cursorObject.Position.X + deltaX, _cursorObject.Position.Y + deltaY));


            if (_firstMove)
            {
                _lastPos = new Vector2(mouse.X, mouse.Y);
                _firstMove = false;
            }

            //if (mouse.IsButtonDown(MouseButton.Left))
            //{
            //    // Apply the camera pitch and yaw (we clamp the pitch in the camera class)
            //    _camera.Yaw += deltaX * sensitivity;
            //    _camera.Pitch -= deltaY * sensitivity; // reversed since y-coordinates range from bottom to top
            //}

            if (MouseState.ScrollDelta[1] < 0)
            {
                Vector3 movement = _camera.Front * cameraSpeed / 2;
                if (_camera.Position.Z - movement.Z < 26)
                {
                    _camera.Position -= movement; // Backwards
                }
            }
            else if (MouseState.ScrollDelta[1] > 0)
            {
                Vector3 movement = _camera.Front * cameraSpeed / 2;
                if (_camera.Position.Z + movement.Z > 0)
                {
                    _camera.Position += movement; // Forward
                }
            }

            if (KeyboardState.IsKeyDown(Keys.LeftShift))
            {
                //_camera.Position -= _camera.Up * cameraSpeed * (float)args.Time; // Down
                cameraSpeed *= 20;
            }

            _lastPos = new Vector2(mouse.X, mouse.Y);

            if (KeyboardState.IsKeyDown(Keys.W))
            {
                //_camera.Position += _camera.Front * cameraSpeed * (float)args.Time; // Forward
                //_camera.Position += _camera.Up * cameraSpeed * (float)args.Time; // Up
                _camera.Position += Vector3.UnitY * cameraSpeed * (float)args.Time;
            }

            if (KeyboardState.IsKeyDown(Keys.S))
            {
                //_camera.Position -= _camera.Front * cameraSpeed * (float)args.Time; // Backwards
                //_camera.Position -= _camera.Up * cameraSpeed * (float)args.Time; // Down
                _camera.Position -= Vector3.UnitY * cameraSpeed * (float)args.Time;
            }
            if (KeyboardState.IsKeyDown(Keys.A))
            {
                _camera.Position -= _camera.Right * cameraSpeed * (float)args.Time; // Left
            }
            if (KeyboardState.IsKeyDown(Keys.D))
            {
                _camera.Position += _camera.Right * cameraSpeed * (float)args.Time; // Right
            }
            if (KeyboardState.IsKeyDown(Keys.Space))
            {
                //_camera.Position += _camera.Up * cameraSpeed * (float)args.Time; // Up
            }
            

            

            base.OnUpdateFrame(args);
        }

        private void LoadTextures()
        {
            _renderer.LoadTextureFromBaseObject(_cursorObject);

            _renderedItems.ForEach(obj =>
            {
                _renderer.LoadTextureFromBaseObject(obj);
            });

            for (int u = 0; u < _scenes.Count; u++)
            {
                if (_scenes[u].Loaded)
                {
                    for (int i = 0; i < _scenes[u]._renderedObjects.Count; i++)
                    {
                        //_scenes[u]._renderedObjects[i].BaseObjects.ForEach(baseObj => //Load BaseObject textures
                        //{
                        //    _renderer.LoadTextureFromBaseObject(baseObj);
                        //});
                        _renderer.LoadTextureFromGameObj(_scenes[u]._renderedObjects[i]);

                        _scenes[u]._renderedObjects[i].ParticleGenerators.ForEach(particleGen => //Load ParticleGenerator textures
                        {
                            _renderer.LoadTextureFromParticleGen(particleGen);
                        });
                    }

                    _scenes[u]._text.ForEach(text => //Load Text textures
                    {
                        if (text.Letters.Count > 0)
                        {
                            _renderer.LoadTextureFromBaseObject(text.Letters[0].BaseObjects[0], false);
                        }
                    });

                    _scenes[u]._units.ForEach(obj =>
                    {
                        if(obj.Render)
                            _renderer.LoadTextureFromGameObj(obj);
                    });

                    _scenes[u]._tileMaps.ForEach(obj =>
                    {
                        if (obj.Render && obj.Tiles.Count > 0)
                            _renderer.LoadTextureFromGameObj(obj.Tiles[0]);
                    });

                    _scenes[u]._UI.ForEach(obj =>
                    {
                        _renderer.LoadTextureFromUIObject(obj);
                    });
                }
            }
        }

        private void TickAllObjects() 
        {
            if (tick != lastTick)
            {
                lastTick = tick;
                _scenes.ForEach(scene =>
                {
                    scene.Logic();

                    Task renderedObjectTask = new Task(() =>
                    {
                        scene._renderedObjects.ForEach(gameObject =>
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
                        scene._tileMaps.ForEach(tileMap =>
                        {
                            tileMap.Tick();
                        });
                    });

                    Task uiTask = new Task(() =>
                    {
                        scene._UI.ForEach(ui =>
                        {
                            ui.Tick();
                        });
                    });

                    renderedObjectTask.Start();
                    unitTask.Start();
                    tileMapTask.Start();
                    uiTask.Start();
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
        }

        protected override void OnUnload()
        {
            // Unbind all the resources by binding the targets to 0/null.
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.UseProgram(0);

            // Delete all the resources.
            _renderer.ClearData();

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
            lineObject.BaseFrame.Color = color;

            if(camPerspective)
                lineObject.MoveObject(-line1);

            _renderedItems.Add(lineObject);
            LoadTextures();
        }

        private List<GameObject> CheckBoundsForObjects(List<GameObject> listObjects)
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

        #region Event handlers
        private void Window_MouseDown(MouseButtonEventArgs obj)
        {
            _scenes.ForEach(scene =>
            {
                scene.onMouseDown(obj);
            });
        }

        private void Window_MouseMove(MouseMoveEventArgs obj)
        {
            _scenes.ForEach(scene =>
            {
                scene.onMouseMove(obj);
            });
        }

        private void Window_MouseUp(MouseButtonEventArgs obj)
        {
            _scenes.ForEach(scene =>
            {
                scene.onMouseUp(obj);
            });
        }

        private void Window_KeyUp(KeyboardKeyEventArgs obj)
        {
            _scenes.ForEach(scene =>
            {
                scene.onKeyUp(obj);
            });

            //

            switch (obj.Key) 
            {
                case (Keys.Q):
                    _points.Add(NormalizeGlobalCoordinates(new Vector2(_cursorObject.Position.X, _cursorObject.Position.Y), WindowConstants.ClientSize));

                    if(_points.Count > 1)
                    {
                        CreateNewLine(new Vector3(_points[_points.Count - 2].X, _points[_points.Count - 2].Y, 0), new Vector3(_points[_points.Count - 1].X, _points[_points.Count - 1].Y, 0), new Vector4(0, 0, 1, 1), 0.02f, false);
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
                        if(obj.Name == "line") 
                        {
                            indexesToRemove.Add(index);
                        }
                        index++;
                    });

                    for(int i = indexesToRemove.Count - 1; i >= 0; i--)
                    {
                        _renderedItems.RemoveAt(indexesToRemove[i]);
                    }
                    break;
                //case (Keys.U):
                //    CheckBoundsForObjects(_renderedItems, (foundObjs) => 
                //    {
                //        for(int i = 0; i < foundObjs.Count; i++)
                //        {
                //            foundObjs[i].Display.Color = new Vector4(1, 0, 0, 1);
                //            foundObjs[i].Display.ColorProportion = 0.5f;
                //        }
                //    });
                //    break;
                default:
                    break;
            }
        }

        private void Window_KeyDown(KeyboardKeyEventArgs obj) 
        {
            _scenes.ForEach(scene =>
            {
                scene.onKeyDown(obj);
            });
        }
        #endregion
    }
}
