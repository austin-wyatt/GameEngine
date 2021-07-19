using MortalDungeon.Game.Objects;
using MortalDungeon.Game.UI;
using MortalDungeon.Objects;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace MortalDungeon.Engine_Classes
{
    public class Renderer
    {
        Random _rand = new Random();

        public int _vertexBufferObject;
        public int _vertexArrayObject;

        private int _elementBufferObject;

        private int _instancedVertexBuffer;
        private int _instancedArrayBuffer;

        private const int ObjectBufferCount = 10000;
        private const int instanceDataOffset = 24;
        private float[] _instancedRenderArray = new float[ObjectBufferCount * instanceDataOffset];
        private const int instanceDataLength = instanceDataOffset * sizeof(float);

        private List<Texture> _textures = new List<Texture>();
        private Dictionary<string, int> _loadedTextures = new Dictionary<string, int>();


        public int DrawCount = 0;
        public int FPSCount = 0;
        public bool DisplayFPS = true;
        public Stopwatch _internalTimer = new Stopwatch();

        public Renderer() { }
        public void Load() //initialization of renderer
        {
            GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc((BlendingFactor)BlendingFactorSrc.SrcAlpha, (BlendingFactor)BlendingFactorDest.OneMinusSrcAlpha);
            GL.Enable(EnableCap.ProgramPointSize);

            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Less);


            _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);

            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);

            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);


            _elementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);

            _instancedArrayBuffer = GL.GenBuffer();
            _instancedVertexBuffer = GL.GenBuffer();

            _internalTimer.Start();
        }

        //currently this only works for the default shaders. Any new shaders will need special handling/their own function
        public void RenderObject(BaseObject obj, bool setVertexData = true, bool setTextureData = true, bool setCam = true, bool setColor = true)
        {
            RenderableObject Display = obj._currentAnimation.CurrentFrame;
            if (setVertexData)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
                GL.BufferData(BufferTarget.ArrayBuffer, Display.GetVerticesSize(), Display.Vertices, BufferUsageHint.StreamDraw);
                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Display.Stride, 0);

                GL.BufferData(BufferTarget.ElementArrayBuffer, Display.GetVerticesDrawOrderSize(), Display.VerticesDrawOrder, BufferUsageHint.StreamDraw);
            }

            if (setTextureData)
            {
                GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, Display.Stride, Display.GetRenderDataOffset());
                Display.TextureReference.Use(TextureUnit.Texture0);
            }

            var transform = obj._baseFrame.Transformations;
            //var transform = Matrix4.Identity;
            //transform *= obj._baseFrame.Rotation;
            //transform *= obj._baseFrame.Scale;
            //transform *= obj._baseFrame.Translation;


            if (setCam)
            {
                Display.ShaderReference.SetBool("enable_cam", Display.CameraPerspective);
            }
            if (setColor)
            {
                Display.ShaderReference.SetVector4("aColor", obj._baseFrame.Color);
            }

            Display.ShaderReference.SetMatrix4("transform", transform);

            //Display.ShaderReference.SetFloat("fMixPercent", obj._baseFrame.ColorProportion);

            GL.DrawElements(PrimitiveType.Triangles, Display.VerticesDrawOrder.Length, DrawElementsType.UnsignedInt, 0);

            DrawCount++;
        }

        public void RenderObjectsInstanced(List<GameObject> objects) //this should be called separately for each texture that needs to be accessed
        {
            if (objects.Count == 0)
                return;

            RenderableObject Display = objects[0].BaseObjects[0]._currentAnimation.CurrentFrame;
            TextureName currTexture = Display.Textures.Textures[0];

            Display.TextureReference.Use(TextureUnit.Texture0);
            EnableInstancedShaderAttributes();

            GL.BindBuffer(BufferTarget.ArrayBuffer, _instancedVertexBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, Display.Vertices.Length * sizeof(float), Display.Vertices, BufferUsageHint.StreamDraw);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Display.Stride, 0); //vertex data
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, Display.Stride, 3 * sizeof(float)); //Texture coordinate data, change this to be instanced data instead and change spritesheet object definitions to compensate

            GL.BindBuffer(BufferTarget.ArrayBuffer, _instancedArrayBuffer);

            GL.VertexAttribPointer(2, 4, VertexAttribPointerType.Float, false, instanceDataLength, 0);                  //| Transformation matrix data
            GL.VertexAttribPointer(3, 4, VertexAttribPointerType.Float, false, instanceDataLength, 4 * sizeof(float));  //|
            GL.VertexAttribPointer(4, 4, VertexAttribPointerType.Float, false, instanceDataLength, 8 * sizeof(float));  //|
            GL.VertexAttribPointer(5, 4, VertexAttribPointerType.Float, false, instanceDataLength, 12 * sizeof(float)); //|

            GL.VertexAttribPointer(6, 4, VertexAttribPointerType.Float, false, instanceDataLength, 16 * sizeof(float)); //Color data
            GL.VertexAttribPointer(7, 4, VertexAttribPointerType.Float, false, instanceDataLength, 20 * sizeof(float)); //Camera enabled + spritesheet position + side lengths X and Y

            int currIndex = 0;

            int count = 0;
            List<GameObject> recursiveCallList = new List<GameObject>();

            objects.ForEach(objG => //all base objects inside of a game object should use the same spritesheet. 
            {
                objG.BaseObjects.ForEach(obj =>
                {
                    if(obj.Render)

                    if (obj._currentAnimation.CurrentFrame.Textures.Textures[0] != currTexture || count == ObjectBufferCount)
                    {
                        recursiveCallList.Add(objG);
                    }
                    else
                    {
                        var transform = obj._baseFrame.Transformations;


                        InsertMatrixDataIntoArray(ref _instancedRenderArray, ref transform, currIndex);
                        currIndex += 16;
                        _instancedRenderArray[currIndex++] = obj._baseFrame.Color.X;
                        _instancedRenderArray[currIndex++] = obj._baseFrame.Color.Y;
                        _instancedRenderArray[currIndex++] = obj._baseFrame.Color.Z;
                        _instancedRenderArray[currIndex++] = obj._baseFrame.Color.W;

                        _instancedRenderArray[currIndex++] = obj._baseFrame.CameraPerspective ? 1.0f : 0.0f;
                        _instancedRenderArray[currIndex++] = obj._currentAnimation.CurrentFrame.SpritesheetPosition;
                        _instancedRenderArray[currIndex++] = obj._currentAnimation.CurrentFrame.SideLengths.X;
                        _instancedRenderArray[currIndex++] = obj._currentAnimation.CurrentFrame.SideLengths.Y;

                        count++;
                    }
                });
            });

            GL.BufferData(BufferTarget.ArrayBuffer, count * instanceDataOffset * sizeof(float), _instancedRenderArray, BufferUsageHint.StreamDraw);

            GL.DrawArraysInstanced(PrimitiveType.TriangleFan, 0, 4, count);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            DisableInstancedShaderAttributes();

            if (recursiveCallList.Count > 0)
            {
                RenderObjectsInstanced(recursiveCallList);
            }
            DrawCount++;
        }

        private float[] tempVertices = new float[20];
        public void RenderLettersInstanced(List<Letter> objects, RenderableObject display = null, bool shiftingText = false) //same as RenderObjectsInstanced but uses a list of Letter objects
        {
            RenderableObject Display = null;
            if (display != null) 
            {
                Display = display;
            }
            else 
            {
                Display = objects[0].BaseObjects[0]._currentAnimation.CurrentFrame;

            }
            TextureName currTexture = Display.Textures.Textures[0];

            Display.TextureReference.Use(TextureUnit.Texture0);
            EnableInstancedShaderAttributes();

            if (!Display.CameraPerspective && false)  
            {
                float aspectRatio = (float)WindowConstants.ClientSize.X / WindowConstants.ClientSize.Y;
                int j = 0; //used to avoid using modulo
                for (int i = 0; i < Display.Vertices.Length; i++) 
                {
                    if (j == 1)
                    {
                        tempVertices[i] = Display.Vertices[i] / aspectRatio;
                    }
                    else if (j == 2)
                    {
                        j = -1;
                        tempVertices[i] = Display.Vertices[i];
                    }
                    else 
                    {
                        tempVertices[i] = Display.Vertices[i];
                    }

                    j++;
                }
            }//this code makes interestingly styled text, save for later
            if (!Display.CameraPerspective && false)
            {
                float aspectRatio = (float)WindowConstants.ClientSize.X / WindowConstants.ClientSize.Y;
                int j = 0; //used to avoid using modulo
                for (int i = 0; i < Display.Vertices.Length; i++)
                {
                    if (j == 1)
                    {
                        tempVertices[i] = Display.Vertices[i] * 0.5f;
                    }
                    else if (j == 2)
                    {
                        j = -1;
                        tempVertices[i] = Display.Vertices[i];
                    }
                    else
                    {
                        tempVertices[i] = Display.Vertices[i] * 0.5f;
                    }

                    j++;
                }
            }//cartoony style text
            if (!Display.CameraPerspective && false)
            {
                float aspectRatio = (float)WindowConstants.ClientSize.X / WindowConstants.ClientSize.Y;
                int j = 0; //used to avoid using modulo
                for (int i = 0; i < Display.Vertices.Length; i++)
                {
                    if (j == 1)
                    {
                        tempVertices[i] = Display.Vertices[i];
                    }
                    else if (j == 5)
                    {
                        j = -1;
                        tempVertices[i] = Display.Vertices[i];
                    }
                    else
                    {
                        tempVertices[i] = Display.Vertices[i] * 0.5f;
                    }

                    j++;
                }
            }//actually good looking font
            if (_internalTimer.ElapsedMilliseconds > 500 && shiftingText)
            {
                int j = 0; //used to avoid using modulo
                for (int i = 0; i < Display.Vertices.Length; i++)
                {
                    if (j == 1)
                    {
                        tempVertices[i] = Display.Vertices[i] * (0.5f + (float)_rand.NextDouble() / 2);
                    }
                    else if (j == 5)
                    {
                        j = -1;
                        tempVertices[i] = Display.Vertices[i];
                    }
                    else
                    {
                        tempVertices[i] = Display.Vertices[i] * (0.5f + (float)_rand.NextDouble() / 2);
                    }

                    j++;
                }
                _internalTimer.Restart();
            }//shifting text

            GL.BindBuffer(BufferTarget.ArrayBuffer, _instancedVertexBuffer);
            if(shiftingText)
                GL.BufferData(BufferTarget.ArrayBuffer, Display.Vertices.Length * sizeof(float), tempVertices, BufferUsageHint.StreamDraw);
            else
                GL.BufferData(BufferTarget.ArrayBuffer, Display.Vertices.Length * sizeof(float), Display.Vertices, BufferUsageHint.StreamDraw);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Display.Stride, 0); //vertex data
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, Display.Stride, 3 * sizeof(float)); //Texture coordinate data, change this to be instanced data instead and change spritesheet object definitions to compensate

            GL.BindBuffer(BufferTarget.ArrayBuffer, _instancedArrayBuffer);

            GL.VertexAttribPointer(2, 4, VertexAttribPointerType.Float, false, instanceDataLength, 0);                  //| Transformation matrix data
            GL.VertexAttribPointer(3, 4, VertexAttribPointerType.Float, false, instanceDataLength, 4 * sizeof(float));  //|
            GL.VertexAttribPointer(4, 4, VertexAttribPointerType.Float, false, instanceDataLength, 8 * sizeof(float));  //|
            GL.VertexAttribPointer(5, 4, VertexAttribPointerType.Float, false, instanceDataLength, 12 * sizeof(float)); //|

            GL.VertexAttribPointer(6, 4, VertexAttribPointerType.Float, false, instanceDataLength, 16 * sizeof(float)); //Color data
            GL.VertexAttribPointer(7, 4, VertexAttribPointerType.Float, false, instanceDataLength, 20 * sizeof(float)); //Camera enabled + spritesheet position + side lengths X and Y

            int currIndex = 0;

            int count = 0;
            List<Letter> recursiveCallList = new List<Letter>();


            objects.ForEach(objG => //all base objects inside of a game object should use the same spritesheet. 
            {
                objG.BaseObjects.ForEach(obj =>
                {
                    if (obj.Render)

                    if (obj._currentAnimation.CurrentFrame.Textures.Textures[0] != currTexture || count == ObjectBufferCount)
                    {
                        recursiveCallList.Add(objG);
                    }
                    else
                    {
                        var transform = obj._baseFrame.Transformations;

                        InsertMatrixDataIntoArray(ref _instancedRenderArray, ref transform, currIndex);
                        currIndex += 16;
                        _instancedRenderArray[currIndex++] = obj._baseFrame.Color.X;
                        _instancedRenderArray[currIndex++] = obj._baseFrame.Color.Y;
                        _instancedRenderArray[currIndex++] = obj._baseFrame.Color.Z;
                        _instancedRenderArray[currIndex++] = obj._baseFrame.Color.W;

                        _instancedRenderArray[currIndex++] = obj._baseFrame.CameraPerspective ? 1.0f : 0.0f;
                        _instancedRenderArray[currIndex++] = obj._currentAnimation.CurrentFrame.SpritesheetPosition;
                        _instancedRenderArray[currIndex++] = obj._currentAnimation.CurrentFrame.SideLengths.X;
                        _instancedRenderArray[currIndex++] = obj._currentAnimation.CurrentFrame.SideLengths.Y;

                        count++;
                    }
                });
            });

            GL.BufferData(BufferTarget.ArrayBuffer, count * instanceDataOffset * sizeof(float), _instancedRenderArray, BufferUsageHint.StreamDraw);

            GL.DrawArraysInstanced(PrimitiveType.TriangleFan, 0, 4, count);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            DisableInstancedShaderAttributes();

            if (recursiveCallList.Count > 0)
            {
                RenderLettersInstanced(recursiveCallList, Display);
            }
            DrawCount++;
        }
        public void RenderTextInstanced(List<Text> objects) 
        {
            Shaders.FAST_DEFAULT_SHADER.SetFloat("alpha_threshold", 0.5f);
            objects.ForEach(obj =>
            {
                if(obj.Render)
                    RenderLettersInstanced(obj.Letters);
            });
            Shaders.FAST_DEFAULT_SHADER.SetFloat("alpha_threshold", 0.25f);
        }

        public void RenderObjectsInstancedGeneric<T>(List<T> objects, RenderableObject display = null) where T : GameObject //this should be called separately for each texture that needs to be accessed
        {
            if (objects.Count == 0)
                return;

            RenderableObject Display;

            if (display == null)
            {
                Display = objects[0].BaseObjects[0]._currentAnimation.CurrentFrame;
            }
            else 
            {
                Display = display;
            }
            TextureName currTexture = Display.Textures.Textures[0];

            Display.TextureReference.Use(TextureUnit.Texture0);
            EnableInstancedShaderAttributes();

            if (!Display.CameraPerspective)
            {
                float aspectRatio = (float)WindowConstants.ClientSize.X / WindowConstants.ClientSize.Y;
                int j = 0; //used to avoid using modulo

                for (int i = 0; i < Display.Vertices.Length; i++)
                {
                    if (j == 0)
                    {
                        tempVertices[i] = Display.Vertices[i] / aspectRatio;
                    }
                    else if (j == 2) 
                    {
                        j = -1;
                    }

                    j++;
                }
            }

            GL.BindBuffer(BufferTarget.ArrayBuffer, _instancedVertexBuffer);
            //if (Display.CameraPerspective)
            //{
                GL.BufferData(BufferTarget.ArrayBuffer, Display.Vertices.Length * sizeof(float), Display.Vertices, BufferUsageHint.StreamDraw); //take the raw vertices
            //}
            //else 
            //{
            //    GL.BufferData(BufferTarget.ArrayBuffer, tempVertices.Length * sizeof(float), tempVertices, BufferUsageHint.StreamDraw);
            //}

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Display.Stride, 0); //vertex data
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, Display.Stride, 3 * sizeof(float)); //Texture coordinate data, change this to be instanced data instead and change spritesheet object definitions to compensate

            GL.BindBuffer(BufferTarget.ArrayBuffer, _instancedArrayBuffer);

            GL.VertexAttribPointer(2, 4, VertexAttribPointerType.Float, false, instanceDataLength, 0);                  //| Transformation matrix data
            GL.VertexAttribPointer(3, 4, VertexAttribPointerType.Float, false, instanceDataLength, 4 * sizeof(float));  //|
            GL.VertexAttribPointer(4, 4, VertexAttribPointerType.Float, false, instanceDataLength, 8 * sizeof(float));  //|
            GL.VertexAttribPointer(5, 4, VertexAttribPointerType.Float, false, instanceDataLength, 12 * sizeof(float)); //|

            GL.VertexAttribPointer(6, 4, VertexAttribPointerType.Float, false, instanceDataLength, 16 * sizeof(float)); //Color data
            GL.VertexAttribPointer(7, 4, VertexAttribPointerType.Float, false, instanceDataLength, 20 * sizeof(float)); //Camera enabled + spritesheet position + side lengths X and Y

            int currIndex = 0;

            int count = 0;
            List<T> recursiveCallList = new List<T>();

            float sideLengthX = 0;
            float sideLengthY = 0;
            float spritesheetPosition = 0;
            float cameraPerspective = 0;

            BaseObject obj;
            TextureName texName;

            objects.ForEach(objG => //all base objects inside of a game object should use the same spritesheet. 
            {
                if (objG.Render)
                {
                    for (int i = 0; i < objG.BaseObjects.Count; i++)
                    {
                        if (objG.BaseObjects[i].Render)
                        {
                            obj = objG.BaseObjects[i];
                            texName = obj._currentAnimation.CurrentFrame.Textures.Textures[0];
                            if (count == ObjectBufferCount || texName != currTexture)
                            {
                                recursiveCallList.Add(objG);
                            }
                            else
                            {
                                var transform = obj._baseFrame.Transformations;

                                spritesheetPosition = obj._currentAnimation.CurrentFrame.SpritesheetPosition;
                                sideLengthX = obj._currentAnimation.CurrentFrame.SideLengths.X;
                                sideLengthY = obj._currentAnimation.CurrentFrame.SideLengths.Y;
                                if (obj._baseFrame.CameraPerspective)
                                    cameraPerspective = 1.0f;

                                InsertMatrixDataIntoArray(ref _instancedRenderArray, ref transform, currIndex);
                                currIndex += 16;
                                _instancedRenderArray[currIndex++] = obj._baseFrame.Color.X;
                                _instancedRenderArray[currIndex++] = obj._baseFrame.Color.Y;
                                _instancedRenderArray[currIndex++] = obj._baseFrame.Color.Z;
                                _instancedRenderArray[currIndex++] = obj._baseFrame.Color.W;

                                _instancedRenderArray[currIndex++] = cameraPerspective;
                                _instancedRenderArray[currIndex++] = spritesheetPosition;
                                _instancedRenderArray[currIndex++] = sideLengthX;
                                _instancedRenderArray[currIndex++] = sideLengthY;

                                count++;
                            }
                        }
                    }
                }
            });

            GL.BufferData(BufferTarget.ArrayBuffer, count * instanceDataOffset * sizeof(float), _instancedRenderArray, BufferUsageHint.StreamDraw);

            GL.DrawArraysInstanced(PrimitiveType.TriangleFan, 0, 4, count);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            DisableInstancedShaderAttributes();

            if (recursiveCallList.Count > 0)
            {
                RenderObjectsInstancedGeneric(recursiveCallList, Display);
            }
            DrawCount++;
        }
        public void RenderParticlesInstanced(ParticleGenerator generator)
        {
            if (generator.Particles.Count == 0)
                return;

            RenderableObject Display = generator.ParticleDisplay;
            TextureName currTexture = Display.Textures.Textures[0];

            Display.TextureReference.Use(TextureUnit.Texture0);
            EnableInstancedShaderAttributes();

            GL.BindBuffer(BufferTarget.ArrayBuffer, _instancedVertexBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, Display.Vertices.Length * sizeof(float), Display.Vertices, BufferUsageHint.StreamDraw);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Display.Stride, 0); //vertex data
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, Display.Stride, 3 * sizeof(float)); //Texture coordinate data, change this to be instanced data instead and change spritesheet object definitions to compensate

            GL.BindBuffer(BufferTarget.ArrayBuffer, _instancedArrayBuffer);

            GL.VertexAttribPointer(2, 4, VertexAttribPointerType.Float, false, instanceDataLength, 0);                  //| Transformation matrix data
            GL.VertexAttribPointer(3, 4, VertexAttribPointerType.Float, false, instanceDataLength, 4 * sizeof(float));  //|
            GL.VertexAttribPointer(4, 4, VertexAttribPointerType.Float, false, instanceDataLength, 8 * sizeof(float));  //|
            GL.VertexAttribPointer(5, 4, VertexAttribPointerType.Float, false, instanceDataLength, 12 * sizeof(float)); //|

            GL.VertexAttribPointer(6, 4, VertexAttribPointerType.Float, false, instanceDataLength, 16 * sizeof(float)); //Color data
            GL.VertexAttribPointer(7, 4, VertexAttribPointerType.Float, false, instanceDataLength, 20 * sizeof(float)); //Camera enabled + spritesheet position + side lengths X and Y

            int currIndex = 0;

            int count = 0;

            float sideLengthX = 0;
            float sideLengthY = 0;
            float spritesheetPosition = 0;
            float cameraPerspective = 0;

            Particle obj;

            for (int i = 0; i < generator.Particles.Count; i++)
            {
                obj = generator.Particles[i];
                if (obj.Life > 0)
                {
                    var transform = obj.Transformations;

                    spritesheetPosition = obj.SpritesheetPosition;
                    sideLengthX = obj.SideLengths.X;
                    sideLengthY = obj.SideLengths.Y;
                    if (Display.CameraPerspective)
                        cameraPerspective = 1.0f;

                    InsertMatrixDataIntoArray(ref _instancedRenderArray, ref transform, currIndex);
                    currIndex += 16;
                    _instancedRenderArray[currIndex++] = obj.Color.X;
                    _instancedRenderArray[currIndex++] = obj.Color.Y;
                    _instancedRenderArray[currIndex++] = obj.Color.Z;
                    _instancedRenderArray[currIndex++] = obj.Color.W;

                    _instancedRenderArray[currIndex++] = cameraPerspective;
                    _instancedRenderArray[currIndex++] = spritesheetPosition;
                    _instancedRenderArray[currIndex++] = sideLengthX;
                    _instancedRenderArray[currIndex++] = sideLengthY;

                    count++;
                }
            }

            GL.BufferData(BufferTarget.ArrayBuffer, count * instanceDataOffset * sizeof(float), _instancedRenderArray, BufferUsageHint.StreamDraw);

            GL.DrawArraysInstanced(PrimitiveType.TriangleFan, 0, 4, count);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            DisableInstancedShaderAttributes();

            DrawCount++;
        }
        public void RenderParticle(ParticleGenerator generator, Particle obj, Matrix4 cameraMatrix, bool setVertexData = true, bool setTextureData = true, bool setCam = true)
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

            var transform = obj.Transformations;
            //var transform = Matrix4.Identity;
            //transform *= obj.Rotation;
            //transform *= obj.Scale;
            //transform *= obj.Translation;

            if (setCam)
            {
                generator.ParticleDisplay.ShaderReference.SetBool("enable_cam", generator.ParticleDisplay.CameraPerspective);
            }

            generator.ParticleDisplay.ShaderReference.SetMatrix4("transform", transform);

            generator.ParticleDisplay.ShaderReference.SetVector4("aColor", obj.Color);

            GL.DrawElements(PrimitiveType.TriangleStrip, generator.ParticleDisplay.VerticesDrawOrder.Length, DrawElementsType.UnsignedInt, 0);
            DrawCount++;
        }


        public void LoadTextureFromGameObj<T>(T gameObj, bool nearest = true) where T : GameObject 
        {
            gameObj.BaseObjects.ForEach(obj =>
            {
                foreach (KeyValuePair<AnimationType, Animation> entry in obj.Animations)
                {
                    for (int o = 0; o < entry.Value.Frames.Count; o++)
                    {
                        for (int p = 0; p < entry.Value.Frames[o].Textures.Textures.Length; p++)
                        {
                            if (!_loadedTextures.TryGetValue(entry.Value.Frames[o].Textures.TextureFilenames[p], out int handle))
                            {
                                Texture newTexture = Texture.LoadFromFile(entry.Value.Frames[o].Textures.TextureFilenames[p], nearest);
                                _textures.Add(newTexture);
                                _loadedTextures.Add(entry.Value.Frames[o].Textures.TextureFilenames[p], newTexture.Handle);

                                entry.Value.Frames[o].TextureReference = newTexture;
                            }
                            else
                            {
                                entry.Value.Frames[o].TextureReference = new Texture(handle);
                            }
                        }
                    }
                }
            });
        } 
        public void LoadTextureFromBaseObject(BaseObject obj, bool nearest = true)
        {
            foreach (KeyValuePair<AnimationType, Animation> entry in obj.Animations)
            {
                for (int o = 0; o < entry.Value.Frames.Count; o++)
                {
                    for (int p = 0; p < entry.Value.Frames[o].Textures.Textures.Length; p++)
                    {
                        if (!_loadedTextures.TryGetValue(entry.Value.Frames[o].Textures.TextureFilenames[p], out int handle))
                        {
                            Texture newTexture = Texture.LoadFromFile(entry.Value.Frames[o].Textures.TextureFilenames[p], nearest);
                            _textures.Add(newTexture);
                            _loadedTextures.Add(entry.Value.Frames[o].Textures.TextureFilenames[p], newTexture.Handle);

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
        public void LoadTextureFromParticleGen(ParticleGenerator generator)
        {
            for (int p = 0; p < generator.ParticleDisplay.Textures.Textures.Length; p++)
            {
                if (!_loadedTextures.TryGetValue(generator.ParticleDisplay.Textures.TextureFilenames[p], out int handle))
                {
                    Texture newTexture = Texture.LoadFromFile(generator.ParticleDisplay.Textures.TextureFilenames[p]);
                    _textures.Add(newTexture);
                    _loadedTextures.Add(generator.ParticleDisplay.Textures.TextureFilenames[p], newTexture.Handle);

                    generator.ParticleDisplay.TextureReference = newTexture;
                }
                else
                {
                    generator.ParticleDisplay.TextureReference = new Texture(handle);
                }
            }
        }
        public void LoadTextureFromUIObject<T>(T UIObj) where T : UIObject 
        {
            UIObj.BaseObjects.ForEach(obj => 
            {
                LoadTextureFromBaseObject(obj, true);
            });
            UIObj.TextObjects.ForEach(obj =>
            {
                if (obj.Letters.Count > 0)
                {
                    LoadTextureFromBaseObject(obj.Letters[0].BaseObjects[0], true);
                }
            });
            UIObj.Children.ForEach(obj =>
            {
                LoadTextureFromUIObject(obj);
            });
        }
        public void RenderUIElements<T>(List<T> uiObjects) where T : UIObject
        {
            if (uiObjects.Count > 0)
            {
                uiObjects.ForEach(obj =>
                {
                    if (obj.Render)
                    {
                        RenderTextInstanced(obj.TextObjects);
                        RenderUIElements(obj.Children);
                    }
                });


                RenderableObject display = uiObjects[0].GetDisplay();

                RenderObjectsInstancedGeneric(uiObjects, display);
            }
        }

        public void ClearData() 
        {
            GL.DeleteBuffer(_vertexBufferObject);
            GL.DeleteBuffer(_instancedVertexBuffer);
            GL.DeleteBuffer(_elementBufferObject);

            GL.DeleteVertexArray(_vertexArrayObject);
            GL.DeleteVertexArray(_instancedArrayBuffer);

            for (int i = 0; i < _textures.Count; i++)
            {
                GL.DeleteTexture(_textures[i].Handle);
            }
        }

        private void EnableInstancedShaderAttributes()
        {
            GL.EnableVertexAttribArray(2);
            GL.EnableVertexAttribArray(3);
            GL.EnableVertexAttribArray(4);
            GL.EnableVertexAttribArray(5);
            GL.EnableVertexAttribArray(6);
            GL.EnableVertexAttribArray(7);
            GL.VertexAttribDivisor(2, 1);
            GL.VertexAttribDivisor(3, 1);
            GL.VertexAttribDivisor(4, 1);
            GL.VertexAttribDivisor(5, 1);
            GL.VertexAttribDivisor(6, 1);
            GL.VertexAttribDivisor(7, 1);
        }
        private void DisableInstancedShaderAttributes()
        {
            GL.DisableVertexAttribArray(2);
            GL.DisableVertexAttribArray(3);
            GL.DisableVertexAttribArray(4);
            GL.DisableVertexAttribArray(5);
            GL.DisableVertexAttribArray(6);
            GL.DisableVertexAttribArray(7);
            GL.VertexAttribDivisor(2, 0);
            GL.VertexAttribDivisor(3, 0);
            GL.VertexAttribDivisor(4, 0);
            GL.VertexAttribDivisor(5, 0);
            GL.VertexAttribDivisor(6, 0);
            GL.VertexAttribDivisor(7, 0);
        }

        private void InsertMatrixDataIntoArray(ref float[] arr, ref Matrix4 mat, int currIndex)
        {

            //this seems to perform better (maybe due to M11,M22,etc using getters and setters)
            arr[currIndex++] = mat.Row0.X;
            arr[currIndex++] = mat.Row1.X;
            arr[currIndex++] = mat.Row2.X;
            arr[currIndex++] = mat.Row3.X;
            arr[currIndex++] = mat.Row0.Y;
            arr[currIndex++] = mat.Row1.Y;
            arr[currIndex++] = mat.Row2.Y;
            arr[currIndex++] = mat.Row3.Y;
            arr[currIndex++] = mat.Row0.Z;
            arr[currIndex++] = mat.Row1.Z;
            arr[currIndex++] = mat.Row2.Z;
            arr[currIndex++] = mat.Row3.Z;
            arr[currIndex++] = mat.Row0.W;
            arr[currIndex++] = mat.Row1.W;
            arr[currIndex++] = mat.Row2.W;
            arr[currIndex++] = mat.Row3.W;
        }
    }
}
