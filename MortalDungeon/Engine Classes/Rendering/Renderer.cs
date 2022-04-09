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
        public static Vector3 LightPosition = new Vector3();
        public static Vector4 LightColor = new Vector4();
    }

    public struct ViewportRectangle
    {
        public int X;
        public int Y;
        public int Width;
        public int Height;
    }
    public static class Renderer
    {
        static readonly Random _rand = new ConsistentRandom();
        public static int _vertexBufferObject;
        public static int _vertexArrayObject;

        private static int _elementBufferObject;
        public static int _instancedVertexBuffer;
        public static int _instancedArrayBuffer;


        private const int ObjectBufferCount = 7500;
        private const int instanceDataOffset = 40;
        public static float[] _instancedRenderArray = new float[ObjectBufferCount * instanceDataOffset];
        private const int instanceDataLength = instanceDataOffset * sizeof(float);

        private const int particleDataOffset = 28;
        private const int particleDataLength = particleDataOffset * sizeof(float);

        public static readonly List<Texture> _textures = new List<Texture>();
        public static readonly Dictionary<int, int> _loadedTextures = new Dictionary<int, int>();


        public static FrameBufferObject MainFBO;
        public static FrameBufferObject StageOneFBO;
        public static FrameBufferObject StageTwoFBO;

        public static List<FrameBufferObject> _fbos = new List<FrameBufferObject>();

        public static int DrawCount = 0;
        public static int FPSCount = 0;
        public static int ObjectsDrawn = 0;

        public static Stopwatch _internalTimer = new Stopwatch();

        public static readonly Vector4 ClearColor = new Vector4(0.21f, 0.21f, 0.21f, 1);

        public static ViewportRectangle ViewportRectangle;

        static Renderer() { }
        public static void Initialize() //initialization of renderer
        {
            GL.ClearColor(ClearColor.X, ClearColor.Y, ClearColor.Z, ClearColor.W);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc((BlendingFactor)BlendingFactorSrc.SrcAlpha, (BlendingFactor)BlendingFactorDest.OneMinusSrcAlpha);
            GL.Enable(EnableCap.ProgramPointSize);

            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Less);

            GL.ClearStencil(0x00);
            GL.ClearDepth(1);

            _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);

            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);

            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);


            MainFBO = new FrameBufferObject(WindowConstants.ClientSize);

            _fbos.Add(MainFBO);

            StageOneFBO = new FrameBufferObject(WindowConstants.ClientSize);
            _fbos.Add(StageOneFBO);

            StageTwoFBO = new FrameBufferObject(WindowConstants.ClientSize);
            _fbos.Add(StageTwoFBO);

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
            int uniformLocation = GL.GetUniformLocation(Shaders.SIMPLE_SHADER.Handle, "texture0");
            GL.Uniform1(uniformLocation, 0);

            Shaders.PARTICLE_SHADER.Use();
        }

        public static void RenderObject(BaseObject obj, bool setVertexData = true, bool setTextureData = true, bool setCam = true, bool setColor = true)
        {
            RenderableObject Display = obj._currentAnimation.CurrentFrame;
            if (setVertexData)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
                GL.BufferData(BufferTarget.ArrayBuffer, Display.Vertices.Length * sizeof(float), Display.Vertices, BufferUsageHint.DynamicDraw);
                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Display.Stride, 0);

                GL.BufferData(BufferTarget.ElementArrayBuffer, Display.VerticesDrawOrder.Length * sizeof(uint), Display.VerticesDrawOrder, BufferUsageHint.DynamicDraw);
            }

            if (setTextureData)
            {
                GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, Display.Stride, 3 * sizeof(float));
                GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, Display.Stride, 5 * sizeof(float)); //Normal coordinate data
                Display.Material.Diffuse.Use(TextureUnit.Texture0);
            }

            var transform = obj.BaseFrame.Transformations;


            if (setCam)
            {
                Display.ShaderReference.SetBool("enable_cam", Display.CameraPerspective);
            }
            if (setColor)
            {
                Display.ShaderReference.SetVector4("aColor", obj.BaseFrame.InterpolatedColor);
            }


            Display.ShaderReference.SetMatrix4("transform", transform);

            //Display.ShaderReference.SetFloat("fMixPercent", obj._baseFrame.ColorProportion);

            GL.DrawElements(PrimitiveType.Triangles, Display.VerticesDrawOrder.Length, DrawElementsType.UnsignedInt, new IntPtr());

            DrawCount++;
        }

        public static void PrepareInstancedRenderFunc(RenderableObject Display) 
        {
            EnableInstancedShaderAttributes();


            GL.BindBuffer(BufferTarget.ArrayBuffer, _instancedVertexBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, Display.Vertices.Length * sizeof(float), Display.Vertices, BufferUsageHint.DynamicDraw); //take the raw vertices


            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, Display.VerticesDrawOrder.Length * sizeof(uint), Display.VerticesDrawOrder, BufferUsageHint.DynamicDraw);

            //Whenever this data is changed the instanceDataOffset parameter needs to be updated
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Display.Stride, 0); //vertex data
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, Display.Stride, 3 * sizeof(float)); //Texture coordinate data
            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, Display.Stride, 5 * sizeof(float)); //Normal coordinate data

            GL.BindBuffer(BufferTarget.ArrayBuffer, _instancedArrayBuffer);

            GL.VertexAttribPointer(3, 4, VertexAttribPointerType.Float, false, instanceDataLength, 0);                  //| Transformation matrix data
            GL.VertexAttribPointer(4, 4, VertexAttribPointerType.Float, false, instanceDataLength, 4 * sizeof(float));  //|
            GL.VertexAttribPointer(5, 4, VertexAttribPointerType.Float, false, instanceDataLength, 8 * sizeof(float));  //|
            GL.VertexAttribPointer(6, 4, VertexAttribPointerType.Float, false, instanceDataLength, 12 * sizeof(float)); //|

            GL.VertexAttribPointer(7, 4, VertexAttribPointerType.Float, false, instanceDataLength, 16 * sizeof(float)); //Color data
            GL.VertexAttribPointer(8, 4, VertexAttribPointerType.Float, false, instanceDataLength, 20 * sizeof(float)); //Camera enabled + spritesheet position + side lengths X and Y
            GL.VertexAttribPointer(9, 4, VertexAttribPointerType.Float, false, instanceDataLength, 24 * sizeof(float)); //Spritesheet X + Spritesheet Y + use second texture + mix percent

            GL.VertexAttribPointer(10, 4, VertexAttribPointerType.Float, false, instanceDataLength, 28 * sizeof(float)); //Inline thickness + outline thickness + alpha threshold + primary texture target
            GL.VertexAttribPointer(11, 4, VertexAttribPointerType.Float, false, instanceDataLength, 32 * sizeof(float)); //Inline color
            GL.VertexAttribPointer(12, 4, VertexAttribPointerType.Float, false, instanceDataLength, 36 * sizeof(float)); //Lighting parameters
        }

        public static void PrepareParticleRenderFunc(RenderableObject Display)
        {
            EnableParticleShaderAttributes();

            GL.BindBuffer(BufferTarget.ArrayBuffer, _instancedVertexBuffer);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);

            GL.BufferData(BufferTarget.ArrayBuffer, Display.Vertices.Length * sizeof(float), Display.Vertices, BufferUsageHint.DynamicDraw); //take the raw vertices
            GL.BufferData(BufferTarget.ElementArrayBuffer, Display.VerticesDrawOrder.Length * sizeof(uint), Display.VerticesDrawOrder, BufferUsageHint.DynamicDraw);


            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Display.Stride, 0); //vertex data
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, Display.Stride, 3 * sizeof(float)); //Texture coordinate data

            GL.BindBuffer(BufferTarget.ArrayBuffer, _instancedArrayBuffer);

            GL.VertexAttribPointer(2, 4, VertexAttribPointerType.Float, false, particleDataLength, 0);                  //| Transformation matrix data
            GL.VertexAttribPointer(3, 4, VertexAttribPointerType.Float, false, particleDataLength, 4 * sizeof(float));  //|
            GL.VertexAttribPointer(4, 4, VertexAttribPointerType.Float, false, particleDataLength, 8 * sizeof(float));  //|
            GL.VertexAttribPointer(5, 4, VertexAttribPointerType.Float, false, particleDataLength, 12 * sizeof(float)); //|

            GL.VertexAttribPointer(6, 4, VertexAttribPointerType.Float, false, particleDataLength, 16 * sizeof(float)); //Color data
            GL.VertexAttribPointer(7, 4, VertexAttribPointerType.Float, false, particleDataLength, 20 * sizeof(float)); //empty + spritesheet position + side lengths X and Y
            GL.VertexAttribPointer(8, 4, VertexAttribPointerType.Float, false, particleDataLength, 24 * sizeof(float)); //Spritesheet X + Spritesheet Y
        }

        public static void InsertDataIntoInstancedRenderArray(BaseObject obj, MultiTextureData renderingData, ref float[] _instancedRenderArray, ref int currIndex, int textureTarget)
        {
            var transform = obj.BaseFrame.Transformations;

            InsertMatrixDataIntoArray(ref _instancedRenderArray, ref transform, currIndex);
            currIndex += 16;
            _instancedRenderArray[currIndex++] = obj.BaseFrame.InterpolatedColor.X;
            _instancedRenderArray[currIndex++] = obj.BaseFrame.InterpolatedColor.Y;
            _instancedRenderArray[currIndex++] = obj.BaseFrame.InterpolatedColor.Z;
            _instancedRenderArray[currIndex++] = obj.BaseFrame.InterpolatedColor.W;

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

            _instancedRenderArray[currIndex++] = obj.EnableLighting ? 1 : 0;
            _instancedRenderArray[currIndex++] = 0;
            _instancedRenderArray[currIndex++] = 0;
            _instancedRenderArray[currIndex++] = 0;
        }

        public static void RenderObjectsInstancedGeneric<T>(List<T> objects, ref float[] _instancedRenderDataArray, RenderableObject display = null, bool instantiateRenderFunc = true, bool enableLighting = true) where T : GameObject
        {
            if (objects.Count == 0)
                return;

            if (objects[0] == null)
                return;

            Shaders.FAST_DEFAULT_SHADER.Use();

            //Shaders.FAST_DEFAULT_SHADER.SetFloat("enableLighting", enableLighting ? 1 : 0);

            RenderableObject Display = null;

            if (display == null)
            {
                for(int i = 0; i < objects.Count; i++)
                {
                    if (objects[i].TextureLoaded && objects[i].BaseObjects.Count > 0)
                    {
                        Display = objects[i].BaseObjects[0]._currentAnimation.CurrentFrame;
                        break;
                    }
                }
                //Display = objects[0].BaseObjects[0]._currentAnimation.CurrentFrame;
            }
            else
            {
                Display = display;
            }

            if (Display == null)
                return;

            //for (int i = 0; i < 8; i++)
            //{
            //    Shaders.FAST_DEFAULT_SHADER.SetInt($"material[{i}].specular", 2);
            //    Shaders.FAST_DEFAULT_SHADER.SetInt($"material[{i}].diffuse", 2);
            //    Shaders.FAST_DEFAULT_SHADER.SetFloat($"material[{i}].shininess", 16);
            //}

            int currTexture = Display.Textures.TextureIds[0];
            Display.Material.Diffuse.Use(TextureUnit.Texture0);
            Shaders.FAST_DEFAULT_SHADER.SetInt("material[0].specular", 0);
            Shaders.FAST_DEFAULT_SHADER.SetInt("material[0].diffuse", 0);
            Shaders.FAST_DEFAULT_SHADER.SetFloat("material[0].shininess", 16);

            //Shaders.FAST_DEFAULT_SHADER.SetInt("texture1", 1);

            PrepareInstancedRenderFunc(Display);

            int currIndex = 0;

            int count = 0;
            List<T> recursiveCallList = new List<T>();

            Dictionary<int, TextureUnit> usedTextures = new Dictionary<int, TextureUnit>();
            Dictionary<Texture, TextureUnit> textureReferences = new Dictionary<Texture, TextureUnit>();

            List<T> scissorCallList = new List<T>();

            usedTextures.Add(currTexture, TextureUnit.Texture0);
            textureReferences.Add(Display.Material.Diffuse, TextureUnit.Texture0);

            BaseObject obj;
            int texId;

            TextureUnit currentTextureUnit = TextureUnit.Texture1;

            void draw(int itemCount, ref float[] renderDataArray) 
            {
                GL.BufferData(BufferTarget.ArrayBuffer, itemCount * instanceDataOffset * sizeof(float), renderDataArray, BufferUsageHint.DynamicDraw);

                GL.DrawElementsInstanced(PrimitiveType.Triangles, Display.VerticesDrawOrder.Length, DrawElementsType.UnsignedInt, new IntPtr(), itemCount);

                DrawCount++;
                ObjectsDrawn += itemCount;
            }

            for (int i = 0; i < objects.Count; i++)
            {
                if (objects[i] != null && objects[i].TextureLoaded && objects[i].Render && !objects[i].Cull)
                {
                    if (objects[i].ScissorData.Scissor && objects[i].ScissorData._scissorFlag) 
                    {
                        draw(count, ref _instancedRenderDataArray);
                        count = 0;
                        currIndex = 0;

                        scissorCallList.Add(objects[i]);
                        objects[i].ScissorData._scissorFlag = false;

                        ScissorData scissorData = scissorCallList[^1].ScissorData;
                        GL.Scissor(
                            (int)((float)(scissorData.X) / WindowConstants.ClientSize.X * ViewportRectangle.Width) + ViewportRectangle.X,
                            (int)((float)(scissorData.Y) / WindowConstants.ClientSize.Y * ViewportRectangle.Height) + ViewportRectangle.Y, 
                            (int)((float)scissorData.Width / WindowConstants.ClientSize.X * ViewportRectangle.Width),
                            (int)((float)scissorData.Height / WindowConstants.ClientSize.Y * ViewportRectangle.Height));
                        GL.Enable(EnableCap.ScissorTest);

                        textureReferences.Clear();
                        usedTextures.Clear();

                        RenderObjectsInstancedGeneric(scissorCallList, ref _instancedRenderDataArray, null, false, enableLighting);

                        currentTextureUnit = TextureUnit.Texture0;
                        currTexture = int.MaxValue;

                        GL.Disable(EnableCap.ScissorTest);
 
                        scissorCallList.Clear();

                        EnableInstancedShaderAttributes();
                    }
                    else if (objects[i].ScissorData._scissorFlag)
                    {
                        scissorCallList.Add(objects[i]);
                        objects[i].ScissorData._scissorFlag = false;
                    }
                    else
                    {
                        int objCount = 0;
                        BaseObject[] baseObjects = new BaseObject[objects[i].BaseObjects.Count];
                        int[] texIds = new int[objects[i].BaseObjects.Count];
                        bool multipleBaseObjects = objects[i].BaseObjects.Count > 1;

                        for (int j = 0; j < objects[i].BaseObjects.Count; j++)
                        {
                            if (objects[i].BaseObjects[j].Render)
                            {
                                if (objects[i].BaseObjects[j].BaseFrame.VerticeType != Display.VerticeType) 
                                {
                                    if (!multipleBaseObjects)
                                        recursiveCallList.Add(objects[i]);
                                    continue;
                                }

                                obj = objects[i].BaseObjects[j];
                                texId = obj._currentAnimation.CurrentFrame.Textures.TextureIds[0];

                                texIds[j] = texId;

                                if (texId != currTexture)
                                {
                                    if (!usedTextures.ContainsKey(texId))
                                    {
                                        if (currentTextureUnit > TextureUnit.Texture7)
                                        {
                                            recursiveCallList.Add(objects[i]);
                                            continue;
                                        }
                                        else
                                        {
                                            usedTextures.Add(texId, currentTextureUnit);
                                            textureReferences.Add(obj._currentAnimation.CurrentFrame.Material.Diffuse, currentTextureUnit);

                                            obj._currentAnimation.CurrentFrame.Material.Diffuse.Use(currentTextureUnit);

                                            int texIndex = (int)currentTextureUnit - 33984;

                                            Shaders.FAST_DEFAULT_SHADER.SetInt($"material[{texIndex}].diffuse", texIndex);
                                            Shaders.FAST_DEFAULT_SHADER.SetInt($"material[{texIndex}].specular", texIndex);
                                            Shaders.FAST_DEFAULT_SHADER.SetFloat($"material[{texIndex}].shininess", 16);

                                            currentTextureUnit++;
                                        }
                                    }
                                }
                                baseObjects[j] = objects[i].BaseObjects[j];

                                objCount++;

                                if(!multipleBaseObjects)
                                {
                                    if (count == ObjectBufferCount)
                                    {
                                        recursiveCallList.Add(objects[i]);
                                    }
                                    else
                                    {
                                        InsertDataIntoInstancedRenderArray(obj, objects[i].MultiTextureData, ref _instancedRenderDataArray, ref currIndex, (usedTextures[texId] - TextureUnit.Texture0));

                                        count++;
                                    }
                                }
                            }
                        }

                        if (multipleBaseObjects)
                        {
                            if (objCount == objects[i].BaseObjects.Count)
                            {
                                if (count + baseObjects.Length >= ObjectBufferCount)
                                {
                                    recursiveCallList.Add(objects[i]);
                                }
                                else
                                {
                                    for (int k = 0; k < baseObjects.Length; k++)
                                    {
                                        InsertDataIntoInstancedRenderArray(baseObjects[k], objects[i].MultiTextureData, ref _instancedRenderDataArray, ref currIndex, (usedTextures[texIds[k]] - TextureUnit.Texture0));

                                        count++;
                                    }
                                }
                            }
                            else
                            {
                                recursiveCallList.Add(objects[i]);
                            }
                        }
                    }
                }
            }

            draw(count, ref _instancedRenderDataArray);

            DisableInstancedShaderAttributes();


            if (recursiveCallList.Count > 0)
            {
                RenderObjectsInstancedGeneric(recursiveCallList, ref _instancedRenderDataArray, enableLighting: enableLighting);
            }
        }

        public static void RenderInstancedRenderData<T>(List<T> data) where T : InstancedRenderData
        {
            for (int i = 0; i < data.Count; i++)
            {
                if (!data[i].IsValid)
                    continue;

                data[i].Shader.Use();
                //data[i].Shader.SetFloat("enableLighting", data[i].EnableLighting ? 1 : 0);

                if (data[i].ScissorData.Scissor)
                {
                    GL.Enable(EnableCap.ScissorTest);
                    GL.Scissor(data[i].ScissorData.X, data[i].ScissorData.Y, data[i].ScissorData.Width, data[i].ScissorData.Height);
                }

                foreach (var Tex in data[i].Textures)
                {
                    Tex.Key.Use(Tex.Value);

                    int texIndex = (int)Tex.Value - 33984;
                    //int materialIndex = texIndex > 0 ? texIndex - 1 : 0;
                    data[i].Shader.SetInt($"material[{texIndex}].diffuse", texIndex);
                    data[i].Shader.SetInt($"material[{texIndex}].specular", 16);
                    data[i].Shader.SetFloat($"material[{texIndex}].shininess", 16);
                }

                data[i].EnableInstancedShaderAttributes();
                data[i].PrepareInstancedRenderFunc();


                GL.DrawElementsInstanced(PrimitiveType.Triangles, data[i].VerticesCount, DrawElementsType.UnsignedInt, new IntPtr(), data[i].ItemCount);

                DrawCount++;
                ObjectsDrawn += data[i].ItemCount;

                GL.Disable(EnableCap.ScissorTest);
            }

            if(data.Count > 0)
            {
                data[0].DisableInstancedShaderAttributes();
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
            int currTexture = Display.Textures.TextureIds[0];


            Display.Material.Diffuse.Use(TextureUnit.Texture0);


            PrepareInstancedRenderFunc(Display);


            int currIndex = 0;

            int count = 0;
            List<BaseObject> recursiveCallList = new List<BaseObject>();

            List<BaseObject> difTextureCallList = new List<BaseObject>();

            List<MultiTextureData> multiTextureList = new List<MultiTextureData>();

            int texId;

            int objIndex = 0;

            objects.ForEach(obj =>
            {
                if (obj.Render)
                {
                    texId = obj._currentAnimation.CurrentFrame.Textures.TextureIds[0];
                    if (count == ObjectBufferCount)
                    {
                        recursiveCallList.Add(obj);
                        multiTextureList.Add(multiTextureData[objIndex]);
                    }
                    else if (texId != currTexture)
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
            ObjectsDrawn += count;
        }


        static readonly float[] g_quad_vertex_buffer_data = new float[]{
            -1.0f, -1.0f, 0.0f,
            1.0f, -1.0f, 0.0f,
            -1.0f,  1.0f, 0.0f,
            -1.0f,  1.0f, 0.0f,
            1.0f, -1.0f, 0.0f,
            1.0f,  1.0f, 0.0f,
        };

        public static void RenderFrameBuffer(FrameBufferObject frameBuffer, FrameBufferObject destinationBuffer = null, Shader shader = null) 
        {
            //Bind the texture that the objects were rendered on to
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, frameBuffer.RenderTexture);


            //empty out the depth buffer for when we reuse this frame buffer
            //frameBuffer.ClearBuffers();

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


            if (RenderingQueue.RenderStateManager.GetFlag(RenderingStates.Fade))
            {
                Shaders.SIMPLE_SHADER.SetFloat("Alpha", RenderFunctions.FadeParameters.Alpha);
            }
            else
            {
                Shaders.SIMPLE_SHADER.SetFloat("Alpha", 1);
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

            Shaders.PARTICLE_SHADER.Use();

            RenderableObject Display = generator.ParticleDisplay;

            Display.Material.Diffuse.Use(TextureUnit.Texture0);

            PrepareParticleRenderFunc(Display);

            int currIndex = 0;

            int count = 0;

            float sideLengthX;
            float sideLengthY;
            float spritesheetPosition;
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

                    spritesheetWidth = Display.Textures.Spritesheet.Columns;
                    spritesheetHeight = Display.Textures.Spritesheet.Rows;

                    InsertMatrixDataIntoArray(ref _instancedRenderArray, ref transform, currIndex);
                    currIndex += 16;
                    _instancedRenderArray[currIndex++] = obj.Color.X;
                    _instancedRenderArray[currIndex++] = obj.Color.Y;
                    _instancedRenderArray[currIndex++] = obj.Color.Z;
                    _instancedRenderArray[currIndex++] = obj.Color.W;

                    _instancedRenderArray[currIndex++] = 0;
                    _instancedRenderArray[currIndex++] = spritesheetPosition;
                    _instancedRenderArray[currIndex++] = sideLengthX;
                    _instancedRenderArray[currIndex++] = sideLengthY;

                    _instancedRenderArray[currIndex++] = spritesheetWidth;
                    _instancedRenderArray[currIndex++] = spritesheetHeight;
                    _instancedRenderArray[currIndex++] = 0;
                    _instancedRenderArray[currIndex++] = 0;

                    count++;
                }
            }

            GL.BufferData(BufferTarget.ArrayBuffer, count * particleDataOffset * sizeof(float), _instancedRenderArray, BufferUsageHint.DynamicDraw);
            GL.DrawElementsInstanced(PrimitiveType.Triangles, Display.VerticesDrawOrder.Length, DrawElementsType.UnsignedInt, new IntPtr(), count);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);


            DisableParticleShaderAttributes();

            DrawCount++;
            ObjectsDrawn += count;
        }

        private static float[] skyboxVertices = new float[]
        {
            // positions          
            -1.0f,  1.0f, -1.0f,
            -1.0f, -1.0f, -1.0f,
             1.0f, -1.0f, -1.0f,
             1.0f, -1.0f, -1.0f,
             1.0f,  1.0f, -1.0f,
            -1.0f,  1.0f, -1.0f,

            -1.0f, -1.0f,  1.0f,
            -1.0f, -1.0f, -1.0f,
            -1.0f,  1.0f, -1.0f,
            -1.0f,  1.0f, -1.0f,
            -1.0f,  1.0f,  1.0f,
            -1.0f, -1.0f,  1.0f,

             1.0f, -1.0f, -1.0f,
             1.0f, -1.0f,  1.0f,
             1.0f,  1.0f,  1.0f,
             1.0f,  1.0f,  1.0f,
             1.0f,  1.0f, -1.0f,
             1.0f, -1.0f, -1.0f,

            -1.0f, -1.0f,  1.0f,
            -1.0f,  1.0f,  1.0f,
             1.0f,  1.0f,  1.0f,
             1.0f,  1.0f,  1.0f,
             1.0f, -1.0f,  1.0f,
            -1.0f, -1.0f,  1.0f,

            -1.0f,  1.0f, -1.0f,
             1.0f,  1.0f, -1.0f,
             1.0f,  1.0f,  1.0f,
             1.0f,  1.0f,  1.0f,
            -1.0f,  1.0f,  1.0f,
            -1.0f,  1.0f, -1.0f,

            -1.0f, -1.0f, -1.0f,
            -1.0f, -1.0f,  1.0f,
             1.0f, -1.0f, -1.0f,
             1.0f, -1.0f, -1.0f,
            -1.0f, -1.0f,  1.0f,
             1.0f, -1.0f,  1.0f
        };
        public static void RenderSkybox(CubeMap map) 
        {
            Shaders.SKYBOX_SHADER.Use();




            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, skyboxVertices.Length * sizeof(float), skyboxVertices, BufferUsageHint.DynamicDraw);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture3D, map.Handle);
            Shaders.SKYBOX_SHADER.SetInt("texture0", 0);


            GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
        }

        public static void RenderTextInstanced(List<Letter> objects, ref float[] _instancedRenderDataArray) 
        {
            if (objects.Count == 0)
                return;

            Shaders.TEXT_SHADER.Use();

            const int textInstanceDataOffset = 28;
            const int textInstanceDataLength = textInstanceDataOffset * sizeof(float);


            RenderableObject Display = objects[0].BaseObjects[0]._currentAnimation.CurrentFrame;

            Display.Material.Diffuse.Use(TextureUnit.Texture0);
            Shaders.TEXT_SHADER.SetInt("texture0", 0);

            #region enable instanced attributes
            GL.EnableVertexAttribArray(2);
            GL.EnableVertexAttribArray(3);
            GL.EnableVertexAttribArray(4);
            GL.EnableVertexAttribArray(5);
            GL.EnableVertexAttribArray(6);
            GL.EnableVertexAttribArray(7);
            GL.EnableVertexAttribArray(8);
            GL.EnableVertexAttribArray(9);
            GL.VertexAttribDivisor(3, 1);
            GL.VertexAttribDivisor(4, 1);
            GL.VertexAttribDivisor(5, 1);
            GL.VertexAttribDivisor(6, 1);
            GL.VertexAttribDivisor(7, 1);
            GL.VertexAttribDivisor(8, 1);
            GL.VertexAttribDivisor(9, 1);
            #endregion

            #region prepare for render
            GL.BindBuffer(BufferTarget.ArrayBuffer, _instancedVertexBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, Display.Vertices.Length * sizeof(float), Display.Vertices, BufferUsageHint.DynamicDraw); //take the raw vertices

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, Display.VerticesDrawOrder.Length * sizeof(uint), Display.VerticesDrawOrder, BufferUsageHint.DynamicDraw);

            //Whenever this data is changed the instanceDataOffset parameter needs to be updated
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Display.Stride, 0); //vertex data
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, Display.Stride, 3 * sizeof(float)); //Texture coordinate data
            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, Display.Stride, 5 * sizeof(float)); //Normal coordinate data

            GL.BindBuffer(BufferTarget.ArrayBuffer, _instancedArrayBuffer);

            GL.VertexAttribPointer(3, 4, VertexAttribPointerType.Float, false, textInstanceDataLength, 0);                  //| Transformation matrix data
            GL.VertexAttribPointer(4, 4, VertexAttribPointerType.Float, false, textInstanceDataLength, 4 * sizeof(float));  //|
            GL.VertexAttribPointer(5, 4, VertexAttribPointerType.Float, false, textInstanceDataLength, 8 * sizeof(float));  //|
            GL.VertexAttribPointer(6, 4, VertexAttribPointerType.Float, false, textInstanceDataLength, 12 * sizeof(float)); //|

            GL.VertexAttribPointer(7, 4, VertexAttribPointerType.Float, false, textInstanceDataLength, 16 * sizeof(float)); //Color data
            GL.VertexAttribPointer(8, 4, VertexAttribPointerType.Float, false, textInstanceDataLength, 20 * sizeof(float)); //spritesheet position, alpha threshold
            GL.VertexAttribPointer(9, 4, VertexAttribPointerType.Float, false, textInstanceDataLength, 24 * sizeof(float)); //scissor X, scissor Y, scissow Width, scissor Height
            #endregion

            void InsertDataIntoArray(BaseObject obj, Letter letter, ref float[] _instancedRenderArray, ref int currIndex) 
            {
                var transform = obj.BaseFrame.Transformations;

                InsertMatrixDataIntoArray(ref _instancedRenderArray, ref transform, currIndex);
                currIndex += 16;
                _instancedRenderArray[currIndex++] = obj.BaseFrame.InterpolatedColor.X;
                _instancedRenderArray[currIndex++] = obj.BaseFrame.InterpolatedColor.Y;
                _instancedRenderArray[currIndex++] = obj.BaseFrame.InterpolatedColor.Z;
                _instancedRenderArray[currIndex++] = obj.BaseFrame.InterpolatedColor.W;

                _instancedRenderArray[currIndex++] = obj._currentAnimation.CurrentFrame.SpritesheetPosition;
                _instancedRenderArray[currIndex++] = obj.RenderData.AlphaThreshold;
                _instancedRenderArray[currIndex++] = letter.TextRenderData.Outline ? 1 : 0;
                _instancedRenderArray[currIndex++] = letter.TextRenderData.Bold ? 1 : 0;

                _instancedRenderArray[currIndex++] = (float)letter.ScissorData.X / WindowConstants.ClientSize.X;
                _instancedRenderArray[currIndex++] = (float)letter.ScissorData.Y / WindowConstants.ClientSize.Y;
                _instancedRenderArray[currIndex++] = (float)letter.ScissorData.Width / WindowConstants.ClientSize.X;
                _instancedRenderArray[currIndex++] = (float)letter.ScissorData.Height / WindowConstants.ClientSize.Y;
            }


            int currIndex = 0;

            int count = 0;
            List<Letter> recursiveCallList = new List<Letter>();

            BaseObject obj;

            void draw(int itemCount, float[] renderDataArray)
            {
                GL.BufferData(BufferTarget.ArrayBuffer, itemCount * textInstanceDataOffset * sizeof(float), renderDataArray, BufferUsageHint.DynamicDraw);

                GL.DrawElementsInstanced(PrimitiveType.Triangles, Display.VerticesDrawOrder.Length, DrawElementsType.UnsignedInt, new IntPtr(), itemCount);

                DrawCount++;
                ObjectsDrawn += itemCount;
            }

            for (int i = 0; i < objects.Count; i++)
            {
                if (objects[i] != null && objects[i].TextureLoaded && objects[i].Render && !objects[i].Cull)
                {
                    for (int j = 0; j < objects[i].BaseObjects.Count; j++)
                    {
                        if (objects[i].BaseObjects[j].Render)
                        {
                            if(objects[i].ScissorData.Width > 0) 
                            {

                            }

                            obj = objects[i].BaseObjects[j];

                            if (count == ObjectBufferCount)
                            {
                                recursiveCallList.Add(objects[i]);
                            }
                            else
                            {
                                InsertDataIntoArray(obj, objects[i], ref _instancedRenderDataArray, ref currIndex);

                                count++;
                            }
                        }
                    }
                }
            }

            draw(count, _instancedRenderDataArray);

            #region disable instanced attributes
            GL.DisableVertexAttribArray(2);
            GL.DisableVertexAttribArray(3);
            GL.DisableVertexAttribArray(4);
            GL.DisableVertexAttribArray(5);
            GL.DisableVertexAttribArray(6);
            GL.DisableVertexAttribArray(7);
            GL.DisableVertexAttribArray(8);
            GL.DisableVertexAttribArray(9);
            GL.VertexAttribDivisor(3, 0);
            GL.VertexAttribDivisor(4, 0);
            GL.VertexAttribDivisor(5, 0);
            GL.VertexAttribDivisor(6, 0);
            GL.VertexAttribDivisor(7, 0);
            GL.VertexAttribDivisor(8, 0);
            GL.VertexAttribDivisor(9, 0);
            #endregion


            if (recursiveCallList.Count > 0)
            {
                RenderTextInstanced(recursiveCallList, ref _instancedRenderDataArray);
            }
        }

        #region Load/unload textures
        public static void LoadTextureFromGameObj<T>(T gameObj, bool nearest = true, bool generateMipMaps = true) where T : GameObject 
        {
            if (!gameObj._canLoadTexture)
                return;

            gameObj.BaseObjects.ForEach(obj =>
            {
                foreach (KeyValuePair<AnimationType, Animation> entry in obj.Animations)
                {
                    for (int o = 0; o < entry.Value.Frames.Count; o++)
                    {
                        for (int p = 0; p < entry.Value.Frames[o].Textures.TextureIds.Length; p++)
                        {
                            if (entry.Value.Frames[o].Material.Diffuse == null) 
                            {
                                if (!_loadedTextures.TryGetValue(entry.Value.Frames[o].Textures.TextureIds[p], out int handle))
                                {
                                    Texture newTexture = Texture.LoadFromFile(entry.Value.Frames[o].Textures.TextureFilenames[p], nearest, generateMipMaps: generateMipMaps);
                                    newTexture.TextureId = entry.Value.Frames[o].Textures.TextureIds[p];

                                    _textures.Add(newTexture);
                                    _loadedTextures.Add(entry.Value.Frames[o].Textures.TextureIds[p], newTexture.Handle);

                                    entry.Value.Frames[o].Material.Diffuse = newTexture;
                                }
                                else
                                {
                                    entry.Value.Frames[o].Material.Diffuse = new Texture(handle, entry.Value.Frames[o].Textures.TextureIds[p]);
                                }

                                gameObj.SetTextureLoaded(true);
                            }
                            else
                            {
                                gameObj.SetTextureLoaded(true);
                            }
                        }
                    }
                }
            });
        }
        public static void LoadTextureFromBaseObject(BaseObject obj, bool nearest = true, bool generateMipMaps = true)
        {
            foreach (KeyValuePair<AnimationType, Animation> entry in obj.Animations)
            {
                for (int o = 0; o < entry.Value.Frames.Count; o++)
                {
                    for (int p = 0; p < entry.Value.Frames[o].Textures.TextureIds.Length; p++)
                    {
                        if (entry.Value.Frames[o].Material.Diffuse == null) 
                        {
                            if (!_loadedTextures.TryGetValue(entry.Value.Frames[o].Textures.TextureIds[p], out int handle))
                            {
                                Texture newTexture = Texture.LoadFromFile(entry.Value.Frames[o].Textures.TextureFilenames[p], nearest, default, generateMipMaps);
                                newTexture.TextureId = entry.Value.Frames[o].Textures.TextureIds[p];

                                _textures.Add(newTexture);
                                _loadedTextures.Add(entry.Value.Frames[o].Textures.TextureIds[p], newTexture.Handle);

                                entry.Value.Frames[o].Material.Diffuse = newTexture;
                            }
                            else
                            {
                                entry.Value.Frames[o].Material.Diffuse = new Texture(handle, entry.Value.Frames[o].Textures.TextureIds[p]);
                            }
                        }

                    }
                }
            }
        }

        public static void LoadTextureFromRenderableObject(RenderableObject obj, bool nearest = true)
        {
            if (obj.Material.Diffuse == null || obj.Material.Diffuse.ImageData == null)
            {
                if (!_loadedTextures.TryGetValue(obj.Textures.TextureIds[0], out int handle))
                {
                    Texture newTexture = Texture.LoadFromFile(obj.Textures.TextureFilenames[0], nearest);
                    newTexture.TextureId = obj.Textures.TextureIds[0];

                    _textures.Add(newTexture);
                    _loadedTextures.Add(obj.Textures.TextureIds[0], newTexture.Handle);

                    obj.Material.Diffuse = newTexture;
                }
                else
                {
                    obj.Material.Diffuse = new Texture(handle, obj.Textures.TextureIds[0]);
                }
            }
        }

        public static void LoadTextureFromParticleGen(ParticleGenerator generator)
        {
            for (int p = 0; p < generator.ParticleDisplay.Textures.TextureIds.Length; p++)
            {
                if (!_loadedTextures.TryGetValue(generator.ParticleDisplay.Textures.TextureIds[p], out int handle))
                {
                    Texture newTexture = Texture.LoadFromFile(generator.ParticleDisplay.Textures.TextureFilenames[p]);
                    newTexture.TextureId = generator.ParticleDisplay.Textures.TextureIds[p];

                    _textures.Add(newTexture);
                    _loadedTextures.Add(generator.ParticleDisplay.Textures.TextureIds[p], newTexture.Handle);

                    generator.ParticleDisplay.Material.Diffuse = newTexture;
                }
                else
                {
                    generator.ParticleDisplay.Material.Diffuse = new Texture(handle, generator.ParticleDisplay.Textures.TextureIds[p]);
                }
            }
        }
        public static void LoadTextureFromUIObject<T>(T UIObj) where T : UIObject 
        {
            UIObj.BaseObjects.ForEach(obj => 
            {
                if (UIObj._canLoadTexture)
                {
                    LoadTextureFromBaseObject(obj, true);
                }
            });
            UIObj.TextObjects.ForEach(obj =>
            {
                for (int i = 0; i < obj.Letters.Count; i++)
                {
                    LoadTextureFromBaseObject(obj.Letters[i].BaseObjects[0], false, false);
                    obj.Letters[i].SetTextureLoaded(true);
                }
            });

            object lockObj = new object();

            if(UIObj.ManagerHandle != null)
            {
                lockObj = UIObj.ManagerHandle._UILock;
            }

            lock (lockObj)
            {
                for(int i = 0; i < UIObj.Children.Count; i++)
                {
                    LoadTextureFromUIObject(UIObj.Children[i]);
                }
            }

            if (UIObj._canLoadTexture)
            {
                UIObj.TextureLoaded = true;
            }
        }

        public static void LoadTextureFromTextureObj(Texture texture, int textureName) 
        {
            if (!_loadedTextures.TryGetValue(textureName, out _))
            {
                _textures.Add(texture);
                _loadedTextures.Add(textureName, texture.Handle);
            }
        }

        public static void LoadTextureFromSimple(SimpleTexture simp)
        {
            if (!simp.TextureLoaded)
            {
                if (!_loadedTextures.TryGetValue(simp.TextureId, out int handle))
                {
                    Texture newTexture = Texture.LoadFromFile(simp.FileName, nearest: simp.Nearest, generateMipMaps: simp.GenerateMipMaps);
                    newTexture.TextureId = simp.TextureId;

                    _textures.Add(newTexture);
                    _loadedTextures.Add(simp.TextureId, newTexture.Handle);

                    simp.Texture = newTexture;
                }
                else
                {
                    simp.Texture = new Texture(handle, simp.TextureId);
                }

                simp.TextureLoaded = true;
            }
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
            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);
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
            GL.EnableVertexAttribArray(12);
            GL.VertexAttribDivisor(3, 1);
            GL.VertexAttribDivisor(4, 1);
            GL.VertexAttribDivisor(5, 1);
            GL.VertexAttribDivisor(6, 1);
            GL.VertexAttribDivisor(7, 1);
            GL.VertexAttribDivisor(8, 1);
            GL.VertexAttribDivisor(9, 1);
            GL.VertexAttribDivisor(10, 1);
            GL.VertexAttribDivisor(11, 1);
            GL.VertexAttribDivisor(12, 1);
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
            GL.DisableVertexAttribArray(12);
            GL.VertexAttribDivisor(3, 0);
            GL.VertexAttribDivisor(4, 0);
            GL.VertexAttribDivisor(5, 0);
            GL.VertexAttribDivisor(6, 0);
            GL.VertexAttribDivisor(7, 0);
            GL.VertexAttribDivisor(8, 0);
            GL.VertexAttribDivisor(9, 0);
            GL.VertexAttribDivisor(10, 0);
            GL.VertexAttribDivisor(11, 0);
            GL.VertexAttribDivisor(12, 0);
        }

        public static void EnableParticleShaderAttributes()
        {
            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);
            GL.EnableVertexAttribArray(2);
            GL.EnableVertexAttribArray(3);
            GL.EnableVertexAttribArray(4);
            GL.EnableVertexAttribArray(5);
            GL.EnableVertexAttribArray(6);
            GL.EnableVertexAttribArray(7);
            GL.EnableVertexAttribArray(8);
            GL.VertexAttribDivisor(1, 0);
            GL.VertexAttribDivisor(2, 1);
            GL.VertexAttribDivisor(3, 1);
            GL.VertexAttribDivisor(4, 1);
            GL.VertexAttribDivisor(5, 1);
            GL.VertexAttribDivisor(6, 1);
            GL.VertexAttribDivisor(7, 1);
            GL.VertexAttribDivisor(8, 1);
        }
        public static void DisableParticleShaderAttributes()
        {
            GL.DisableVertexAttribArray(2);
            GL.DisableVertexAttribArray(3);
            GL.DisableVertexAttribArray(4);
            GL.DisableVertexAttribArray(5);
            GL.DisableVertexAttribArray(6);
            GL.DisableVertexAttribArray(7);
            GL.DisableVertexAttribArray(8);
            GL.VertexAttribDivisor(2, 0);
            GL.VertexAttribDivisor(3, 0);
            GL.VertexAttribDivisor(4, 0);
            GL.VertexAttribDivisor(5, 0);
            GL.VertexAttribDivisor(6, 0);
            GL.VertexAttribDivisor(7, 0);
            GL.VertexAttribDivisor(8, 0);
        }

        public static void InsertMatrixDataIntoArray(ref float[] arr, ref Matrix4 mat, int currIndex)
        {
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


        public delegate void RenderEventHandler();

        public static event RenderEventHandler OnRender;
        public static event RenderEventHandler OnRenderEnd;

        public static void RenderStart() 
        {
            OnRender?.Invoke();
        }

        public static void RenderEnd()
        {
            OnRenderEnd?.Invoke();
        }

        public static void CheckError()
        {
            var error = GL.GetError();

            if(error != ErrorCode.NoError)
            {

            }
        }
    }
}
