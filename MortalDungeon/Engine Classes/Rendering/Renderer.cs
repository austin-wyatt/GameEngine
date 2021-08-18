using MortalDungeon.Game.GameObjects;
using MortalDungeon.Game.Objects;
using MortalDungeon.Game.Tiles;
using MortalDungeon.Game.UI;
using MortalDungeon.Objects;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace MortalDungeon.Engine_Classes.Rendering
{
    public static class RenderingConstants 
    {
        public static float TextAlphaThreshold = 0.5f;
        public static float DefaultAlphaThreshold = 0.15f;
    }
    public static class Renderer
    {
        static readonly Random _rand = new Random();
        public static int _vertexBufferObject;
        public static int _vertexArrayObject;

        private static int _elementBufferObject;
        public static int _instancedVertexBuffer;
        public static int _instancedArrayBuffer;


        private const int ObjectBufferCount = 15000;
        private const int instanceDataOffset = 40;
        public static float[] _instancedRenderArray = new float[ObjectBufferCount * instanceDataOffset];
        private const int instanceDataLength = instanceDataOffset * sizeof(float);

        private static readonly List<Texture> _textures = new List<Texture>();
        private static readonly Dictionary<TextureName, int> _loadedTextures = new Dictionary<TextureName, int>();


        private static FrameBufferObject MainFBO;

        public static List<FrameBufferObject> _fbos = new List<FrameBufferObject>();

        public static int DrawCount = 0;
        public static int FPSCount = 0;
        public static Stopwatch _internalTimer = new Stopwatch();

        static Renderer() { }
        public static void Load() //initialization of renderer
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


            MainFBO = new FrameBufferObject(WindowConstants.ClientSize);

            _fbos.Add(MainFBO);

            _elementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);

            _instancedArrayBuffer = GL.GenBuffer();
            _instancedVertexBuffer = GL.GenBuffer();

            _internalTimer.Start();


            //set the texture uniform locations
            Shaders.FAST_DEFAULT_SHADER.Use();
            //int tex0Location = GL.GetUniformLocation(Shaders.FAST_DEFAULT_SHADER.Handle, "texture0");
            //int tex1Location = GL.GetUniformLocation(Shaders.FAST_DEFAULT_SHADER.Handle, "texture1");
            //int tex2Location = GL.GetUniformLocation(Shaders.FAST_DEFAULT_SHADER.Handle, "texture2");
            //int tex3Location = GL.GetUniformLocation(Shaders.FAST_DEFAULT_SHADER.Handle, "texture3");
            //int tex4Location = GL.GetUniformLocation(Shaders.FAST_DEFAULT_SHADER.Handle, "texture4");

            //GL.Uniform1(tex0Location, 0);
            //GL.Uniform1(tex1Location, 1);
            //GL.Uniform1(tex2Location, 2);
            //GL.Uniform1(tex3Location, 3);
            //GL.Uniform1(tex4Location, 4);

            Shaders.SIMPLE_SHADER.Use();
            int texLocation = GL.GetUniformLocation(Shaders.SIMPLE_SHADER.Handle, "texture0");
            GL.Uniform1(texLocation, 0);
        }

        public static void RenderObject(BaseObject obj, bool setVertexData = true, bool setTextureData = true, bool setCam = true, bool setColor = true)
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

            var transform = obj.BaseFrame.Transformations;


            if (setCam)
            {
                Display.ShaderReference.SetBool("enable_cam", Display.CameraPerspective);
            }
            if (setColor)
            {
                Display.ShaderReference.SetVector4("aColor", obj.BaseFrame.Color);
            }

            Display.ShaderReference.SetMatrix4("transform", transform);

            //Display.ShaderReference.SetFloat("fMixPercent", obj._baseFrame.ColorProportion);

            GL.DrawElements(PrimitiveType.Triangles, Display.VerticesDrawOrder.Length, DrawElementsType.UnsignedInt, 0);

            DrawCount++;
        }

        public static void PrepareInstancedRenderFunc(RenderableObject Display) 
        {
            EnableInstancedShaderAttributes();

            GL.BindBuffer(BufferTarget.ArrayBuffer, _instancedVertexBuffer);

            GL.BufferData(BufferTarget.ArrayBuffer, Display.Vertices.Length * sizeof(float), Display.Vertices, BufferUsageHint.StreamDraw); //take the raw vertices


            //Whenever this data is changed the instanceDataOffset parameter needs to be updated
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Display.Stride, 0); //vertex data
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, Display.Stride, 3 * sizeof(float)); //Texture coordinate data

            GL.BindBuffer(BufferTarget.ArrayBuffer, _instancedArrayBuffer);

            GL.VertexAttribPointer(2, 4, VertexAttribPointerType.Float, false, instanceDataLength, 0);                  //| Transformation matrix data
            GL.VertexAttribPointer(3, 4, VertexAttribPointerType.Float, false, instanceDataLength, 4 * sizeof(float));  //|
            GL.VertexAttribPointer(4, 4, VertexAttribPointerType.Float, false, instanceDataLength, 8 * sizeof(float));  //|
            GL.VertexAttribPointer(5, 4, VertexAttribPointerType.Float, false, instanceDataLength, 12 * sizeof(float)); //|

            GL.VertexAttribPointer(6, 4, VertexAttribPointerType.Float, false, instanceDataLength, 16 * sizeof(float)); //Color data
            GL.VertexAttribPointer(7, 4, VertexAttribPointerType.Float, false, instanceDataLength, 20 * sizeof(float)); //Camera enabled + spritesheet position + side lengths X and Y
            GL.VertexAttribPointer(8, 4, VertexAttribPointerType.Float, false, instanceDataLength, 24 * sizeof(float)); //Spritesheet X + Spritesheet Y + use second texture + mix percent

            GL.VertexAttribPointer(9, 4, VertexAttribPointerType.Float, false, instanceDataLength, 28 * sizeof(float)); //Inline thickness + outline thickness + alpha threshold + primary texture target
            GL.VertexAttribPointer(10, 4, VertexAttribPointerType.Float, false, instanceDataLength, 32 * sizeof(float)); //Inline color
            GL.VertexAttribPointer(11, 4, VertexAttribPointerType.Float, false, instanceDataLength, 36 * sizeof(float)); //Outline color
        }
        public static void InsertDataIntoInstancedRenderArray(BaseObject obj, MultiTextureData renderingData, ref float[] _instancedRenderArray, ref int currIndex, int textureTarget)
        {
            var transform = obj.BaseFrame.Transformations;

            InsertMatrixDataIntoArray(ref _instancedRenderArray, ref transform, currIndex);
            currIndex += 16;
            _instancedRenderArray[currIndex++] = obj.BaseFrame.Color.X;
            _instancedRenderArray[currIndex++] = obj.BaseFrame.Color.Y;
            _instancedRenderArray[currIndex++] = obj.BaseFrame.Color.Z;
            _instancedRenderArray[currIndex++] = obj.BaseFrame.Color.W;

            _instancedRenderArray[currIndex++] = obj.BaseFrame.CameraPerspective ? 1 : 0;
            _instancedRenderArray[currIndex++] = obj._currentAnimation.CurrentFrame.SpritesheetPosition;
            _instancedRenderArray[currIndex++] = obj._currentAnimation.CurrentFrame.SideLengths.X;
            _instancedRenderArray[currIndex++] = obj._currentAnimation.CurrentFrame.SideLengths.Y;

            _instancedRenderArray[currIndex++] = obj._currentAnimation.CurrentFrame.Textures.Spritesheet.Columns;
            _instancedRenderArray[currIndex++] = obj._currentAnimation.CurrentFrame.Textures.Spritesheet.Rows;
            _instancedRenderArray[currIndex++] = renderingData.MixTexture ? 1 : 0;
            _instancedRenderArray[currIndex++] = renderingData.MixPercent;

            _instancedRenderArray[currIndex++] = obj.OutlineParameters.InlineThickness;
            _instancedRenderArray[currIndex++] = obj.OutlineParameters.OutlineThickness;
            _instancedRenderArray[currIndex++] = obj.RenderData.AlphaThreshold;
            _instancedRenderArray[currIndex++] = textureTarget + 0.01f; //add a small number to curb rounding errors when casting to int in the shader

            _instancedRenderArray[currIndex++] = obj.OutlineParameters.InlineColor.X;
            _instancedRenderArray[currIndex++] = obj.OutlineParameters.InlineColor.Y;
            _instancedRenderArray[currIndex++] = obj.OutlineParameters.InlineColor.Z;
            _instancedRenderArray[currIndex++] = obj.OutlineParameters.InlineColor.W;

            _instancedRenderArray[currIndex++] = obj.OutlineParameters.OutlineColor.X;
            _instancedRenderArray[currIndex++] = obj.OutlineParameters.OutlineColor.Y;
            _instancedRenderArray[currIndex++] = obj.OutlineParameters.OutlineColor.Z;
            _instancedRenderArray[currIndex++] = obj.OutlineParameters.OutlineColor.W;
        }


        public static void RenderObjectsInstancedGeneric<T>(List<T> objects, ref float[] _instancedRenderDataArray, RenderableObject display = null, bool instantiateRenderFunc = true) where T : GameObject
        {
            if (objects.Count == 0)
                return;

            Shaders.FAST_DEFAULT_SHADER.Use();

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

            if (instantiateRenderFunc)
                PrepareInstancedRenderFunc(Display);

            int currIndex = 0;

            int count = 0;
            List<T> recursiveCallList = new List<T>();

            Dictionary<TextureName, TextureUnit> usedTextures = new Dictionary<TextureName, TextureUnit>();
            Dictionary<Texture, TextureUnit> textureReferences = new Dictionary<Texture, TextureUnit>();

            List<T> scissorCallList = new List<T>();

            usedTextures.Add(currTexture, TextureUnit.Texture0);
            textureReferences.Add(Display.TextureReference, TextureUnit.Texture0);

            BaseObject obj;
            TextureName tex;

            TextureUnit currentTextureUnit = TextureUnit.Texture2;

            static void draw(int itemCount, float[] renderDataArray) 
            {
                GL.BufferData(BufferTarget.ArrayBuffer, itemCount * instanceDataOffset * sizeof(float), renderDataArray, BufferUsageHint.StreamDraw);

                GL.DrawArraysInstanced(PrimitiveType.TriangleFan, 0, 4, itemCount);

                DrawCount++;
            }

            for (int i = 0; i < objects.Count; i++)
            {
                if (objects[i].Render && !objects[i].Cull)
                {
                    if (objects[i].MultiTextureData.MixedTexture != null && objects[i].MultiTextureData.MixTexture)
                    {
                        if (!Texture.UsedTextures.TryGetValue(objects[i].MultiTextureData.MixedTextureLocation, out tex) && tex != objects[i].MultiTextureData.MixedTextureName)
                        {
                            objects[i].MultiTextureData.MixedTexture.Use(objects[i].MultiTextureData.MixedTextureLocation);
                        }
                    }

                    
                    if (objects[i].ScissorData.Scissor && objects[i].ScissorData._scissorFlag) 
                    {
                        draw(count, _instancedRenderDataArray);
                        count = 0;
                        currIndex = 0;

                        scissorCallList.Add(objects[i]);
                        objects[i].ScissorData._scissorFlag = false;

                        ScissorData scissorData = scissorCallList[^1].ScissorData;
                        GL.Scissor(scissorData.X, scissorData.Y, scissorData.Width, scissorData.Height);
                        GL.Enable(EnableCap.ScissorTest);

                        int totalObjCount = 0;

                        for (int l = 0; l < scissorCallList.Count; l++) 
                        {
                            totalObjCount += scissorCallList[l].BaseObjects.Count;
                        }

                        float[] instancedArr = new float[totalObjCount * instanceDataOffset];



                        RenderObjectsInstancedGeneric(scissorCallList, ref instancedArr, Display, false);

                        foreach (Texture texture in textureReferences.Keys) 
                        {
                            texture.Use(textureReferences[texture]);
                        }

                        GL.Disable(EnableCap.ScissorTest);

                        scissorCallList.Clear();

                    }
                    else if (objects[i].ScissorData._scissorFlag)
                    {
                        scissorCallList.Add(objects[i]);
                        objects[i].ScissorData._scissorFlag = false;
                    }
                    else
                    {
                        for (int j = 0; j < objects[i].BaseObjects.Count; j++)
                        {
                            if (objects[i].BaseObjects[j].Render)
                            {
                                obj = objects[i].BaseObjects[j];
                                tex = obj._currentAnimation.CurrentFrame.Textures.Textures[0];

                                if (tex != currTexture)
                                {
                                    if (!usedTextures.ContainsKey(tex))
                                    {
                                        usedTextures.Add(tex, currentTextureUnit);
                                        textureReferences.Add(obj._currentAnimation.CurrentFrame.TextureReference, currentTextureUnit);

                                        obj._currentAnimation.CurrentFrame.TextureReference.Use(currentTextureUnit);

                                        currentTextureUnit++;
                                    }
                                }

                                if (count == ObjectBufferCount)
                                {
                                    recursiveCallList.Add(objects[i]);
                                }
                                else
                                {
                                    InsertDataIntoInstancedRenderArray(obj, objects[i].MultiTextureData, ref _instancedRenderDataArray, ref currIndex, (usedTextures[tex] - TextureUnit.Texture0));

                                    count++;
                                }
                            }
                        }
                    }
                }
            }

            draw(count, _instancedRenderDataArray);

            //if (instantiateRenderFunc)
            //    DisableInstancedShaderAttributes();


            if (recursiveCallList.Count > 0)
            {
                RenderObjectsInstancedGeneric(recursiveCallList, ref _instancedRenderDataArray, Display);
            }
        }

        public static void RenderBaseObjectsInstanced(List<BaseObject> objects, List<MultiTextureData> multiTextureData, RenderableObject display = null)
        {
            if (objects.Count == 0)
                return;

            Shaders.FAST_DEFAULT_SHADER.Use();

            RenderableObject Display;

            if (display == null)
            {
                Display = objects[0]._currentAnimation.CurrentFrame;
            }
            else
            {
                Display = display;
            }
            TextureName currTexture = Display.Textures.Textures[0];


            Display.TextureReference.Use(TextureUnit.Texture0);


            PrepareInstancedRenderFunc(Display);


            int currIndex = 0;

            int count = 0;
            List<BaseObject> recursiveCallList = new List<BaseObject>();

            List<BaseObject> difTextureCallList = new List<BaseObject>();

            List<MultiTextureData> multiTextureList = new List<MultiTextureData>();

            TextureName tex;

            int objIndex = 0;

            objects.ForEach(obj =>
            {
                if (obj.Render)
                {
                    tex = obj._currentAnimation.CurrentFrame.Textures.Textures[0];
                    if (count == ObjectBufferCount)
                    {
                        recursiveCallList.Add(obj);
                        multiTextureList.Add(multiTextureData[objIndex]);
                    }
                    else if (tex != currTexture)
                    {
                        difTextureCallList.Add(obj);
                        multiTextureList.Add(multiTextureData[objIndex]);
                    }
                    else
                    {
                        InsertDataIntoInstancedRenderArray(obj, multiTextureData[objIndex], ref _instancedRenderArray, ref currIndex, 0);

                        count++;
                    }
                }
                objIndex++;
            });


            GL.BufferData(BufferTarget.ArrayBuffer, count * instanceDataOffset * sizeof(float), _instancedRenderArray, BufferUsageHint.StreamDraw);


            GL.DrawArraysInstanced(PrimitiveType.TriangleFan, 0, 4, count);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);


            DisableInstancedShaderAttributes();


            if (recursiveCallList.Count > 0)
            {
                RenderBaseObjectsInstanced(recursiveCallList, multiTextureList);
            }

            if (difTextureCallList.Count > 0)
            {
                RenderBaseObjectsInstanced(difTextureCallList, multiTextureList);
            }

            DrawCount++;
        }


        static readonly float[] g_quad_vertex_buffer_data = new float[]{
            -1.0f, -1.0f, 0.0f,
            1.0f, -1.0f, 0.0f,
            -1.0f,  1.0f, 0.0f,
            -1.0f,  1.0f, 0.0f,
            1.0f, -1.0f, 0.0f,
            1.0f,  1.0f, 0.0f,
        };

        //float[] g_quad_vertex_buffer_data = new float[]{
        //    -1.0f, 1f, 0.0f,
        //    -1f, -0f, 0.0f,
        //    0f,  1f, 0.0f,
        //    0f,  1f, 0.0f,
        //    -1f, -0f, 0.0f,
        //    0f,  0f, 0.0f,
        //};

        public static void RenderFrameBuffer(FrameBufferObject frameBuffer, FrameBufferObject destinationBuffer = null, Shader shader = null) 
        {
            //Bind the texture that the objects were rendered on to
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, frameBuffer.RenderTexture);

            //empty out the depth buffer for when we reuse this frame buffer
            frameBuffer.ClearBuffers();

            //bind the current frame buffer to either the destination buffer if passed or the default buffer if not
            if (destinationBuffer != null)
            {
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, destinationBuffer.FrameBuffer);
            }
            else 
            {
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            }

            #region Render quad with the frame buffer texture
            if (shader != null) 
            {
                shader.Use();
            }
            else 
            {
                Shaders.SIMPLE_SHADER.Use();
            }

            GL.EnableVertexAttribArray(0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * g_quad_vertex_buffer_data.Length, g_quad_vertex_buffer_data, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            #endregion

            FramebufferErrorCode errorTest = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);

            if (errorTest != FramebufferErrorCode.FramebufferComplete) 
            {
                Console.WriteLine("Error in RenderFrameBuffer: " + errorTest);
            }


            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public static void DrawToFrameBuffer(FrameBufferObject frameBuffer) 
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, frameBuffer.FrameBuffer);

            GL.DrawBuffer(DrawBufferMode.ColorAttachment0);

            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public static void RenderParticlesInstanced(ParticleGenerator generator)
        {
            if (generator.Particles.Count == 0)
                return;
            Shaders.FAST_DEFAULT_SHADER.Use();

            RenderableObject Display = generator.ParticleDisplay;

            Display.TextureReference.Use(TextureUnit.Texture0);

            PrepareInstancedRenderFunc(Display);

            int currIndex = 0;

            int count = 0;

            float sideLengthX;
            float sideLengthY;
            float spritesheetPosition;
            float cameraPerspective = 0;
            float spritesheetWidth;
            float spritesheetHeight;

            Particle obj;

            for (int i = 0; i < generator.Particles.Count; i++)
            {
                obj = generator.Particles[i];
                if (obj.Life > 0 && !obj.Cull)
                {
                    var transform = obj.Transformations;

                    spritesheetPosition = obj.SpritesheetPosition;
                    sideLengthX = obj.SideLengths.X;
                    sideLengthY = obj.SideLengths.Y;
                    if (Display.CameraPerspective)
                        cameraPerspective = 1.0f;

                    spritesheetWidth = Display.Textures.Spritesheet.Columns;
                    spritesheetHeight = Display.Textures.Spritesheet.Rows;

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

                    _instancedRenderArray[currIndex++] = spritesheetWidth;
                    _instancedRenderArray[currIndex++] = spritesheetHeight;
                    _instancedRenderArray[currIndex++] = 0;
                    _instancedRenderArray[currIndex++] = 0;

                    _instancedRenderArray[currIndex++] = generator.OutlineParameters.InlineThickness;
                    _instancedRenderArray[currIndex++] = generator.OutlineParameters.OutlineThickness;
                    _instancedRenderArray[currIndex++] = 0;
                    _instancedRenderArray[currIndex++] = 0;

                    _instancedRenderArray[currIndex++] = generator.OutlineParameters.InlineColor.X;
                    _instancedRenderArray[currIndex++] = generator.OutlineParameters.InlineColor.Y;
                    _instancedRenderArray[currIndex++] = generator.OutlineParameters.InlineColor.Z;
                    _instancedRenderArray[currIndex++] = generator.OutlineParameters.InlineColor.W;

                    _instancedRenderArray[currIndex++] = generator.OutlineParameters.OutlineColor.X;
                    _instancedRenderArray[currIndex++] = generator.OutlineParameters.OutlineColor.Y;
                    _instancedRenderArray[currIndex++] = generator.OutlineParameters.OutlineColor.Z;
                    _instancedRenderArray[currIndex++] = generator.OutlineParameters.OutlineColor.W;

                    count++;
                }
            }

            GL.BufferData(BufferTarget.ArrayBuffer, count * instanceDataOffset * sizeof(float), _instancedRenderArray, BufferUsageHint.StreamDraw);

            GL.DrawArraysInstanced(PrimitiveType.TriangleFan, 0, 4, count);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            DisableInstancedShaderAttributes();

            DrawCount++;
        }

        #region Load/unload textures
        public static void LoadTextureFromGameObj<T>(T gameObj, bool nearest = true) where T : GameObject 
        {
            gameObj.BaseObjects.ForEach(obj =>
            {
                foreach (KeyValuePair<AnimationType, Animation> entry in obj.Animations)
                {
                    for (int o = 0; o < entry.Value.Frames.Count; o++)
                    {
                        for (int p = 0; p < entry.Value.Frames[o].Textures.Textures.Length; p++)
                        {
                            if (entry.Value.Frames[o].TextureReference == null || entry.Value.Frames[o].TextureReference.ImageData == null) 
                            {
                                if (!_loadedTextures.TryGetValue(entry.Value.Frames[o].Textures.Textures[p], out int handle))
                                {
                                    Texture newTexture = Texture.LoadFromFile(entry.Value.Frames[o].Textures.TextureFilenames[p], nearest);
                                    newTexture.TextureName = entry.Value.Frames[o].Textures.Textures[p];

                                    _textures.Add(newTexture);
                                    _loadedTextures.Add(entry.Value.Frames[o].Textures.Textures[p], newTexture.Handle);

                                    entry.Value.Frames[o].TextureReference = newTexture;
                                }
                                else
                                {
                                    entry.Value.Frames[o].TextureReference = new Texture(handle, entry.Value.Frames[o].Textures.Textures[p]);
                                }
                            }
                        }
                    }
                }
            });
        }
        public static void LoadTextureFromBaseObject(BaseObject obj, bool nearest = true)
        {
            foreach (KeyValuePair<AnimationType, Animation> entry in obj.Animations)
            {
                for (int o = 0; o < entry.Value.Frames.Count; o++)
                {
                    for (int p = 0; p < entry.Value.Frames[o].Textures.Textures.Length; p++)
                    {
                        if (entry.Value.Frames[o].TextureReference == null || entry.Value.Frames[o].TextureReference.ImageData == null) 
                        {
                            if (!_loadedTextures.TryGetValue(entry.Value.Frames[o].Textures.Textures[p], out int handle))
                            {
                                Texture newTexture = Texture.LoadFromFile(entry.Value.Frames[o].Textures.TextureFilenames[p], nearest);
                                newTexture.TextureName = entry.Value.Frames[o].Textures.Textures[p];

                                _textures.Add(newTexture);
                                _loadedTextures.Add(entry.Value.Frames[o].Textures.Textures[p], newTexture.Handle);

                                entry.Value.Frames[o].TextureReference = newTexture;
                            }
                            else
                            {
                                entry.Value.Frames[o].TextureReference = new Texture(handle, entry.Value.Frames[o].Textures.Textures[p]);
                            }
                        }

                    }
                }
            }
        }
        public static void LoadTextureFromParticleGen(ParticleGenerator generator)
        {
            for (int p = 0; p < generator.ParticleDisplay.Textures.Textures.Length; p++)
            {
                if (!_loadedTextures.TryGetValue(generator.ParticleDisplay.Textures.Textures[p], out int handle))
                {
                    Texture newTexture = Texture.LoadFromFile(generator.ParticleDisplay.Textures.TextureFilenames[p]);
                    newTexture.TextureName = generator.ParticleDisplay.Textures.Textures[p];

                    _textures.Add(newTexture);
                    _loadedTextures.Add(generator.ParticleDisplay.Textures.Textures[p], newTexture.Handle);

                    generator.ParticleDisplay.TextureReference = newTexture;
                }
                else
                {
                    generator.ParticleDisplay.TextureReference = new Texture(handle, generator.ParticleDisplay.Textures.Textures[p]);
                }
            }
        }
        public static void LoadTextureFromUIObject<T>(T UIObj) where T : UIObject 
        {
            UIObj.BaseObjects.ForEach(obj => 
            {
                LoadTextureFromBaseObject(obj, true);
            });
            UIObj.TextObjects.ForEach(obj =>
            {
                if (obj.Letters.Count > 0)
                {
                    LoadTextureFromBaseObject(obj.Letters[0].BaseObjects[0], false);
                }
            });
            UIObj.Children.ForEach(obj =>
            {
                LoadTextureFromUIObject(obj);
            });
        }

        public static void LoadTextureFromTextureObj(Texture texture, TextureName textureName) 
        {
            if (!_loadedTextures.TryGetValue(textureName, out _))
            {
                _textures.Add(texture);
                _loadedTextures.Add(textureName, texture.Handle);
            }
        }

        public static void UnloadTexture(TextureName textureName) 
        {
            Texture tex = _textures.Find(tex => tex.TextureName == textureName);
            if (tex != null) 
            {
                GL.DeleteTexture(tex.Handle);
                _loadedTextures.Remove(tex.TextureName);
            }
        }
        public static void UnloadAllTextures() 
        {
            for (int i = 0; i < _textures.Count; i++)
            {
                GL.DeleteTexture(_textures[i].Handle);
            }

            _textures.Clear();
            _loadedTextures.Clear();
        }
        #endregion


        public static int CreatePixelBufferObject(float[] imgData) 
        {
            int pbo = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.PixelUnpackBuffer, pbo);
            GL.BufferData(BufferTarget.PixelUnpackBuffer, imgData.Length * 4, imgData, BufferUsageHint.StreamDraw);

            GL.BindBuffer(BufferTarget.PixelUnpackBuffer, 0);
            return pbo;
        }



        public static void ClearData() 
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

        public static void ResizeFBOs(Vector2i newSize) 
        {
            _fbos.ForEach(fbo =>
            {
                fbo.ResizeFBO(newSize);
            });
        }

        public static void EnableInstancedShaderAttributes()
        {
            GL.EnableVertexAttribArray(2);
            GL.EnableVertexAttribArray(3);
            GL.EnableVertexAttribArray(4);
            GL.EnableVertexAttribArray(5);
            GL.EnableVertexAttribArray(6);
            GL.EnableVertexAttribArray(7);
            GL.EnableVertexAttribArray(8);
            GL.EnableVertexAttribArray(9);
            GL.EnableVertexAttribArray(10);
            GL.EnableVertexAttribArray(11);
            GL.VertexAttribDivisor(2, 1);
            GL.VertexAttribDivisor(3, 1);
            GL.VertexAttribDivisor(4, 1);
            GL.VertexAttribDivisor(5, 1);
            GL.VertexAttribDivisor(6, 1);
            GL.VertexAttribDivisor(7, 1);
            GL.VertexAttribDivisor(8, 1);
            GL.VertexAttribDivisor(9, 1);
            GL.VertexAttribDivisor(10, 1);
            GL.VertexAttribDivisor(11, 1);
        }
        public static void DisableInstancedShaderAttributes()
        {
            GL.DisableVertexAttribArray(2);
            GL.DisableVertexAttribArray(3);
            GL.DisableVertexAttribArray(4);
            GL.DisableVertexAttribArray(5);
            GL.DisableVertexAttribArray(6);
            GL.DisableVertexAttribArray(7);
            GL.DisableVertexAttribArray(8);
            GL.DisableVertexAttribArray(9);
            GL.DisableVertexAttribArray(10);
            GL.DisableVertexAttribArray(11);
            GL.VertexAttribDivisor(2, 0);
            GL.VertexAttribDivisor(3, 0);
            GL.VertexAttribDivisor(4, 0);
            GL.VertexAttribDivisor(5, 0);
            GL.VertexAttribDivisor(6, 0);
            GL.VertexAttribDivisor(7, 0);
            GL.VertexAttribDivisor(8, 0);
            GL.VertexAttribDivisor(9, 0);
            GL.VertexAttribDivisor(10, 0);
            GL.VertexAttribDivisor(11, 0);
        }

        public static void InsertMatrixDataIntoArray(ref float[] arr, ref Matrix4 mat, int currIndex)
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
