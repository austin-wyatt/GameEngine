using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using MortalDungeon.Engine_Classes;
using MortalDungeon.Engine_Classes.MiscOperations;
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
    public class Window : GameWindow
    {
        private readonly Vector4 defaultColor = new Vector4();
        private readonly Random _random = new Random();
        private Vector3 _centerScreen;

        private List<Texture> _textures = new List<Texture>();
        private Dictionary<string, int> _loadedTextures = new Dictionary<string, int>();

        BaseObject _cursorObject;

        //private List<BaseObject> _cameraAffectedObjects = new List<BaseObject>();
        //private List<BaseObject> _staticObjects = new List<BaseObject>();
        private List<BaseObject> _renderedItems = new List<BaseObject>();

        private List<Vector2> _points = new List<Vector2>();
        private List<Vector3> _lines = new List<Vector3>();

        private List<BaseObject> _clickableObjects = new List<BaseObject>();


        private List<Scene> sceneList = new List<Scene>();

        private Stopwatch _timer;

        private int _vertexBufferObject;
        private int _vertexArrayObject;

        private int _elementBufferObject;


        private Camera _camera;
        private MouseRay _mouseRay;
        private bool _firstMove = true;
        private Vector2 _lastPos;


        private int _count = 0;
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

            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Less);


            RenderableObject cursorDisplay = new RenderableObject(CursorObjects.MAIN_CURSOR, defaultColor, ObjectRenderType.Texture, Shaders.DEFAULT_SHADER);
            _cursorObject = new BaseObject(ClientSize, cursorDisplay, 0, "cursor", new Vector3(MousePosition));
            _cursorObject.LockToWindow = true;
            _cursorObject.Display.ScaleAll(0.1f);


            Vector3 button1Position = new Vector3(300, 100, 0);
            RenderableObject button1Test = new RenderableObject(TestObjects.TEST_SPRITESHEET, defaultColor, ObjectRenderType.Texture, Shaders.DEFAULT_SHADER);
            BaseObject button1Object = new BaseObject(ClientSize, button1Test, 2, "Button One", button1Position, TestObjects.TEST_SPRITESHEET.Bounds);
            button1Object.Display.ScaleAll(0.5f);
            _clickableObjects.Add(button1Object);
            

            Vector3 button2Position = new Vector3(button1Position.X, button1Position.Y + 300, 0);
            RenderableObject button2Test = new RenderableObject(TestObjects.TEST_SPRITESHEET, defaultColor, ObjectRenderType.Texture, Shaders.DEFAULT_SHADER);
            BaseObject button2Object = new BaseObject(ClientSize, button2Test, 3, "Button Two", button2Position, TestObjects.TEST_SPRITESHEET.Bounds);
            button2Object.Display.ScaleAll(0.5f);
            _clickableObjects.Add(button2Object);



            Vector3 hexagonTilePosition = _centerScreen;

            for (int p = 0; p < 200; p++)
            {
                for (int i = 0; i < 100; i++)
                {
                    RenderableObject hexagonTileDisplay = new RenderableObject(EnvironmentObjects.HEXAGON_TILE, new Vector4((float)_random.NextDouble(), (float)_random.NextDouble(), 0, 1), ObjectRenderType.Texture, Shaders.DEFAULT_SHADER);
                    BaseObject hexagonTileObject = new BaseObject(ClientSize, hexagonTileDisplay, p * 20 + i, "Hexagon " + (p * 20 + i), hexagonTilePosition, EnvironmentObjects.HEXAGON_TILE.Bounds);
                    hexagonTileObject.Display.CameraPerspective = true;
                    hexagonTileObject.Display.ScaleAll(0.5f);
                    hexagonTileObject.Display.ColorProportion = 0f;

                    _clickableObjects.Add(hexagonTileObject);
                    _renderedItems.Add(hexagonTileObject);

                    hexagonTilePosition.X += 150;
                    hexagonTilePosition.Y += 150 * (i % 2 == 0 ? 1 : -1);

                    //if(p == 0 && i == 0)
                    //button1Object.OnClick = (obj) =>
                    //{
                    //    hexagonTileObject.MoveObject(new Vector3(20f, 0, 0));
                    //};
                }
                hexagonTilePosition = _centerScreen;
                hexagonTilePosition.Y -= p * 300;
            }

            _renderedItems.Add(_cursorObject);
            _renderedItems.Add(button1Object);
            _renderedItems.Add(button2Object);

            Console.WriteLine(ButtonObjects.BASIC_BUTTON.Center.X + ", " + ButtonObjects.BASIC_BUTTON.Center.Y);


            LoadTextures();

            //Define buffers that will be used in rendering
            _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);

            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);
            GL.EnableVertexAttribArray(0);

            _elementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);



            sceneList.Add(new MenuScene());

            _camera = new Camera(Vector3.UnitZ * 3, ClientSize.X / (float)ClientSize.Y);
            _mouseRay = new MouseRay(_camera, ClientSize);
            

            _timer = new Stopwatch();
            _timer.Start();

            //Game logic loop
            //var gameLoop = new Task(() =>
            //{
            //    var diff = _timer.ElapsedMilliseconds;
            //    while (true)
            //    {
            //        diff = _timer.ElapsedMilliseconds;
            //        if (diff > 1000)
            //        {
            //            //Console.WriteLine("loop");
            //            //_timer.Restart();
            //        }
            //    }
            //});

            //gameLoop.Start();

            //hides mouse cursor
            CursorGrabbed = true;

            base.OnLoad();
        }

        
        protected override void OnRenderFrame(FrameEventArgs args)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.Clear(ClearBufferMask.DepthBufferBit);

            
            double timeValue = _timer.Elapsed.TotalSeconds;
            _count++;

            if(timeValue > 1)
            {
                Console.WriteLine(_count);
                _timer.Restart();
                _count = 0;
            }

            Matrix4 viewMatrix = _camera.GetViewMatrix();
            Matrix4 projectionMatrix = _camera.GetProjectionMatrix();

            //all objects using the default shader are handled here
            Shaders.DEFAULT_SHADER.Use();
            GL.EnableVertexAttribArray(1);
            for (int i = 0; i < _renderedItems.Count; i++)
            {
                bool setVertexData = !(i > 0 && _renderedItems[i - 1].Display.ObjectID == _renderedItems[i].Display.ObjectID); //if we are rerendering the same type of object don't redefine the buffer data
                bool setTextureData = !(!setVertexData && _renderedItems[i - 1].Display.animationFrame == _renderedItems[i].Display.animationFrame); //if the object is the same and the current texture is the same don't redefine the texture

                RenderObject(_renderedItems[i].Display, ref viewMatrix, ref projectionMatrix, setVertexData, setTextureData);
            }

            for (int i = 0; i < _points.Count; i++)
            {
                Shaders.POINT_SHADER.Use();

                float[] point = new float[]{ _points[i].X, _points[i].Y };

                GL.BufferData(BufferTarget.ArrayBuffer, point.Length * sizeof(float), point, BufferUsageHint.DynamicDraw);
                GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, point.Length * sizeof(float), 0);

                GL.DrawArrays(PrimitiveType.Points, 0, 1);
            }

            //_mouseRay.Update(new Vector2(_cursorObject.Position.X, _cursorObject.Position.Y));

            SwapBuffers();

            base.OnRenderFrame(args);
        }

        //currently this only works for the default shaders. Any new shaders will need special handling/their own function
        private void RenderObject(RenderableObject obj, ref Matrix4 viewMatrix, ref Matrix4 projectionMatrix, bool setVertexData = true, bool setTextureData = true) 
        {
            if (setVertexData)
            {
                GL.BufferData(BufferTarget.ArrayBuffer, obj.GetVerticesSize(), obj.Vertices, BufferUsageHint.DynamicDraw);
                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, obj.Stride, 0);

                GL.BufferData(BufferTarget.ElementArrayBuffer, obj.GetVerticesDrawOrderSize(), obj.VerticesDrawOrder, BufferUsageHint.DynamicDraw);
            }

            if(setTextureData)
            {
                GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, obj.Stride, obj.GetRenderDataOffset());
                obj.TextureReference.Use(TextureUnit.Texture0);
            }

            var transform = Matrix4.Identity;
            transform *= obj.Rotation;
            transform *= obj.Scale;
            transform *= obj.Translation;

            if (obj.CameraPerspective)
            {
                obj.ShaderReference.SetMatrix4("view", viewMatrix);
                obj.ShaderReference.SetMatrix4("projection", projectionMatrix);
            }
            else
            {
                obj.ShaderReference.SetMatrix4("view", Matrix4.Identity);
                obj.ShaderReference.SetMatrix4("projection", Matrix4.Identity);
            }

            obj.ShaderReference.SetMatrix4("transform", transform);
            obj.ShaderReference.SetVector4("aColor", obj.Color);
            obj.ShaderReference.SetFloat("fMixPercent", obj.ColorProportion);


            //GL.BindVertexArray(_vertexArrayObject);

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
            if (KeyboardState.IsKeyDown(Keys.T))
            {
                CheckBoundsForObjects(_renderedItems, (foundObjs) =>
                {
                    for (int i = 0; i < foundObjs.Count; i++)
                    {
                        foundObjs[i].Display.Color = new Vector4(1, 0, 0, 1);
                        foundObjs[i].Display.ColorProportion = 0.5f;
                        foundObjs[i].MoveObject(Vector3.UnitX * 100);
                    }
                });
            }


            base.OnUpdateFrame(args);
        }

        private void LoadTextures() 
        {
            for (int i = 0; i < _renderedItems.Count; i++)
            {
                for (int o = 0; o < _renderedItems[i].Display.Textures.Textures.Length; o++)
                {
                    if (!_loadedTextures.TryGetValue(_renderedItems[i].Display.Textures.Textures[o], out int handle))
                    {
                        Texture newTexture = Texture.LoadFromFile(_renderedItems[i].Display.Textures.Textures[o]);
                        _textures.Add(newTexture);
                        _loadedTextures.Add(_renderedItems[i].Display.Textures.Textures[o], newTexture.Handle);

                        _renderedItems[i].Display.TextureReference = newTexture;
                    }
                    else
                    {
                        _renderedItems[i].Display.TextureReference = new Texture(handle);
                    }
                }
            }
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

        private Vector2 NormalizeGlobalCoordinates(Vector2 vec) 
        {
            float X = (vec.X / ClientSize.X) * 2 - 1; //converts it into local opengl coordinates
            float Y = ((vec.Y / ClientSize.Y) * 2 - 1) * -1; //converts it into local opengl coordinates

            return new Vector2(X, Y);
        }

        private void CreateNewLine(Vector3 line1, Vector3 line2, Vector4 color = new Vector4(), float thickness = 0.01f)
        {
            LineObject lineObj = new LineObject(line1, line2, thickness);

            RenderableObject testLine = new RenderableObject(lineObj.CreateLineDefinition(), color, ObjectRenderType.Texture, Shaders.DEFAULT_SHADER);
            BaseObject lineObject = new BaseObject(ClientSize, testLine, 999, "line", line1);
            lineObject.Display.ColorProportion = 1.0f;
            lineObject.Display.CameraPerspective = true;
            lineObject.MoveObject(-line1);

            _renderedItems.Add(lineObject);
            LoadTextures();
        }

        private void CheckBoundsForObjects(List<BaseObject> listObjects, Action<List<BaseObject>> FoundObjAction)
        {
            Vector3 near = _mouseRay.UnProject(_cursorObject.Position.X, _cursorObject.Position.Y, 0, _camera, ClientSize); // start of ray (near plane)
            Vector3 far = _mouseRay.UnProject(_cursorObject.Position.X, _cursorObject.Position.Y, 1, _camera, ClientSize); // end of ray (far plane)

            List<BaseObject> foundObjects = new List<BaseObject>();

            for(int i = 0; i < listObjects.Count; i++)
            {
                if(listObjects[i].Bounds.Contains3D(near, far, _camera))
                {
                    foundObjects.Add(listObjects[i]);
                }
            }

            FoundObjAction(foundObjects);
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
                Vector4 MouseCoordinates = new Vector4(NormalizeGlobalCoordinates(new Vector2(_cursorObject.Position.X, _cursorObject.Position.Y)));

                //Console.WriteLine("Mouse Coordinates " + MouseCoordinates.X + ", " + MouseCoordinates.Y);
                _clickableObjects.ForEach(o =>
                {
                    if (!o.Display.CameraPerspective)
                    {
                        if (o.Bounds.Contains(new Vector2(MouseCoordinates.X, MouseCoordinates.Y), _camera))
                        {
                            Console.WriteLine("Object " + o.Name + " clicked.");

                            o.Display.Color = new Vector4(1, 0, 0, 0);
                            o.Display.ColorProportion = 0.5f;

                            if (o.OnClick != null)
                                o.OnClick(o);
                        }
                    }
                });

                CheckBoundsForObjects(_clickableObjects, (foundObjs) =>
                {
                    for(int i = 0; i < foundObjs.Count; i++)
                    {
                        foundObjs[i].MoveObject(new Vector3(-200, 0, 0));
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
                case (Keys.I):
                    Vector3 mousePosition = new Vector3(NormalizeGlobalCoordinates(new Vector2(_cursorObject.Position.X, _cursorObject.Position.Y)));

                    var ray = _mouseRay.GetCurrentRay();

                    Vector3 correctCameraPos = _camera.Position;
                    correctCameraPos.X += 1;
                    correctCameraPos.Y -= 1;
                    //correctCameraPos.Z -= 1.5f;

                    Vector3 rayPoint = ray;

                    Vector3 rayTest = (ray * 50) + correctCameraPos;

                    //Console.WriteLine(ray.X * mousePosition.X + ", " + ray.Y * mousePosition.Y + ", " + ray.Z);
                    //Console.WriteLine(_camera.Front.X + ", " + _camera.Front.Y + ", " + _camera.Front.Z);
                    //new Vector3(0,0,0)

                    //CreateNewLine(new Vector3(0, 0, 0), cameraViewPosition, new Vector4(1, 0, 0, 1), 0.05f);
                    //CreateNewLine(new Vector3(0, 0, 0), _camera.Position, new Vector4(0, 0, 1, 1), 0.05f);
                    //CreateNewLine(correctCameraPos, rayTest, new Vector4(1, 0, 1, 1), 0.05f);

                    Console.WriteLine(correctCameraPos);

                    CreateNewLine(correctCameraPos, rayTest, new Vector4(1, 0, 1, 1), 0.05f);
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
                case (Keys.U):
                    //Vector3 near = _mouseRay.UnProject(_cursorObject.Position.X, _cursorObject.Position.Y, 0, _camera, ClientSize); // start of ray (near plane)
                    //Vector3 far = _mouseRay.UnProject(_cursorObject.Position.X, _cursorObject.Position.Y, 1, _camera, ClientSize); // end of ray (far plane)

                    //Vector3 correctCameraPosition = new Vector3(near.X + 1, near.Y - 1, near.Z);

                    //CreateNewLine(correctCameraPosition, far, new Vector4(0,1,0.5f, 1f), 0.1f);

                    CheckBoundsForObjects(_renderedItems, (foundObjs) => 
                    {
                        for(int i = 0; i < foundObjs.Count; i++)
                        {
                            foundObjs[i].Display.Color = new Vector4(1, 0, 0, 1);
                            foundObjs[i].Display.ColorProportion = 0.5f;
                        }
                    });

                    //float xUnit = far.X - near.X;
                    //float yUnit = far.Y - near.Y;
                    //float zUnit = far.Z - near.Z;

                    //float percentageAlongLine = (0 - near.Z) / (far.Z - near.Z);

                    //Vector3 pointAtZ = new Vector3(near.X + xUnit * percentageAlongLine, near.Y + yUnit * percentageAlongLine, near.Z + zUnit * percentageAlongLine);

                    //CreateNewLine(pointAtZ, new Vector3(0, 0, 0), new Vector4(0, 1, 0.5f, 1));

                    break;
                case (Keys.Z):
                    CreateNewLine(_camera.Position, new Vector3(_count, _count, 0), new Vector4(1, 1, 0.5f, 1f), 0.1f);

                    CreateNewLine(new Vector3(_count, _count, 0), _camera.Position, new Vector4(0, 1, 0.5f, 1f), 0.1f);
                    CreateNewLine(new Vector3(_count * 2, _count * 2, 0), new Vector3(), new Vector4(0, 1, 0.5f, 1f), 0.1f);

                    _count--;
                    RenderableObject hexagonTileDisplay = new RenderableObject(EnvironmentObjects.HEXAGON_TILE, new Vector4((float)_random.NextDouble(), (float)_random.NextDouble(), 0, 1), ObjectRenderType.Texture, Shaders.DEFAULT_SHADER);
                    BaseObject hexagonTileObject = new BaseObject(ClientSize, hexagonTileDisplay, 4, "line", new Vector3(), EnvironmentObjects.HEXAGON_TILE.Bounds);
                    hexagonTileObject.Display.CameraPerspective = true;
                    hexagonTileObject.Display.ColorProportion = 1.0f;

                    _renderedItems.Add(hexagonTileObject);
                    LoadTextures();
                    break;
                default:
                    break;
            }
        }
    }
}
