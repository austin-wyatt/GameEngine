using Empyrean.Game.Objects;
using Empyrean.Game.Tiles;
using Empyrean.Game.UI;
using Empyrean.Game.Units;
using Empyrean.Objects;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Empyrean.Engine_Classes.Rendering
{
    public static class RenderingConstants 
    {
        public static float TextAlphaThreshold = 0.0001f;
        public static float DefaultAlphaThreshold = 0.15f;
        //public static float DefaultAlphaThreshold = 0.3f;
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

        public const int GENERAL_BUFFER_SIZE_BYTES = 1024 * 500; //500kb buffer
        /// <summary>
        /// A general use GPU buffer for use in rendering operations
        /// </summary>
        public static int _generalArrayBuffer;

        private const int ObjectBufferCount = 7500;
        private const int instanceDataOffset = 40;
        public static float[] _instancedRenderArray = new float[ObjectBufferCount * instanceDataOffset];
        private const int instanceDataLength = instanceDataOffset * FLOAT_SIZE;

        private const int particleDataOffset = 28;
        private const int particleDataLength = particleDataOffset * FLOAT_SIZE;

        public static readonly ConcurrentDictionary<int, Texture> _textures = new ConcurrentDictionary<int,Texture>();
        public static readonly Dictionary<int, int> _loadedTextures = new Dictionary<int, int>();


        public static FrameBufferObject MainFBO;
        public static FrameBufferObject StageOneFBO;
        public static FrameBufferObject StageTwoFBO;

        public static List<FrameBufferObject> _fbos = new List<FrameBufferObject>();

        public static GBuffer GBuffer;

        public static int DrawCount = 0;
        public static int FPSCount = 0;
        public static int ObjectsDrawn = 0;

        public static Stopwatch _internalTimer = new Stopwatch();

        public static readonly Vector4 ClearColor = new Vector4(0.21f, 0.21f, 0.21f, 1);

        public static ViewportRectangle ViewportRectangle;

        public static readonly string[] MATERIAL_SHADER_STRINGS = new string[]
        {
            "material[0].diffuse",
            "material[0].specular",
            "material[0].shininess",
            "material[1].diffuse",
            "material[1].specular",
            "material[1].shininess",
            "material[2].diffuse",
            "material[2].specular",
            "material[2].shininess",
            "material[3].diffuse",
            "material[3].specular",
            "material[3].shininess",
            "material[4].diffuse",
            "material[4].specular",
            "material[4].shininess",
            "material[5].diffuse",
            "material[5].specular",
            "material[5].shininess",
            "material[6].diffuse",
            "material[6].specular",
            "material[6].shininess",
            "material[7].diffuse",
            "material[7].specular",
            "material[7].shininess",
            "material[8].diffuse",
            "material[8].specular",
            "material[8].shininess",
        };

        const int FLOAT_SIZE = 4;

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

            _generalArrayBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _generalArrayBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, GENERAL_BUFFER_SIZE_BYTES, IntPtr.Zero, BufferUsageHint.DynamicDraw);

            _internalTimer.Start();

            //set the texture uniform locations
            Shaders.FAST_DEFAULT_SHADER_DEFERRED.Use();

            Shaders.SIMPLE_SHADER.Use();
            int uniformLocation = GL.GetUniformLocation(Shaders.SIMPLE_SHADER.Handle, "texture0");
            GL.Uniform1(uniformLocation, 0);

            Shaders.PARTICLE_SHADER.Use();

            GBuffer = new GBuffer();
        }

        public static void RenderObject(BaseObject obj, bool setVertexData = true, bool setTextureData = true, bool setCam = true, bool setColor = true)
        {
            RenderableObject Display = obj._currentAnimation.CurrentFrame;
            if (setVertexData)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
                GL.BufferData(BufferTarget.ArrayBuffer, Display.Vertices.Length * FLOAT_SIZE, Display.Vertices, BufferUsageHint.DynamicDraw);
                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Display.Stride, 0);

                GL.BufferData(BufferTarget.ElementArrayBuffer, Display.VerticesDrawOrder.Length * sizeof(uint), Display.VerticesDrawOrder, BufferUsageHint.DynamicDraw);
            }

            if (setTextureData)
            {
                GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, Display.Stride, 3 * FLOAT_SIZE);
                GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, Display.Stride, 5 * FLOAT_SIZE); //Normal coordinate data
                Display.Material.Diffuse.Use(TextureUnit.Texture0);
            }

            var transform = obj.BaseFrame.Transformations;


            if (setCam)
            {
                Display.ShaderReference.SetBool("enable_cam", Display.CameraPerspective);
            }
            if (setColor)
            {
                Display.ShaderReference.SetVector4("aColor", ref obj.BaseFrame.InterpolatedColor);
            }


            Display.ShaderReference.SetMatrix4("transform", ref transform);

            //Display.ShaderReference.SetFloat("fMixPercent", obj._baseFrame.ColorProportion);

            GL.DrawElements(PrimitiveType.Triangles, Display.VerticesDrawOrder.Length, DrawElementsType.UnsignedInt, new IntPtr());

            DrawCount++;
        }

        private static float[] _lastVertices = null;

        public static void PrepareInstancedRenderFunc(RenderableObject Display) 
        {
            EnableInstancedShaderAttributes();

            bool alreadyBound = Display.Vertices == _lastVertices;

            GL.BindBuffer(BufferTarget.ArrayBuffer, _instancedVertexBuffer);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);

            if (!alreadyBound)
            {
                GL.BufferData(BufferTarget.ArrayBuffer, Display.Vertices.Length * FLOAT_SIZE, Display.Vertices, BufferUsageHint.DynamicDraw); //take the raw vertices

                GL.BufferData(BufferTarget.ElementArrayBuffer, Display.VerticesDrawOrder.Length * sizeof(uint), Display.VerticesDrawOrder, BufferUsageHint.DynamicDraw);
                _lastVertices = Display.Vertices;
            }

            //Whenever this data is changed the instanceDataOffset parameter needs to be updated
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Display.Stride, 0); //vertex data
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, Display.Stride, 3 * FLOAT_SIZE); //Texture coordinate data
            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, Display.Stride, 5 * FLOAT_SIZE); //Normal coordinate data

            GL.BindBuffer(BufferTarget.ArrayBuffer, _instancedArrayBuffer);

            GL.VertexAttribPointer(3, 4, VertexAttribPointerType.Float, false, instanceDataLength, 0);               //| Transformation matrix data
            GL.VertexAttribPointer(4, 4, VertexAttribPointerType.Float, false, instanceDataLength, 4 * FLOAT_SIZE);  //|
            GL.VertexAttribPointer(5, 4, VertexAttribPointerType.Float, false, instanceDataLength, 8 * FLOAT_SIZE);  //|
            GL.VertexAttribPointer(6, 4, VertexAttribPointerType.Float, false, instanceDataLength, 12 * FLOAT_SIZE); //|

            GL.VertexAttribPointer(7, 4, VertexAttribPointerType.Float, false, instanceDataLength, 16 * FLOAT_SIZE); //Color data
            GL.VertexAttribPointer(8, 4, VertexAttribPointerType.Float, false, instanceDataLength, 20 * FLOAT_SIZE); //Camera enabled + spritesheet position + side lengths X and Y
            GL.VertexAttribPointer(9, 4, VertexAttribPointerType.Float, false, instanceDataLength, 24 * FLOAT_SIZE); //Spritesheet X + Spritesheet Y + use second texture + mix percent

            GL.VertexAttribPointer(10, 4, VertexAttribPointerType.Float, false, instanceDataLength, 28 * FLOAT_SIZE); //Inline thickness + outline thickness + alpha threshold + primary texture target
            GL.VertexAttribPointer(11, 4, VertexAttribPointerType.Float, false, instanceDataLength, 32 * FLOAT_SIZE); //Inline color
            GL.VertexAttribPointer(12, 4, VertexAttribPointerType.Float, false, instanceDataLength, 36 * FLOAT_SIZE); //Lighting parameters
        }

        public static void PrepareParticleRenderFunc(RenderableObject Display)
        {
            EnableParticleShaderAttributes();

            GL.BindBuffer(BufferTarget.ArrayBuffer, _instancedVertexBuffer);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);

            GL.BufferData(BufferTarget.ArrayBuffer, Display.Vertices.Length * FLOAT_SIZE, Display.Vertices, BufferUsageHint.DynamicDraw); //take the raw vertices
            GL.BufferData(BufferTarget.ElementArrayBuffer, Display.VerticesDrawOrder.Length * sizeof(uint), Display.VerticesDrawOrder, BufferUsageHint.DynamicDraw);


            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Display.Stride, 0); //vertex data
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, Display.Stride, 3 * FLOAT_SIZE); //Texture coordinate data
            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, Display.Stride, 5 * FLOAT_SIZE); //Normal coordinate data

            GL.BindBuffer(BufferTarget.ArrayBuffer, _instancedArrayBuffer);

            GL.VertexAttribPointer(3, 4, VertexAttribPointerType.Float, false, particleDataLength, 0);                  //| Transformation matrix data
            GL.VertexAttribPointer(4, 4, VertexAttribPointerType.Float, false, particleDataLength, 4 * FLOAT_SIZE);  //|
            GL.VertexAttribPointer(5, 4, VertexAttribPointerType.Float, false, particleDataLength, 8 * FLOAT_SIZE);  //|
            GL.VertexAttribPointer(6, 4, VertexAttribPointerType.Float, false, particleDataLength, 12 * FLOAT_SIZE); //|

            GL.VertexAttribPointer(7, 4, VertexAttribPointerType.Float, false, particleDataLength, 16 * FLOAT_SIZE); //Color data
            GL.VertexAttribPointer(8, 4, VertexAttribPointerType.Float, false, particleDataLength, 20 * FLOAT_SIZE); //empty + spritesheet position + side lengths X and Y
            GL.VertexAttribPointer(9, 4, VertexAttribPointerType.Float, false, particleDataLength, 24 * FLOAT_SIZE); //Spritesheet X + Spritesheet Y
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

        private static ObjectPool<Dictionary<int, TextureUnit>> _usedTexturesPool = new ObjectPool<Dictionary<int, TextureUnit>>();
        private static ObjectPool<Dictionary<Texture, TextureUnit>> _textureReferencesPool = new ObjectPool<Dictionary<Texture, TextureUnit>>();
        private static ObjectPool<List<object>> _objectListPool = new ObjectPool<List<object>>();
        public static void RenderObjectsInstancedGeneric<T>(List<T> objects, ref float[] _instancedRenderDataArray, RenderableObject display = null, bool instantiateRenderFunc = true,
            bool enableLighting = true, bool deferredShading = true) where T : GameObject
        {
            if (objects.Count == 0 || objects[0] == null)
                return;

            Shader shader;

            if (deferredShading)
            {
                shader = Shaders.FAST_DEFAULT_SHADER_DEFERRED;
            }
            else
            {
                shader = Shaders.FAST_DEFAULT_SHADER_IMMEDIATE;
            }

            shader.Use();

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
            }
            else
            {
                Display = display;
            }

            if (Display == null)
                return;

            int currTexture = Display.Textures.TextureIds[0];
            Display.Material.Diffuse.Use(TextureUnit.Texture0);
            shader.SetInt(MATERIAL_SHADER_STRINGS[0], 0);
            shader.SetInt(MATERIAL_SHADER_STRINGS[1], 0);
            shader.SetFloat(MATERIAL_SHADER_STRINGS[2], 16);

            //Shaders.FAST_DEFAULT_SHADER.SetInt("texture1", 1);

            PrepareInstancedRenderFunc(Display);

            int currIndex = 0;

            int count = 0;
            List<object> recursiveCallList = _objectListPool.GetObject();

            Dictionary<int, TextureUnit> usedTextures = _usedTexturesPool.GetObject();
            Dictionary<Texture, TextureUnit> textureReferences = _textureReferencesPool.GetObject();

            usedTextures.Add(currTexture, TextureUnit.Texture0);
            textureReferences.Add(Display.Material.Diffuse, TextureUnit.Texture0);

            BaseObject obj;
            int texId;

            TextureUnit currentTextureUnit = TextureUnit.Texture1;

            for (int i = 0; i < objects.Count; i++)
            {
                if (objects[i] != null && objects[i].TextureLoaded && objects[i].Render && !objects[i].Cull)
                {
                    
                    bool multipleBaseObjects = objects[i].BaseObjects.Count > 1;

                    if (multipleBaseObjects)
                    {
                        throw new Exception("Multiple base objects");
                    }

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

                                        shader.SetInt(MATERIAL_SHADER_STRINGS[texIndex * 3], texIndex);
                                        shader.SetInt(MATERIAL_SHADER_STRINGS[texIndex * 3 + 1], texIndex);
                                        shader.SetFloat(MATERIAL_SHADER_STRINGS[texIndex * 3 + 2], 16);

                                        currentTextureUnit++;
                                    }
                                }
                            }
                                
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
            }

            //draw(count, ref _instancedRenderDataArray);

            GL.BufferData(BufferTarget.ArrayBuffer, count * instanceDataOffset * FLOAT_SIZE, _instancedRenderDataArray, BufferUsageHint.DynamicDraw);
            GL.DrawElementsInstanced(PrimitiveType.Triangles, Display.VerticesDrawOrder.Length, DrawElementsType.UnsignedInt, IntPtr.Zero, count);
            DrawCount++;
            ObjectsDrawn += count;


            DisableInstancedShaderAttributes();


            if (recursiveCallList.Count > 0)
            {
                RenderObjectsInstancedGeneric(recursiveCallList, ref _instancedRenderDataArray, enableLighting: enableLighting, deferredShading: deferredShading);
            }

            usedTextures.Clear();
            _usedTexturesPool.FreeObject(ref usedTextures);

            textureReferences.Clear();
            _textureReferencesPool.FreeObject(ref textureReferences);

            recursiveCallList.Clear();
            _objectListPool.FreeObject(ref recursiveCallList);
        }

        public static void RenderObjectsInstancedGeneric(List<object> objects, ref float[] _instancedRenderDataArray, RenderableObject display = null, bool instantiateRenderFunc = true,
            bool enableLighting = true, bool deferredShading = true)
        {
            if (objects.Count == 0 || objects[0] == null)
                return;

            Shader shader;

            if (deferredShading)
            {
                shader = Shaders.FAST_DEFAULT_SHADER_DEFERRED;
            }
            else
            {
                shader = Shaders.FAST_DEFAULT_SHADER_IMMEDIATE;
            }

            shader.Use();

            RenderableObject Display = null;

            GameObject gameObject;

            if (display == null)
            {
                for (int i = 0; i < objects.Count; i++)
                {
                    gameObject = (GameObject)objects[i];
                    if (gameObject.TextureLoaded && gameObject.BaseObjects.Count > 0)
                    {
                        Display = gameObject.BaseObjects[0]._currentAnimation.CurrentFrame;
                        break;
                    }
                }
            }
            else
            {
                Display = display;
            }

            if (Display == null)
                return;

            int currTexture = Display.Textures.TextureIds[0];
            Display.Material.Diffuse.Use(TextureUnit.Texture0);
            shader.SetInt(MATERIAL_SHADER_STRINGS[0], 0);
            shader.SetInt(MATERIAL_SHADER_STRINGS[1], 0);
            shader.SetFloat(MATERIAL_SHADER_STRINGS[2], 16);

            //Shaders.FAST_DEFAULT_SHADER.SetInt("texture1", 1);

            PrepareInstancedRenderFunc(Display);

            int currIndex = 0;

            int count = 0;
            List<object> recursiveCallList = _objectListPool.GetObject();

            Dictionary<int, TextureUnit> usedTextures = _usedTexturesPool.GetObject();
            Dictionary<Texture, TextureUnit> textureReferences = _textureReferencesPool.GetObject();

            usedTextures.Add(currTexture, TextureUnit.Texture0);
            textureReferences.Add(Display.Material.Diffuse, TextureUnit.Texture0);

            BaseObject obj;
            int texId;

            TextureUnit currentTextureUnit = TextureUnit.Texture1;

            for (int i = 0; i < objects.Count; i++)
            {
                gameObject = (GameObject)objects[i];
                if (gameObject != null && gameObject.TextureLoaded && gameObject.Render && !gameObject.Cull)
                {
                    bool multipleBaseObjects = gameObject.BaseObjects.Count > 1;

                    if (multipleBaseObjects)
                    {
                        throw new Exception("Multiple base objects");
                    }

                    for (int j = 0; j < gameObject.BaseObjects.Count; j++)
                    {
                        if (gameObject.BaseObjects[j].Render)
                        {
                            if (gameObject.BaseObjects[j].BaseFrame.VerticeType != Display.VerticeType)
                            {
                                if (!multipleBaseObjects)
                                    recursiveCallList.Add(objects[i]);
                                continue;
                            }

                            obj = gameObject.BaseObjects[j];
                            texId = obj._currentAnimation.CurrentFrame.Textures.TextureIds[0];

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

                                        shader.SetInt(MATERIAL_SHADER_STRINGS[texIndex * 3], texIndex);
                                        shader.SetInt(MATERIAL_SHADER_STRINGS[texIndex * 3 + 1], texIndex);
                                        shader.SetFloat(MATERIAL_SHADER_STRINGS[texIndex * 3 + 2], 16);

                                        currentTextureUnit++;
                                    }
                                }
                            }

                            if (count == ObjectBufferCount)
                            {
                                recursiveCallList.Add(objects[i]);
                            }
                            else
                            {
                                InsertDataIntoInstancedRenderArray(obj, gameObject.MultiTextureData, ref _instancedRenderDataArray, ref currIndex, (usedTextures[texId] - TextureUnit.Texture0));

                                count++;
                            }
                        }
                    }
                }
            }

            //draw(count, ref _instancedRenderDataArray);

            GL.BufferData(BufferTarget.ArrayBuffer, count * instanceDataOffset * FLOAT_SIZE, _instancedRenderDataArray, BufferUsageHint.DynamicDraw);
            GL.DrawElementsInstanced(PrimitiveType.Triangles, Display.VerticesDrawOrder.Length, DrawElementsType.UnsignedInt, IntPtr.Zero, count);
            DrawCount++;
            ObjectsDrawn += count;


            DisableInstancedShaderAttributes();


            if (recursiveCallList.Count > 0)
            {
                RenderObjectsInstancedGeneric(recursiveCallList, ref _instancedRenderDataArray, enableLighting: enableLighting, deferredShading: deferredShading);
            }

            usedTextures.Clear();
            _usedTexturesPool.FreeObject(ref usedTextures);

            textureReferences.Clear();
            _textureReferencesPool.FreeObject(ref textureReferences);

            recursiveCallList.Clear();
            _objectListPool.FreeObject(ref recursiveCallList);
        }


        public static void RenderInstancedRenderData<T>(List<T> data) where T : InstancedRenderData
        {
            for (int i = 0; i < data.Count; i++)
            {
                if (!data[i].IsValid)
                    continue;

                data[i].Shader.Use();
                //data[i].Shader.SetFloat("enableLighting", data[i].EnableLighting ? 1 : 0);

                foreach (var Tex in data[i].Textures)
                {
                    Tex.Key.Use(Tex.Value);

                    int texIndex = (int)Tex.Value - 33984;
                    //int materialIndex = texIndex > 0 ? texIndex - 1 : 0;
                    data[i].Shader.SetInt(MATERIAL_SHADER_STRINGS[texIndex * 3], texIndex);
                    data[i].Shader.SetInt(MATERIAL_SHADER_STRINGS[texIndex * 3 + 1], 16);
                    data[i].Shader.SetFloat(MATERIAL_SHADER_STRINGS[texIndex * 3 + 2], 16);
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


        #region Framebuffer rendering
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
            GL.BufferData(BufferTarget.ArrayBuffer, FLOAT_SIZE * g_quad_vertex_buffer_data.Length, g_quad_vertex_buffer_data, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * FLOAT_SIZE, 0);
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

        public static void RenderGBuffer(GBuffer frameBuffer)
        {
            frameBuffer.BindForReading();
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);

            Shaders.DEFERRED_LIGHTING_SHADER.Use();

            //Bind the texture that the objects were rendered on to
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, frameBuffer.ColorTextureHandle);

            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, frameBuffer.PositionTextureHandle);

            GL.ActiveTexture(TextureUnit.Texture2);
            GL.BindTexture(TextureTarget.Texture2D, frameBuffer.NormalTextureHandle);

            Shaders.DEFERRED_LIGHTING_SHADER.SetInt("gColor", 0);
            Shaders.DEFERRED_LIGHTING_SHADER.SetInt("gPosition", 1);
            Shaders.DEFERRED_LIGHTING_SHADER.SetInt("gNormal", 2);

            //empty out the depth buffer for when we reuse this frame buffer
            //frameBuffer.ClearBuffers();

            //bind the current frame buffer to either the destination buffer if passed or the default buffer if not

            #region Render quad with the frame buffer texture
            GL.EnableVertexAttribArray(0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, FLOAT_SIZE * g_quad_vertex_buffer_data.Length, g_quad_vertex_buffer_data, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * FLOAT_SIZE, 0);
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

            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);
            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, 0);
        }

        public static void DrawToFrameBuffer(FrameBufferObject frameBuffer) 
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, frameBuffer.FrameBuffer);

            GL.DrawBuffer(DrawBufferMode.ColorAttachment0);

            GL.BindTexture(TextureTarget.Texture2D, 0);
        }
        #endregion
        public static void RenderParticlesInstanced(ParticleGenerator generator)
        {
            if (generator.Particles.Count == 0)
                return;

            Shaders.PARTICLE_SHADER.Use();

            RenderableObject Display = generator.ParticleDisplay;

            PrepareParticleRenderFunc(Display);

            Display.Material.Diffuse.Use(TextureUnit.Texture0);

            Shaders.PARTICLE_SHADER.SetInt("tex", 0);

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

            GL.BufferData(BufferTarget.ArrayBuffer, count * particleDataOffset * FLOAT_SIZE, _instancedRenderArray, BufferUsageHint.DynamicDraw);
            GL.DrawElementsInstanced(PrimitiveType.Triangles, Display.VerticesDrawOrder.Length, DrawElementsType.UnsignedInt, new IntPtr(), count);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);


            DisableParticleShaderAttributes();

            DrawCount++;
            ObjectsDrawn += count;
        }

        #region Skybox
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

            GL.EnableVertexAttribArray(0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, skyboxVertices.Length * FLOAT_SIZE, skyboxVertices, BufferUsageHint.DynamicDraw);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * FLOAT_SIZE, 0);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture3D, map.Handle);
            Shaders.SKYBOX_SHADER.SetInt("texture0", 0);

            GL.DrawArrays(PrimitiveType.Triangles, 0, 36);

            GL.DisableVertexAttribArray(0);
        }
        #endregion

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

                                    _textures.TryAdd(newTexture.TextureId, newTexture);
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

                                _textures.TryAdd(newTexture.TextureId, newTexture);
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

                    _textures.TryAdd(newTexture.TextureId, newTexture);
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

                    _textures.TryAdd(newTexture.TextureId, newTexture);
                    _loadedTextures.Add(generator.ParticleDisplay.Textures.TextureIds[p], newTexture.Handle);

                    generator.ParticleDisplay.Material.Diffuse = newTexture;
                }
                else
                {
                    generator.ParticleDisplay.Material.Diffuse = new Texture(handle, generator.ParticleDisplay.Textures.TextureIds[p]);
                }
            }
        }
        public static void LoadTextureFromUIObject<T>(T UIObj, bool shouldLock = true) where T : UIObject 
        {
            object lockObj = new object();

            if(shouldLock && UIObj.ManagerHandle != null)
            {
                lockObj = UIObj.ManagerHandle._UILock;
            }

            //lock (lockObj)
            //{
                for (int i = 0; i < UIObj.BaseObjects.Count; i++)
                {
                    if (UIObj._canLoadTexture)
                    {
                        LoadTextureFromBaseObject(UIObj.BaseObjects[i], true);
                    }
                }

                for (int i = 0; i < UIObj.Children.Count; i++)
                {
                    LoadTextureFromUIObject(UIObj.Children[i]);
                }
            //}

            if (UIObj._canLoadTexture)
            {
                UIObj.TextureLoaded = true;
            }
        }

        public static void LoadTextureFromSimple(SimpleTexture simp)
        {
            if (!simp.TextureLoaded)
            {
                if (!_loadedTextures.TryGetValue(simp.TextureId, out int handle))
                {
                    Texture newTexture = Texture.LoadFromFile(simp.FileName, nearest: simp.Nearest, generateMipMaps: simp.GenerateMipMaps, 
                        wrapType: simp.WrapType);
                    newTexture.TextureId = simp.TextureId;

                    _textures.TryAdd(newTexture.TextureId, newTexture);
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

        public static Texture LoadTextureFromSpritesheet(Spritesheet spritesheet)
        {
            if(_textures.TryGetValue(spritesheet.TextureId, out var tex))
                return tex;

            Texture newTex = Texture.LoadFromFile(spritesheet.File);
            _textures.TryAdd(tex.TextureId, newTex);

            return newTex;
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
            GL.DisableVertexAttribArray(0);
            GL.DisableVertexAttribArray(1);
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
            GL.EnableVertexAttribArray(9);
            GL.VertexAttribDivisor(1, 0);
            GL.VertexAttribDivisor(2, 1);
            GL.VertexAttribDivisor(3, 1);
            GL.VertexAttribDivisor(4, 1);
            GL.VertexAttribDivisor(5, 1);
            GL.VertexAttribDivisor(6, 1);
            GL.VertexAttribDivisor(7, 1);
            GL.VertexAttribDivisor(8, 1);
            GL.VertexAttribDivisor(9, 1);
        }
        public static void DisableParticleShaderAttributes()
        {
            GL.DisableVertexAttribArray(0);
            GL.DisableVertexAttribArray(1);
            GL.DisableVertexAttribArray(2);
            GL.DisableVertexAttribArray(3);
            GL.DisableVertexAttribArray(4);
            GL.DisableVertexAttribArray(5);
            GL.DisableVertexAttribArray(6);
            GL.DisableVertexAttribArray(7);
            GL.DisableVertexAttribArray(8);
            GL.DisableVertexAttribArray(9);
            GL.VertexAttribDivisor(2, 0);
            GL.VertexAttribDivisor(3, 0);
            GL.VertexAttribDivisor(4, 0);
            GL.VertexAttribDivisor(5, 0);
            GL.VertexAttribDivisor(6, 0);
            GL.VertexAttribDivisor(7, 0);
            GL.VertexAttribDivisor(8, 0);
            GL.VertexAttribDivisor(9, 0);
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
