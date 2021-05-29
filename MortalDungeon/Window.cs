﻿using System;
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

        private static Vector3 tempVector = new Vector3();
        public static Vector3 ConvertGlobalToLocalCoordinates(Vector3 position)
        {
            tempVector.X = (position.X / ScreenUnits.X) * 2 - 1;
            tempVector.Y = ((position.Y / ScreenUnits.Y) * 2 - 1) * -1;
            tempVector.Z = position.Z;

            return tempVector;
        }
    }
    public class Window : GameWindow
    {
        private readonly bool DISPLAY_FPS = true;
        private readonly Random _random = new Random();
        private Vector2i WindowSize = new Vector2i();

        private List<Texture> _textures = new List<Texture>();
        private Dictionary<string, int> _loadedTextures = new Dictionary<string, int>();

        BaseObject _cursorObject;

        //private List<BaseObject> _cameraAffectedObjects = new List<BaseObject>();
        //private List<BaseObject> _staticObjects = new List<BaseObject>();
        private List<BaseObject> _renderedItems = new List<BaseObject>();

        private List<Vector2> _points = new List<Vector2>();
        private List<Vector3> _lines = new List<Vector3>();

        private List<BaseObject> _clickableObjects = new List<BaseObject>();

        private Dictionary<int, List<BaseObject>> _animations = new Dictionary<int, List<BaseObject>>(); //uses the frequency as a key and returns a list of objects that need to update


        private List<Scene> _scenes = new List<Scene>();

        private Stopwatch _timer;
        private Stopwatch _gameTimer;

        private int _vertexBufferObject;
        private int _vertexArrayObject;

        private int _elementBufferObject;


        private Camera _camera;
        private MouseRay _mouseRay;
        private bool _firstMove = true;
        private Vector2 _lastPos;


        private int _count = 0;
        private uint tick = 0;
        private uint lastTick = 0;

        private const float tickRate = (float)1 / 60;
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

            //GL flags
            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc((BlendingFactor)BlendingFactorSrc.SrcAlpha, (BlendingFactor)BlendingFactorDest.OneMinusSrcAlpha);
            GL.Enable(EnableCap.ProgramPointSize);

            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Less);

            SetWindowSize();
            _camera = new Camera(Vector3.UnitZ * 3, WindowSize.X / (float)WindowSize.Y);
            _mouseRay = new MouseRay(_camera, WindowSize);

            _cursorObject = new BaseObject(WindowSize, CURSOR_ANIMATION.List, 0, "cursor", new Vector3(MousePosition));
            _cursorObject.LockToWindow = true;
            _cursorObject.BaseFrame.ScaleAll(0.1f);

            //Vector3 button1Position = new Vector3(300, 100, 0);
            ////Vector3 button1Position = _centerScreen;
            //BaseObject button1Object = new BaseObject(WindowSize, BUTTON_ANIMATION.List, 2, "Button One", button1Position, ButtonObjects.BUTTON_SPRITESHEET.Bounds);
            //button1Object.BaseFrame.ScaleAll(0.5f);
            //_clickableObjects.Add(button1Object);


            //Vector3 button2Position = new Vector3(button1Position.X, button1Position.Y + 300, 0);
            //BaseObject button2Object = new BaseObject(WindowSize, BUTTON_ANIMATION.List, 3, "Button Two", button2Position, ButtonObjects.BUTTON_SPRITESHEET.Bounds);
            //button2Object.BaseFrame.ScaleAll(0.5f);
            //_clickableObjects.Add(button2Object);


            //_renderedItems.Add(button1Object);
            //_renderedItems.Add(button2Object);
            Scene menuScene = new MenuScene();
            menuScene.Load(WindowSize, _camera, _cursorObject);
            menuScene.SetCursorDetectionFunc(CheckBoundsForObjects);

            _scenes.Add(menuScene);


            LoadTextures();

            //Define buffers that will be used in rendering
            _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);

            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);
            GL.EnableVertexAttribArray(0);

            _elementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
            GL.EnableVertexAttribArray(1);


            _timer = new Stopwatch();
            _timer.Start();

            _gameTimer = new Stopwatch();
            _gameTimer.Start();

            //Game logic loop
            var gameLoop = new Task(() =>
            {
                int waitTime = (int)(tickRate / 2 * 1000);

                while (true)
                {
                    if (tick != lastTick)
                    {
                        lastTick = tick;

                        _scenes.ForEach(scene =>
                        {
                            scene.Logic();

                            scene._renderedObjects.ForEach(gameObject =>
                            {
                                gameObject.BaseObjects.ForEach(baseObj =>
                                {
                                    baseObj.CurrentAnimation.Tick();
                                });

                                gameObject.ParticleGenerators.ForEach(generator =>
                                {
                                    generator.Tick();
                                });
                            });
                        });
                        
                    }
                    Thread.Sleep(waitTime);
                }
            });
            gameLoop.Start();

            //hides mouse cursor
            CursorGrabbed = true;

            base.OnLoad();
        }

        
        protected override void OnRenderFrame(FrameEventArgs args)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.Clear(ClearBufferMask.DepthBufferBit);

            double timeValue;

            ////FPS
            timeValue = _timer.Elapsed.TotalSeconds;
            _count++;

            if (timeValue > 1 && DISPLAY_FPS)
            {
                Console.WriteLine(_count);
                _timer.Restart();
                _count = 0;
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


            //render points
            for (int i = 0; i < _points.Count; i++)
            {
                Shaders.POINT_SHADER.Use();

                float[] point = new float[] { _points[i].X, _points[i].Y };

                GL.BufferData(BufferTarget.ArrayBuffer, point.Length * sizeof(float), point, BufferUsageHint.DynamicDraw);
                GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, point.Length * sizeof(float), 0);

                GL.DrawArrays(PrimitiveType.Points, 0, 1);
            }
            //render lines

            Shaders.DEFAULT_SHADER.SetMatrix4("camera", cameraMatrix);
            _renderedItems.ForEach(obj =>
            {
                Shaders.DEFAULT_SHADER.Use();
                RenderObject(obj, cameraMatrix);
            });

            //all objects using the default shader are handled here
            Shaders.DEFAULT_SHADER.Use();

            ObjectIDs previousObject = ObjectIDs.Unknown;
            Animation previousFrame = new Animation();
            bool previousCamEnabled = false;
            Vector4 prevColor = default;
            _scenes.ForEach(scene =>
            {
                scene._renderedObjects.ForEach(gameObject =>
                {
                    for (int i = 0; i < gameObject.BaseObjects.Count; i++)
                    {
                        var currDisplay = gameObject.BaseObjects[i].Display;
                        var currAnimation = gameObject.BaseObjects[i].CurrentAnimation;
                        bool setVertexData = !(previousObject == currDisplay.ObjectID && previousObject != ObjectIDs.Unknown); //if we are rerendering the same type of object don't redefine the buffer data
                        bool setTextureData = !(!setVertexData && previousFrame == currAnimation); //if the next item to render is in the same animation state don't redefine texture data
                        bool setCamData = previousCamEnabled != gameObject.BaseObjects[i].BaseFrame.CameraPerspective;
                        bool setColor = gameObject.BaseObjects[i].BaseFrame.Color != prevColor;

                        RenderObject(gameObject.BaseObjects[i], cameraMatrix, setVertexData, setTextureData, setCamData);

                        previousObject = currDisplay.ObjectID;
                        previousFrame = currAnimation;
                        previousCamEnabled = gameObject.BaseObjects[i].BaseFrame.CameraPerspective;
                        prevColor = gameObject.BaseObjects[i].BaseFrame.Color;
                    }
                });
            });

            Shaders.PARTICLE_SHADER.Use();
            bool firstParticle = true;


            _scenes.ForEach(scene =>
            {
                scene._renderedObjects.ForEach(gameObject =>
                {
                    gameObject.ParticleGenerators.ForEach(generator =>
                    {
                        bool freshGenerator = true;
                        if(generator.Playing)
                        generator.Particles.ForEach(particle =>
                        {
                            if (particle.Life > 0)
                            {
                                RenderParticle(generator, particle, cameraMatrix, firstParticle, freshGenerator);

                                firstParticle = false; //all particles will have the same vertex data so we just need to set it once
                                freshGenerator = false; //all particles in a generator will have the same texture
                            }
                        });
                    });
                });
            });
            
            
            Shaders.DEFAULT_SHADER.Use();
            RenderObject(_cursorObject, cameraMatrix);

            SwapBuffers();

            base.OnRenderFrame(args);
        }

        //currently this only works for the default shaders. Any new shaders will need special handling/their own function
        private void RenderObject(BaseObject obj, Matrix4 cameraMatrix, bool setVertexData = true, bool setTextureData = true, bool setCam = true, bool setColor = true) 
        {
            RenderableObject Display = obj.Display;
            if (setVertexData)
            {
                GL.BufferData(BufferTarget.ArrayBuffer, Display.GetVerticesSize(), Display.Vertices, BufferUsageHint.DynamicDraw);
                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Display.Stride, 0);

                GL.BufferData(BufferTarget.ElementArrayBuffer, Display.GetVerticesDrawOrderSize(), Display.VerticesDrawOrder, BufferUsageHint.DynamicDraw);
            }

            if (setTextureData)
            {
                GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, Display.Stride, Display.GetRenderDataOffset());
                obj.Display.TextureReference.Use(TextureUnit.Texture0);
            }

            var transform = Matrix4.Identity;
            transform *= obj.BaseFrame.Rotation;
            transform *= obj.BaseFrame.Scale;
            transform *= obj.BaseFrame.Translation;


            if (setCam) 
            {
                Display.ShaderReference.SetBool("enable_cam", Display.CameraPerspective);
            }
            if (setColor)
            {
                Display.ShaderReference.SetVector4("aColor", obj.BaseFrame.Color);
            }

            Display.ShaderReference.SetMatrix4("transform", transform);
            //Display.ShaderReference.SetFloat("fMixPercent", obj.BaseFrame.ColorProportion);

            GL.DrawElements(PrimitiveType.Triangles, obj.Display.VerticesDrawOrder.Length, DrawElementsType.UnsignedInt, 0);
        }
        private void RenderParticle(ParticleGenerator generator, Particle obj, Matrix4 cameraMatrix, bool setVertexData = true, bool setTextureData = true)
        {
            if (setVertexData)
            {
                GL.BufferData(BufferTarget.ArrayBuffer, generator.ParticleDisplay.GetVerticesSize(), generator.ParticleDisplay.Vertices, BufferUsageHint.DynamicDraw);
                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, generator.ParticleDisplay.Stride, 0);

                GL.BufferData(BufferTarget.ElementArrayBuffer, generator.ParticleDisplay.GetVerticesDrawOrderSize(), generator.ParticleDisplay.VerticesDrawOrder, BufferUsageHint.DynamicDraw);
            }

            if (setTextureData)
            {
                GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, generator.ParticleDisplay.Stride, generator.ParticleDisplay.GetRenderDataOffset());
                generator.ParticleDisplay.TextureReference.Use(TextureUnit.Texture0);
            }

            var transform = Matrix4.Identity;
            //transform *= obj.Rotation;
            transform *= obj.Scale;
            transform *= obj.Translation;
            transform *= cameraMatrix;

            generator.ParticleDisplay.ShaderReference.SetMatrix4("transform", transform);

            generator.ParticleDisplay.ShaderReference.SetVector4("aColor", obj.Color);

            GL.DrawElements(PrimitiveType.TriangleStrip, generator.ParticleDisplay.VerticesDrawOrder.Length, DrawElementsType.UnsignedInt, 0);
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            if (!IsFocused) // check to see if the window is focused
            {
                return;
            }

            if (KeyboardState.IsKeyDown(Keys.Escape))
            {
                Close();
            }

            //if (MouseState.ScrollDelta[1] > 0)
            //{

            //}
            //if (MouseState.ScrollDelta[1] < 0)
            //{

            //}

            _scenes.ForEach(scene =>
            {
                scene.MouseState = MouseState;
                scene.KeyboardState = KeyboardState;

                scene.onUpdateFrame();
            });

            var mouse = MouseState;
            const float cameraSpeed = 4.0f;
            const float sensitivity = 0.2f;

            // Calculate the offset of the mouse position
            var deltaX = mouse.X - _lastPos.X;
            var deltaY = mouse.Y - _lastPos.Y;

            _cursorObject.SetPosition(new Vector2(_cursorObject.Position.X + deltaX, _cursorObject.Position.Y + deltaY));


            if (_firstMove)
            {
                _lastPos = new Vector2(mouse.X, mouse.Y);
                _firstMove = false;
            }

            if (mouse.IsButtonDown(MouseButton.Left))
            {
                // Apply the camera pitch and yaw (we clamp the pitch in the camera class)
                _camera.Yaw += deltaX * sensitivity;
                _camera.Pitch -= deltaY * sensitivity; // reversed since y-coordinates range from bottom to top
            }

            _lastPos = new Vector2(mouse.X, mouse.Y);

            if (KeyboardState.IsKeyDown(Keys.W))
            {
                _camera.Position += _camera.Front * cameraSpeed * (float)args.Time; // Forward
            }

            if (KeyboardState.IsKeyDown(Keys.S))
            {
                _camera.Position -= _camera.Front * cameraSpeed * (float)args.Time; // Backwards
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
                _camera.Position += _camera.Up * cameraSpeed * (float)args.Time; // Up
            }
            if (KeyboardState.IsKeyDown(Keys.LeftShift))
            {
                _camera.Position -= _camera.Up * cameraSpeed * (float)args.Time; // Down
            }
            //if (KeyboardState.IsKeyDown(Keys.T))
            //{
            //    CheckBoundsForObjects(_renderedItems).ForEach((foundObj) =>
            //    {
            //        foundObj.Display.Color = new Vector4(1, 0, 0, 0);
            //        foundObj.Display.ColorProportion = 0.5f;
            //        foundObj.MoveObject(Vector3.UnitX * 100);
            //    });
            //}


            base.OnUpdateFrame(args);
        }

        private void LoadTextures() 
        {
            LoadTextureFromBaseObject(_cursorObject);

            _renderedItems.ForEach(obj =>
            {
                LoadTextureFromBaseObject(obj);
            });

            for(int u = 0; u < _scenes.Count; u++)
            {
                if(_scenes[u].Loaded)
                {
                    for (int i = 0; i < _scenes[u]._renderedObjects.Count; i++)
                    {
                        _scenes[u]._renderedObjects[i].BaseObjects.ForEach(baseObj =>
                        {
                            LoadTextureFromBaseObject(baseObj);
                        });

                        _scenes[u]._renderedObjects[i].ParticleGenerators.ForEach(particleGen =>
                        {
                            LoadTextureFromParticleGen(particleGen);
                        });
                    }
                }
            }
        }

        private void LoadTextureFromBaseObject(BaseObject obj)
        {
            foreach(KeyValuePair <AnimationType, Animation> entry in obj.Animations)
            {
                for (int o = 0; o < entry.Value.Frames.Count; o++)
                {
                    for (int p = 0; p < entry.Value.Frames[o].Textures.Textures.Length; p++)
                    {
                        if (!_loadedTextures.TryGetValue(entry.Value.Frames[o].Textures.Textures[p], out int handle))
                        {
                            Texture newTexture = Texture.LoadFromFile(entry.Value.Frames[o].Textures.Textures[p]);
                            _textures.Add(newTexture);
                            _loadedTextures.Add(entry.Value.Frames[o].Textures.Textures[p], newTexture.Handle);

                            entry.Value.Frames[o].TextureReference = newTexture;
                        }
                        else
                        {
                            entry.Value.Frames[o].TextureReference = new Texture(handle);
                        }
                    }
                }
            }
        }
        private void LoadTextureFromParticleGen(ParticleGenerator generator)
        {
            for (int p = 0; p < generator.ParticleDisplay.Textures.Textures.Length; p++)
            {
                if (!_loadedTextures.TryGetValue(generator.ParticleDisplay.Textures.Textures[p], out int handle))
                {
                    Texture newTexture = Texture.LoadFromFile(generator.ParticleDisplay.Textures.Textures[p]);
                    _textures.Add(newTexture);
                    _loadedTextures.Add(generator.ParticleDisplay.Textures.Textures[p], newTexture.Handle);

                    generator.ParticleDisplay.TextureReference = newTexture;
                }
                else
                {
                    generator.ParticleDisplay.TextureReference = new Texture(handle);
                }
            }
        }
        private void SetWindowSize() 
        {
            WindowSize.X = ClientSize.X;
            WindowSize.Y = ClientSize.Y;

            if (_camera != null) 
            {
                _mouseRay._windowSize = WindowSize;
                _cursorObject._windowSize = WindowSize;
                _camera.AspectRatio = WindowSize.X / (float)WindowSize.Y;
            }
        }
        protected override void OnResize(ResizeEventArgs e)
        {
            // When the window gets resized, we have to call GL.Viewport to resize OpenGL's viewport to match the new size.
            // If we don't, the NDC will no longer be correct.
            base.OnResize(e);

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
            GL.DeleteBuffer(_vertexBufferObject);
            GL.DeleteVertexArray(_vertexArrayObject);

            for(int i = 0; i < ShaderList.AllShaders.Count; i++)
            {
                GL.DeleteProgram(ShaderList.AllShaders[i].Handle);
            }

            for (int i = 0; i < _textures.Count; i++)
            {
                GL.DeleteTexture(_textures[i].Handle);
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
            BaseObject lineObject = new BaseObject(WindowSize, new LINE_ANIMATION(lineObj).List, 999, "line", line1);
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
            Vector3 near = _mouseRay.UnProject(_cursorObject.Position.X, _cursorObject.Position.Y, 0, _camera, WindowSize); // start of ray (near plane)
            Vector3 far = _mouseRay.UnProject(_cursorObject.Position.X, _cursorObject.Position.Y, 1, _camera, WindowSize); // end of ray (far plane)

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
            //if (obj.Button == MouseButton.Left && obj.Action == InputAction.Release)
            //{
            //    Vector4 MouseCoordinates = new Vector4(NormalizeGlobalCoordinates(new Vector2(_cursorObject.Position.X, _cursorObject.Position.Y)));

            //    //Console.WriteLine("Mouse Coordinates " + MouseCoordinates.X + ", " + MouseCoordinates.Y);
            //    _clickableObjects.ForEach(o =>
            //    {
            //        if (!o.BaseFrame.CameraPerspective)
            //        {
            //            if (o.Bounds.Contains(new Vector2(MouseCoordinates.X, MouseCoordinates.Y), _camera))
            //            {
            //                Console.WriteLine("Object " + o.Name + " clicked.");

            //                o.Display.Color = new Vector4(1, 0, 0, 0);
            //                o.Display.ColorProportion = 0.5f;

            //                if (o.OnClick != null)
            //                    o.OnClick(o);
            //            }
            //        }
            //    });

            //    CheckBoundsForObjects(_clickableObjects, (foundObjs) =>
            //    {
            //        for(int i = 0; i < foundObjs.Count; i++)
            //        {
            //            foundObjs[i].OnClick?.Invoke(foundObjs[i]);
            //            //foundObjs[i].MoveObject(new Vector3(-200, 0, 0));
            //        }
            //    });
            //}
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
                    _points.Add(NormalizeGlobalCoordinates(new Vector2(_cursorObject.Position.X, _cursorObject.Position.Y), WindowSize));

                    if(_points.Count > 1)
                    {
                        CreateNewLine(new Vector3(_points[_points.Count - 2].X, _points[_points.Count - 2].Y, 0), new Vector3(_points[_points.Count - 1].X, _points[_points.Count - 1].Y, 0), new Vector4(0, 0, 1, 1), 0.02f, false);
                    }

                    var temp = NormalizeGlobalCoordinates(new Vector2(_cursorObject.Position.X, _cursorObject.Position.Y), WindowSize);
                    //Console.WriteLine("Normalized cursor coordinates: " + temp.X + ", " + temp.Y);
                    break;
                case (Keys.R):
                    _points.Clear();
                    _renderedItems.Clear();
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

                    _count = 0;

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
