using MortalDungeon.Objects;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Engine_Classes.Rendering
{
    public class InstancedRenderData
    {
        private const int ObjectBufferCount = 7500;
        private const int instanceDataOffset = 40;
        private static float[] _instancedDataArray = new float[ObjectBufferCount * instanceDataOffset];
        private const int instanceDataLength = instanceDataOffset * sizeof(float);

        public int VertexBuffer;
        public int ElementBuffer;
        public int InstancedDataBuffer;

        public bool EnableLighting = false;

        public int VerticesCount;
        public int ItemCount;

        public int Stride;

        public Dictionary<Texture, TextureUnit> Textures = new Dictionary<Texture, TextureUnit>();

        public InstancedRenderData()
        {
            VertexBuffer = GL.GenBuffer();
            ElementBuffer = GL.GenBuffer();
            InstancedDataBuffer = GL.GenBuffer();
        }

        public void CleanUp()
        {
            GL.DeleteBuffers(3, new int[]{ VertexBuffer, ElementBuffer, InstancedDataBuffer });

            Textures = null;
        }


        public static List<InstancedRenderData> GenerateInstancedRenderData<T>(List<T> objects, RenderableObject display = null, bool enableLighting = true) where T : GameObject
        {
            List<InstancedRenderData> data = new List<InstancedRenderData>();




            if(objects.Count > 0)
            {
                GenerateInstancedRenderData(ref data, objects, display, enableLighting);
            }

            return data;
        }

        public static void GenerateInstancedRenderData<T>(ref List<InstancedRenderData> data, List<T> objects, RenderableObject display = null, bool enableLighting = true) where T : GameObject
        {
            InstancedRenderData instancedRenderData = new InstancedRenderData();

            data.Add(instancedRenderData);

            RenderableObject Display;

            if (display == null)
            {
                Display = objects[0].BaseObjects[0]._currentAnimation.CurrentFrame;
            }
            else
            {
                Display = display;
            }

            int currTexture = Display.Textures.TextureIds[0];
            Display.Material.Diffuse.Use(TextureUnit.Texture0);

            FillVertexAndElementBuffers(Display, ref instancedRenderData);

            instancedRenderData.EnableLighting = enableLighting;

            int currIndex = 0;

            int count = 0;
            List<T> recursiveCallList = new List<T>();

            Dictionary<int, TextureUnit> usedTextures = new Dictionary<int, TextureUnit>();
            Dictionary<Texture, TextureUnit> textureReferences = new Dictionary<Texture, TextureUnit>();

            usedTextures.Add(currTexture, TextureUnit.Texture0);
            textureReferences.Add(Display.Material.Diffuse, TextureUnit.Texture0);

            BaseObject obj;
            int texId;

            TextureUnit currentTextureUnit = TextureUnit.Texture2;

            void draw(int itemCount, ref float[] renderDataArray)
            {
                instancedRenderData.Textures = textureReferences;

                GL.BindBuffer(BufferTarget.ArrayBuffer, instancedRenderData.InstancedDataBuffer);
                GL.BufferData(BufferTarget.ArrayBuffer, itemCount * instanceDataOffset * sizeof(float), renderDataArray, BufferUsageHint.DynamicDraw);

                instancedRenderData.ItemCount = itemCount;
                instancedRenderData.VerticesCount = Display.VerticesDrawOrder.Length;
            }

            for (int i = 0; i < objects.Count; i++)
            {
                if (objects[i] != null && objects[i].TextureLoaded && objects[i].Render && !objects[i].Cull)
                {
                    for (int j = 0; j < objects[i].BaseObjects.Count; j++)
                    {
                        if (objects[i].BaseObjects[j].Render)
                        {
                            if (objects[i].BaseObjects[j].BaseFrame.VerticeType != Display.VerticeType)
                            {
                                recursiveCallList.Add(objects[i]);
                                continue;
                            }

                            obj = objects[i].BaseObjects[j];
                            texId = obj._currentAnimation.CurrentFrame.Textures.TextureIds[0];

                            if (texId != currTexture)
                            {
                                if (!usedTextures.ContainsKey(texId))
                                {
                                    usedTextures.Add(texId, currentTextureUnit);
                                    textureReferences.Add(obj._currentAnimation.CurrentFrame.Material.Diffuse, currentTextureUnit);

                                    obj._currentAnimation.CurrentFrame.Material.Diffuse.Use(currentTextureUnit);

                                    int texIndex = (int)currentTextureUnit - 33984;
                                    int materialIndex = texIndex > 0 ? texIndex - 1 : 0;
                                    Shaders.FAST_DEFAULT_SHADER.SetInt($"material[{materialIndex}].diffuse", texIndex);
                                    Shaders.FAST_DEFAULT_SHADER.SetInt($"material[{materialIndex}].specular", texIndex);
                                    //Shaders.FAST_DEFAULT_SHADER.SetInt($"material[{materialIndex}].specular", 15);
                                    Shaders.FAST_DEFAULT_SHADER.SetFloat($"material[{materialIndex}].shininess", 16);

                                    currentTextureUnit++;
                                }
                            }

                            if (count == ObjectBufferCount)
                            {
                                recursiveCallList.Add(objects[i]);
                            }
                            else
                            {
                                InsertDataIntoInstancedRenderArray(obj, objects[i].MultiTextureData, ref _instancedDataArray, ref currIndex, (usedTextures[texId] - TextureUnit.Texture0));

                                count++;
                            }
                        }
                    }
                }
            }

            draw(count, ref _instancedDataArray);


            if (recursiveCallList.Count > 0)
            {
                GenerateInstancedRenderData(ref data, recursiveCallList, null, enableLighting);
            }
        }

        public static void FillVertexAndElementBuffers(RenderableObject Display, ref InstancedRenderData renderData)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, renderData.VertexBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, Display.Vertices.Length * sizeof(float), Display.Vertices, BufferUsageHint.DynamicDraw); //take the raw vertices

            renderData.Stride = Display.Stride;

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, renderData.ElementBuffer);
            GL.BufferData(BufferTarget.ElementArrayBuffer, Display.VerticesDrawOrder.Length * sizeof(uint), Display.VerticesDrawOrder, BufferUsageHint.DynamicDraw);
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

        public static void PrepareInstancedRenderFunc(InstancedRenderData data)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, data.VertexBuffer);

            //Whenever this data is changed the instanceDataOffset parameter needs to be updated
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, data.Stride, 0); //vertex data
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, data.Stride, 3 * sizeof(float)); //Texture coordinate data
            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, data.Stride, 5 * sizeof(float)); //Normal coordinate data

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, data.ElementBuffer);
            

            GL.BindBuffer(BufferTarget.ArrayBuffer, data.InstancedDataBuffer);

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
    }
}
