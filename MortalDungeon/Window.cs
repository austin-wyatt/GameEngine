using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
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
        private int _MAX_X;
        private int _MAX_Y;
        const int _MIN_X = 0;
        const int _MIN_Y = 0;

        private readonly Random _random = new Random();

        private static readonly ShaderInfo[] _shaderInfo = {
            new ShaderInfo("Shaders/shader.vert", "Shaders/shader.frag")
        };

        private static readonly Shader[] _shaders = {
            new Shader(_shaderInfo[0].Vertex, _shaderInfo[0].Fragment)
        };

        private List<Texture> _textures = new List<Texture>();
        private Dictionary<string, int> _loadedTextures = new Dictionary<string, int>(); 

        private readonly float[] _vertices =
        {
            0.5f,  0.5f, 0.0f, 1.0f, 1.0f, // top right
             0.5f, -0.5f, 0.0f, 1.0f, 0.0f, // bottom right
            -0.5f, -0.5f, 0.0f, 0.0f, 0.0f, // bottom left
            -0.5f,  0.5f, 0.0f, 0.0f, 1.0f // top left
        };


        //private readonly float[] _verticesTwo =
        //{
        //    0.9f,  0.9f, 0.0f, 1.0f, 1.0f, // top right
        //     0.9f, -0.9f, 0.0f, 1.0f, 0.0f, // bottom right
        //    -0.8f, 0.8f, 0.0f, 0.0f, 0.0f, // bottom left
        //    -0.5f,  0.9f, 0.0f, 0.0f, 1.0f // top left
        //};

        private readonly float[] _verticesTwo =
        {
            0.4f,  0.9f, 0.0f, 1.0f, 0.0f, // top right
             0.4f, 0.4f, 0.0f, 1.0f, 1.0f, // bottom right
            0f, 0.4f, 0.0f, 0.0f, 1.0f, // bottom left
            0f,  0.9f, 0.0f, 0.0f, 0.0f // top left
        };

        private readonly uint[] _indices =
        {
            // Note that indices start at 0!
            0, 1, 3, // The first triangle will be the bottom-right half of the triangle
            1, 2, 3  // Then the second will be the top-right half of the triangle
        };

        private readonly uint[] _indicesTwo =
        {
            // Note that indices start at 0!
            0, 1, 3, // The first triangle will be the bottom-right half of the triangle
            1, 2, 3  // Then the second will be the top-right half of the triangle
        };

        private ShaderInfo defaultShaders = new ShaderInfo
        {
            Vertex = "Shaders/shader.vert",
            Fragment = "Shaders/shader.frag"
        };

        private RenderableObject _testObject;

        private List<RenderableObject> _objectList = new List<RenderableObject>();

        private Stopwatch _timer;

        private int _vertexBufferObject;
        private int _vertexArrayObject;

        private Shader _shader;

        private int _elementBufferObject;


        private Camera _camera;
        private bool _firstMove = true;
        private Vector2 _lastPos;


        public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings) { }

        protected override void OnLoad()
        {
            //Set listeners
            MouseDown += Window_MouseDown;



            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
            _MAX_X = Size.X;
            _MAX_Y = Size.Y;

            //GL.Enable(EnableCap.DepthTest);

            _testObject = new RenderableObject(_vertices, _indices, 4, "Resources/container.png", new float[0], ObjectRenderType.Texture, _shaders[0]);

            _objectList.Add(_testObject);
            _objectList.Add(new RenderableObject(_verticesTwo, _indicesTwo, 4, "Resources/stonks.png", new float[0], ObjectRenderType.Texture, _shaders[0]));

            for (int i = 0; i < 20; i++) 
            {
                _objectList.Add(new RenderableObject(_verticesTwo, _indicesTwo, 4, "Resources/stonks.png", new float[0], ObjectRenderType.Texture, _shaders[0]));
            }

            //Load textures (if necessary) for all objects in the _objectList and assign handles
            for (int i = 0; i < _objectList.Count; i++) 
            {
                if (!_loadedTextures.TryGetValue(_objectList[i].Texture, out int handle))
                {
                    Texture newTexture = Texture.LoadFromFile(_objectList[i].Texture);
                    _textures.Add(newTexture);
                    _loadedTextures.Add(_objectList[i].Texture, newTexture.Handle);

                    _objectList[i].TextureReference = newTexture;
                }
                else
                {
                    _objectList[i].TextureReference = new Texture(handle);
                }
            }


            _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _testObject.GetVerticesSize(), _testObject.Vertices, BufferUsageHint.StaticDraw);

            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, _testObject.Stride, 0);
            GL.EnableVertexAttribArray(0);

            //GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
            //GL.EnableVertexAttribArray(1);

            _elementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _testObject.GetVerticesDrawOrderSize(), _testObject.VerticesDrawOrder, BufferUsageHint.StaticDraw);



            _shader = _shaders[0];
            _shaders[0].Use();



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
                        Console.WriteLine("loop");
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


            for(int i = 0; i < _objectList.Count; i++) 
            {
                GL.BufferData(BufferTarget.ArrayBuffer, _objectList[i].GetVerticesSize(), _objectList[i].Vertices, BufferUsageHint.DynamicDraw);
                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, _objectList[i].Stride, 0);

                GL.BufferData(BufferTarget.ElementArrayBuffer, _objectList[i].GetVerticesDrawOrderSize(), _objectList[i].VerticesDrawOrder, BufferUsageHint.DynamicDraw);



                _objectList[i].ShaderReference.Use();

                _objectList[i].TextureReference.Use(TextureUnit.Texture0);

                var texCoordLocation = _objectList[i].ShaderReference.GetAttribLocation("aTexCoord");
                GL.EnableVertexAttribArray(texCoordLocation);
                GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, _objectList[i].Stride, _objectList[i].GetRenderDataOffset());

                var transform = Matrix4.Identity;
                transform *= _objectList[i].Rotation;
                transform *= _objectList[i].Scale;
                transform *= _objectList[i].Translation;

                //transform = transform * _view * _projection;

                _objectList[i].ShaderReference.SetMatrix4("transform", transform);
                _objectList[i].ShaderReference.SetMatrix4("view", _camera.GetViewMatrix());
                _objectList[i].ShaderReference.SetMatrix4("projection", _camera.GetProjectionMatrix());


                GL.BindVertexArray(_vertexArrayObject);

                GL.DrawElements(PrimitiveType.Triangles, _objectList[i].VerticesDrawOrder.Length, DrawElementsType.UnsignedInt, 0);
            }



            SwapBuffers();

            base.OnRenderFrame(args);
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

            //if (KeyboardState.IsKeyDown(Keys.Right))
            //{
            //    _objectList[1].TranslateX(0.01f);
            //}
            //else if (KeyboardState.IsKeyDown(Keys.Left))
            //{
            //    _objectList[1].TranslateX(-0.01f);
            //}

            if (MouseState.ScrollDelta[1] > 0)
            {
                //_objectList[1].ScaleAll(1.1f);
                //Vector3 viewTranslation = _view.ExtractTranslation();
                //viewTranslation[2] += 0.1f;

                //_view = _view.ClearTranslation() * Matrix4.CreateTranslation(viewTranslation);
            }
            if (MouseState.ScrollDelta[1] < 0)
            {
                //_objectList[1].ScaleAll(0.9f);
                //Vector3 viewTranslation = _view.ExtractTranslation();
                //viewTranslation[2] -= 0.1f;

                //_view = _view.ClearTranslation() * Matrix4.CreateTranslation(viewTranslation);
            }


            //if (KeyboardState.IsKeyDown(Keys.A))
            //{
            //    for(int i = 0; i < _objectList.Count; i++) 
            //    {
            //        _objectList[i].TranslateX((float)_random.NextDouble() / 100 * -1);
            //    }
            //}


            var mouse = MouseState;
            const float cameraSpeed = 1.5f;
            const float sensitivity = 0.2f;

            if (_firstMove)
            {
                _lastPos = new Vector2(mouse.X, mouse.Y);
                _firstMove = false;
            }
            else
            {
                // Calculate the offset of the mouse position
                var deltaX = mouse.X - _lastPos.X;
                var deltaY = mouse.Y - _lastPos.Y;
                _lastPos = new Vector2(mouse.X, mouse.Y);

                // Apply the camera pitch and yaw (we clamp the pitch in the camera class)
                _camera.Yaw += deltaX * sensitivity;
                _camera.Pitch -= deltaY * sensitivity; // reversed since y-coordinates range from bottom to top
                
            }

            if (mouse.IsButtonDown(MouseButton.Left))
            {
            }
            else 
            {
            }


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



            base.OnUpdateFrame(args);
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            // When the window gets resized, we have to call GL.Viewport to resize OpenGL's viewport to match the new size.
            // If we don't, the NDC will no longer be correct.
            _MAX_X = Size.X;
            _MAX_Y = Size.Y;
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
            
            base.OnUnload();
        }



        private List<Action> _mouseDownActions = new List<Action>(); //add conditions inside of the passed function maybe?
        private void Window_MouseDown(MouseButtonEventArgs obj)
        {
            _mouseDownActions.ForEach(a =>
            {
                a.Invoke();
            });

            throw new NotImplementedException();
        }
    }
}
