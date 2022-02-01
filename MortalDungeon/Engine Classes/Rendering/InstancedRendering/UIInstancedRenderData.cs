using MortalDungeon.Objects;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Engine_Classes.Rendering
{
    public class UIInstancedRenderData : InstancedRenderData
    {
        private const int ObjectBufferCount = 7500;
        private const int instanceDataOffset = 28;
        private const int instanceDataLength = instanceDataOffset * sizeof(float);

        public UIInstancedRenderData()
        {
            Shader = Shaders.UI_SHADER;
        }

        public static List<UIInstancedRenderData> GenerateInstancedRenderData(List<UIObject> objects, RenderableObject display = null, bool enableLighting = false)
        {
            List<UIInstancedRenderData> data = new List<UIInstancedRenderData>();


            if (objects.Count > 0)
            {
                GenerateInstancedRenderData(ref data, objects, display, enableLighting);
            }
            else
            {
                data.Add(new UIInstancedRenderData());
            }

            return data;
        }

        public static void GenerateInstancedRenderData(ref List<UIInstancedRenderData> data, List<UIObject> objects, RenderableObject display = null, bool enableLighting = false)
        {
            UIInstancedRenderData instancedRenderData = new UIInstancedRenderData();

            data.Add(instancedRenderData);

            RenderableObject Display;

            if (display == null)
            {
                Display = objects[0].GetBaseObject()._currentAnimation.CurrentFrame;
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
            List<UIObject> recursiveCallList = new List<UIObject>();

            Dictionary<int, TextureUnit> usedTextures = new Dictionary<int, TextureUnit>();
            Dictionary<Texture, TextureUnit> textureReferences = new Dictionary<Texture, TextureUnit>();

            usedTextures.Add(currTexture, TextureUnit.Texture0);
            textureReferences.Add(Display.Material.Diffuse, TextureUnit.Texture0);

            BaseObject obj;
            int texId;

            TextureUnit currentTextureUnit = TextureUnit.Texture1;

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
                                    if(usedTextures.Count > 7)
                                    {
                                        recursiveCallList.Add(objects[i]);
                                        continue;
                                    }

                                    usedTextures.Add(texId, currentTextureUnit);
                                    textureReferences.Add(obj._currentAnimation.CurrentFrame.Material.Diffuse, currentTextureUnit);

                                    currentTextureUnit++;
                                }
                            }

                            if (count == ObjectBufferCount)
                            {
                                recursiveCallList.Add(objects[i]);
                            }
                            else
                            {
                                instancedRenderData.InsertDataIntoInstancedRenderArray(obj, objects[i], ref _instancedDataArray, ref currIndex, (usedTextures[texId] - TextureUnit.Texture0));

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



        public void InsertDataIntoInstancedRenderArray(BaseObject obj, UIObject uiObj, ref float[] _instancedRenderArray, ref int currIndex, int textureTarget)
        {
            var transform = obj.BaseFrame.Transformations;

            InsertMatrixDataIntoArray(ref _instancedRenderArray, ref transform, currIndex);
            currIndex += 16;
            _instancedRenderArray[currIndex++] = obj.BaseFrame.InterpolatedColor.X;
            _instancedRenderArray[currIndex++] = obj.BaseFrame.InterpolatedColor.Y;
            _instancedRenderArray[currIndex++] = obj.BaseFrame.InterpolatedColor.Z;
            _instancedRenderArray[currIndex++] = obj.BaseFrame.InterpolatedColor.W;

            _instancedRenderArray[currIndex++] = obj._currentAnimation.CurrentFrame.SpritesheetPosition;
            _instancedRenderArray[currIndex++] = uiObj.ZPos;
            _instancedRenderArray[currIndex++] = obj.OutlineParameters.InlineThickness;
            _instancedRenderArray[currIndex++] = textureTarget;

            _instancedRenderArray[currIndex++] = obj._currentAnimation.CurrentFrame.SideLengths.X;
            _instancedRenderArray[currIndex++] = obj._currentAnimation.CurrentFrame.SideLengths.Y;
            _instancedRenderArray[currIndex++] = obj._currentAnimation.CurrentFrame.Textures.Spritesheet.Columns;
            _instancedRenderArray[currIndex++] = obj._currentAnimation.CurrentFrame.Textures.Spritesheet.Rows;
        }




        public override void PrepareInstancedRenderFunc()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBuffer);

            //Whenever this data is changed the instanceDataOffset parameter needs to be updated
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Stride, 0); //vertex data
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, Stride, 3 * sizeof(float)); //Texture coordinate data
            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, Stride, 5 * sizeof(float)); //Normal coordinate data

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBuffer);


            GL.BindBuffer(BufferTarget.ArrayBuffer, InstancedDataBuffer);

            GL.VertexAttribPointer(3, 4, VertexAttribPointerType.Float, false, instanceDataLength, 0);                  //| Transformation matrix data
            GL.VertexAttribPointer(4, 4, VertexAttribPointerType.Float, false, instanceDataLength, 4 * sizeof(float));  //|
            GL.VertexAttribPointer(5, 4, VertexAttribPointerType.Float, false, instanceDataLength, 8 * sizeof(float));  //|
            GL.VertexAttribPointer(6, 4, VertexAttribPointerType.Float, false, instanceDataLength, 12 * sizeof(float)); //|

            GL.VertexAttribPointer(7, 4, VertexAttribPointerType.Float, false, instanceDataLength, 16 * sizeof(float)); //Color data
            GL.VertexAttribPointer(8, 4, VertexAttribPointerType.Float, false, instanceDataLength, 20 * sizeof(float)); //spritesheet position (0), enable lighting (1), overlay 0 (2), overlay 0 color (3)
            GL.VertexAttribPointer(9, 4, VertexAttribPointerType.Float, false, instanceDataLength, 24 * sizeof(float)); //side lengths X and Y (1, 2), spritesheet columns and rows (2, 3)

            //set uniforms here
        }

        public override void EnableInstancedShaderAttributes()
        {
            for (int i = 0; i < 10; i++)
            {
                GL.EnableVertexAttribArray(i);
            }
            for (int i = 3; i < 10; i++)
            {
                GL.VertexAttribDivisor(i, 1);
            }
        }
        public override void DisableInstancedShaderAttributes()
        {
            for (int i = 2; i < 10; i++)
            {
                GL.DisableVertexAttribArray(i);
            }

            for (int i = 3; i < 10; i++)
            {
                GL.VertexAttribDivisor(i, 0);
            }
        }
    }
}
