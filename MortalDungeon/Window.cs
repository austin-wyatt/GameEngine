using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using MortalDungeon.Engine_Classes;
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
    public class Window : GameWindow
    {
        private readonly Vector4 defaultColor = new Vector4();
        private readonly Random _random = new Random();
        private Vector3 _centerScreen;

        private static readonly ShaderInfo defaultShaders = new ShaderInfo
        {
            Vertex = "Shaders/shader.vert",
            Fragment = "Shaders/shader.frag"
        };

        private static readonly ShaderInfo pointShader = new ShaderInfo
        {
            Vertex = "Shaders/pointShader.vert",
            Fragment = "Shaders/pointShader.frag"
        };

        private static readonly ShaderInfo[] _shaderInfo = {
            defaultShaders,
            pointShader
        };

        private static readonly Shader[] _shaders = {
            new Shader(_shaderInfo[0].Vertex, _shaderInfo[0].Fragment),
            new Shader(_shaderInfo[1].Vertex, _shaderInfo[1].Fragment)
        };

        private List<Texture> _textures = new List<Texture>();
        private Dictionary<string, int> _loadedTextures = new Dictionary<string, int>();

        BaseObject _cursorObject;

        

        private RenderableObject _testObject;

        //private List<BaseObject> _cameraAffectedObjects = new List<BaseObject>();
        //private List<BaseObject> _staticObjects = new List<BaseObject>();
        private List<BaseObject> _renderedItems = new List<BaseObject>();

        private List<Vector2> _points = new List<Vector2>();

        private List<BaseObject> _clickableObjects = new List<BaseObject>();




        private Stopwatch _timer;

        private int _vertexBufferObject;
        private int _vertexArrayObject;

        private int _elementBufferObject;


        private Camera _camera;
        private bool _firstMove = true;
        private Vector2 _lastPos;


        public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings) 
        {
            _centerScreen = new Vector3(ClientSize.X / 2, ClientSize.Y / 2, 0); //use to outline bounds
        }

        protected override void OnLoad()
        {
            //Set listeners
            MouseDown += Window_MouseDown;
            MouseMove += Window_MouseMove;
            MouseUp += Window_MouseUp;
            KeyUp += Window_KeyUp;

            //Clear color
            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc((BlendingFactor)BlendingFactorSrc.SrcAlpha, (BlendingFactor)BlendingFactorDest.OneMinusSrcAlpha);
            GL.Enable(EnableCap.ProgramPointSize);

            //GL.Enable(EnableCap.DepthTest);

            RenderableObject cursorDisplay = new RenderableObject(CursorObjects.MAIN_CURSOR, defaultColor, ObjectRenderType.Texture, _shaders[0]);
            _cursorObject = new BaseObject(ClientSize, cursorDisplay, 0, "cursor", new Vector3(MousePosition));
            _cursorObject.LockToWindow = true;
            _cursorObject.Display.ScaleAll(0.1f);


            Vector3 button1Position = new Vector3(300, 100, 0);
            RenderableObject button1Test = new RenderableObject(ButtonObjects.BASIC_BUTTON, defaultColor, ObjectRenderType.Texture, _shaders[0]);
            BaseObject button1Object = new BaseObject(ClientSize, button1Test, 2, "Button One", button1Position, ButtonObjects.BASIC_BUTTON.Bounds);
            button1Object.Display.ScaleAll(0.2f);
            //_staticObjects.Add(button1Object);
            _clickableObjects.Add(button1Object);
            

            Vector3 button2Position = new Vector3(button1Position.X, button1Position.Y + 100, 0);
            RenderableObject button2Test = new RenderableObject(ButtonObjects.BASIC_BUTTON, defaultColor, ObjectRenderType.Texture, _shaders[0]);
            BaseObject button2Object = new BaseObject(ClientSize, button2Test, 3, "Button Two", button2Position, ButtonObjects.BASIC_BUTTON.Bounds);
            button2Object.Display.ScaleAll(0.2f);
            //_staticObjects.Add(button2Object);
            _clickableObjects.Add(button2Object);



            //Vector3 tree1Position = new Vector3(6000, ClientSize.Y / 2, 0);
            //RenderableObject tree1Display = new RenderableObject(EnvironmentObjects.TREE1, new float[0], ObjectRenderType.Texture, _shaders[0]);
            //BaseObject tree1Object = new BaseObject(ClientSize, tree1Display, 4, "Tree One", tree1Position, EnvironmentObjects.TREE1.Bounds);
            //tree1Object.Display.ScaleAll(0.5f);


            //button2Object.OnClick = (obj) =>
            //{
            //    tree1Object.Display.RotateX(10);
            //};


            //_cameraAffectedObjects.Add(tree1Object);
            //_clickableObjects.Add(tree1Object);
            //_renderedItems.Add(tree1Object);

            Vector3 hexagonTilePosition = _centerScreen;

            for (int p = 0; p < 1; p++)
            {
                for (int i = 0; i < 1; i++)
                {
                    RenderableObject hexagonTileDisplay = new RenderableObject(EnvironmentObjects.OCTAGON_TILE_TEST, new Vector4((float)_random.NextDouble(), (float)_random.NextDouble(), 0, 1), ObjectRenderType.Texture, _shaders[0]);
                    BaseObject hexagonTileObject = new BaseObject(ClientSize, hexagonTileDisplay, 4, "Hexagon " + (p * 15 + i), hexagonTilePosition, EnvironmentObjects.OCTAGON_TILE_TEST.Bounds);
                    hexagonTileObject.Display.CameraPerspective = false;
                    //hexagonTileObject.Display.ScaleAll(0.5f);
                    hexagonTileObject.Display.ColorProportion = 0.5f;

                    _clickableObjects.Add(hexagonTileObject);

                    _renderedItems.Add(hexagonTileObject);

                    hexagonTilePosition.X += 150;
                    hexagonTilePosition.Y += 150 * (i % 2 == 0 ? 1 : -1);
                    //if (i % 2 == 1)
                    //{
                    //    hexagonTileObject.Display.RotateZ(30 * p);
                    //}
                    //else
                    //{
                    //    hexagonTileObject.Display.RotateZ(30 * p * 2);
                    //}

                    button1Object.OnClick = (obj) =>
                    {
                        hexagonTileObject.MoveObject(new Vector3(20f, 0, 0));
                    };

                    button2Object.OnClick = (obj) =>
                    {
                        hexagonTileObject.MoveObject(new Vector3(-20f, 0, 0));
                        hexagonTileObject.Display.Color = new Vector4((float)_random.NextDouble(), (float)_random.NextDouble(), (float)_random.NextDouble(), (float)_random.NextDouble());
                    };
                }
                hexagonTilePosition = _centerScreen;
                hexagonTilePosition.Y -= p * 300;
            }

            


            _renderedItems.Add(button1Object);
            _renderedItems.Add(_cursorObject);
            _renderedItems.Add(button2Object);

            Console.WriteLine(ButtonObjects.BASIC_BUTTON.Center.X + ", " + ButtonObjects.BASIC_BUTTON.Center.Y);

            //_testObject = new RenderableObject(TestObjects.BASIC_SQUARE, new float[0], ObjectRenderType.Texture, _shaders[0]);
            //BaseObject baseObject = new BaseObject(ClientSize, _testObject, 1, "test", new Vector3(600, 300, 0), TestObjects.BASIC_SQUARE.Bounds);

            //_cameraAffectedObjects.Add(baseObject);
            //_renderedItems.Add(baseObject);
            //_clickableObjects.Add(baseObject);

            //_objectList.Add(new RenderableObject(_verticesTwo, _indicesTwo, 4, "Resources/stonks.png", new float[0], ObjectRenderType.Texture, _shaders[0]));

            //Load textures (if necessary) for all objects in the _objectList and assign handles
            for (int i = 0; i < _renderedItems.Count; i++) 
            {
                for(int o = 0; o < _renderedItems[i].Display.Textures.Length; o++)
                {
                    if (!_loadedTextures.TryGetValue(_renderedItems[i].Display.Textures[o], out int handle))
                    {
                        Texture newTexture = Texture.LoadFromFile(_renderedItems[i].Display.Textures[o]);
                        _textures.Add(newTexture);
                        _loadedTextures.Add(_renderedItems[i].Display.Textures[o], newTexture.Handle);

                        _renderedItems[i].Display.TextureReference = newTexture;
                    }
                    else
                    {
                        _renderedItems[i].Display.TextureReference = new Texture(handle);
                    }
                }
                
            }

            //Define buffers that will be used in rendering
            _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);

            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);
            GL.EnableVertexAttribArray(0);

            _elementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);





            _camera = new Camera(Vector3.UnitZ * 3, Size.X / (float)Size.Y);

            

            _timer = new Stopwatch();
            _timer.Start();

            //Game logic loop
            var gameLoop = new Task(() =>
            {
                var diff = _timer.ElapsedMilliseconds;
                while (true)
                {
                    diff = _timer.ElapsedMilliseconds;
                    if (diff > 1000)
                    {
                        //Console.WriteLine("loop");
                        _timer.Restart();
                    }
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


            double timeValue = _timer.Elapsed.TotalSeconds;


            //for(int i = 0; i < _cameraAffectedObjects.Count; i++) 
            //{
            //    RenderableObject obj = _cameraAffectedObjects[i].Display;
            //    RenderObject(obj);
            //}

            //for (int i = 0; i < _staticObjects.Count; i++)
            //{
            //    RenderableObject staticObject = _staticObjects[i].Display;
            //    RenderObject(staticObject);
            //}

            for (int i = 0; i < _renderedItems.Count; i++)
            {
                RenderableObject renderedItem = _renderedItems[i].Display;
                RenderObject(renderedItem);
            }

            for (int i = 0; i < _points.Count; i++)
            {
                _shaders[1].Use();

                float[] point = new float[]{ _points[i].X, _points[i].Y };

                GL.BufferData(BufferTarget.ArrayBuffer, point.Length * sizeof(float), point, BufferUsageHint.DynamicDraw);
                GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, point.Length * sizeof(float), 0);

                //Console.WriteLine(point[0] + ", " + point[1]);

                GL.DrawArrays(PrimitiveType.Points, 0, 2);
            }

            RenderObject(_cursorObject.Display);

            SwapBuffers();

            base.OnRenderFrame(args);
        }

        private void RenderObject(RenderableObject obj) 
        {
            GL.BufferData(BufferTarget.ArrayBuffer, obj.GetVerticesSize(), obj.Vertices, BufferUsageHint.DynamicDraw);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, obj.Stride, 0);

            GL.BufferData(BufferTarget.ElementArrayBuffer, obj.GetVerticesDrawOrderSize(), obj.VerticesDrawOrder, BufferUsageHint.DynamicDraw);



            obj.ShaderReference.Use();

            obj.TextureReference.Use(TextureUnit.Texture0);

            var texCoordLocation = obj.ShaderReference.GetAttribLocation("aTexCoord");
            GL.EnableVertexAttribArray(texCoordLocation);
            GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, obj.Stride, obj.GetRenderDataOffset());

            var transform = Matrix4.Identity;
            transform *= obj.Rotation;
            transform *= obj.Scale;
            transform *= obj.Translation;

            obj.ShaderReference.SetMatrix4("transform", transform);

            if (obj.CameraPerspective) 
            {
                obj.ShaderReference.SetMatrix4("view", _camera.GetViewMatrix());
                obj.ShaderReference.SetMatrix4("projection", _camera.GetProjectionMatrix());
                
            }
            else
            {
                obj.ShaderReference.SetMatrix4("view", Matrix4.Identity);
                obj.ShaderReference.SetMatrix4("projection", Matrix4.Identity);
            }

            obj.ShaderReference.SetVector4("aColor", obj.Color);
            obj.ShaderReference.SetFloat("fMixPercent", obj.ColorProportion);


            GL.BindVertexArray(_vertexArrayObject);

            GL.DrawElements(PrimitiveType.Triangles, obj.VerticesDrawOrder.Length, DrawElementsType.UnsignedInt, 0);
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

            var mouse = MouseState;
            const float cameraSpeed = 1.5f;
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

            //if(KeyboardState.IsKeyDown(Keys.Q))
            //{
            //    _points.Add(NormalizeGlobalCoordinates(new Vector2(_cursorObject.Position.X, _cursorObject.Position.Y)));
            //    var temp = NormalizeGlobalCoordinates(new Vector2(_cursorObject.Position.X, _cursorObject.Position.Y));
            //    Console.WriteLine("Normalized cursor coordinates: " + temp.X + ", " + temp.Y);
            //}



            base.OnUpdateFrame(args);
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            // When the window gets resized, we have to call GL.Viewport to resize OpenGL's viewport to match the new size.
            // If we don't, the NDC will no longer be correct.
            GL.Viewport(0, 0, Size.X, Size.Y);
            base.OnResize(e);
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

            for(int i = 0; i < _shaders.Length; i++)
            {
                GL.DeleteProgram(_shaders[i].Handle);
            }

            for (int i = 0; i < _textures.Count; i++)
            {
                GL.DeleteTexture(_textures[i].Handle);
            }
            
            base.OnUnload();
        }

        private Vector2 NormalizeGlobalCoordinates(Vector2 vec) 
        {
            float X = (vec.X / ClientSize.X) * 2 - 1; //converts it into local opengl coordinates
            float Y = ((vec.Y / ClientSize.Y) * 2 - 1) * -1; //converts it into local opengl coordinates

            return new Vector2(X, Y);
        }



        private List<Action> _mouseDownActions = new List<Action>();
        private void Window_MouseDown(MouseButtonEventArgs obj)
        {
            _mouseDownActions.ForEach(a =>
            {
                a.Invoke();
            });
        }

        private List<Action> _mouseMoveActions = new List<Action>();
        private void Window_MouseMove(MouseMoveEventArgs obj)
        {
            _mouseMoveActions.ForEach(a =>
            {
                a.Invoke();
            });
        }

        private void Window_MouseUp(MouseButtonEventArgs obj)
        {
            if (obj.Button == MouseButton.Left && obj.Action == InputAction.Release)
            {
                _clickableObjects.ForEach(o =>
                {
                    if (o.Bounds.Contains(NormalizeGlobalCoordinates(new Vector2(_cursorObject.Position.X, _cursorObject.Position.Y))))
                    {
                        Console.WriteLine("Object " + o.Name + " clicked.");

                        if(o.OnClick != null)
                            o.OnClick(o);
                    }
                });
            }
        }

        private void Window_KeyUp(KeyboardKeyEventArgs obj)
        {
            switch (obj.Key) 
            {
                case (Keys.Q):
                    _points.Add(NormalizeGlobalCoordinates(new Vector2(_cursorObject.Position.X, _cursorObject.Position.Y)));
                    var temp = NormalizeGlobalCoordinates(new Vector2(_cursorObject.Position.X, _cursorObject.Position.Y));
                    Console.WriteLine("Normalized cursor coordinates: " + temp.X + ", " + temp.Y);
                    break;
                case (Keys.R):
                    _points.Clear();
                    break;
                case (Keys.P):
                    Console.Write("new float[]{\n");
                    _points.ForEach(p =>
                    {
                        Console.Write(p.X + "f, " + p.Y + "f, 0.0f, \n");
                    });
                    Console.Write("}");
                    break;
                default:
                    break;
            }
        }
    }
}
