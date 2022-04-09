using MortalDungeon.Game.Tiles;
using MortalDungeon.Game.Tiles.Meshes;
using MortalDungeon.Objects;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Engine_Classes.Rendering
{
    public enum MeshChunkDrawType
    {
        Visible,
        Fog,
    }

    public class MeshChunkInstancedRenderData : InstancedRenderData
    {
        private const int instanceDataOffset = 18;
        private const int instanceDataLength = instanceDataOffset * sizeof(float);

        public int FogElementBuffer;
        public int TextureInfoBuffer;

        public int FogDrawOrderLen = 0;
        public int VisibleDrawOrderLen = 0;

        TileChunk ChunkHandle;

        public MeshChunkInstancedRenderData() : base()
        {
            FogElementBuffer = GL.GenBuffer();
            TextureInfoBuffer = GL.GenBuffer();

            Shader = Shaders.CHUNK_SHADER;
        }

        public void Draw(MeshChunkDrawType drawType = MeshChunkDrawType.Visible)
        {
            if (!IsValid)
                return;

            Shader.Use();
            //data[i].Shader.SetFloat("enableLighting", data[i].EnableLighting ? 1 : 0);

            for(int i = 0; i < ChunkHandle.MeshChunk.UsedTextureHandles.Count; i++)
            {
                Texture.Use(TextureUnit.Texture0 + i, ChunkHandle.MeshChunk.UsedTextureHandles[i]);

                Shader.SetInt($"material[{i}].diffuse", i);
                Shader.SetInt($"material[{i}].specular", 16);
                Shader.SetFloat($"material[{i}].shininess", 16);
            }

            EnableInstancedShaderAttributes();
            PrepareInstancedRenderFunc();

            VerticesCount = 0;

            switch (drawType)
            {
                case MeshChunkDrawType.Visible:
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBuffer);
                    VerticesCount = VisibleDrawOrderLen;
                    break;
                case MeshChunkDrawType.Fog:
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, FogElementBuffer);
                    VerticesCount = FogDrawOrderLen;
                    break;
            }
            

            GL.DrawElements(PrimitiveType.Triangles, VerticesCount, DrawElementsType.UnsignedInt, new IntPtr());

            Renderer.DrawCount++;
            Renderer.ObjectsDrawn += 1;
        }

        public override void CleanUp()
        {
            GL.DeleteBuffers(2, new int[]{ FogElementBuffer, TextureInfoBuffer });

            base.CleanUp();
        }

        /// <summary>
        /// Fill the buffer data from the mesh chunk
        /// </summary>
        public void GenerateInstancedRenderData(TileChunk chunk)
        {
            FillTransformationData(chunk);
            FillVisionBuffers(chunk);
            FillVertexBuffers(chunk);
            FillTextureBuffers(chunk);

            ChunkHandle = chunk;
        }

        /// <summary>
        /// Fills the fog and vision element array buffers using the data in a tile chunk's mesh chunk.
        /// </summary>
        /// <param name="chunk"></param>
        public void FillVisionBuffers(TileChunk chunk)
        {
            //visible tiles
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBuffer);
            GL.BufferData(BufferTarget.ElementArrayBuffer, chunk.MeshChunk.VisionDrawOrder.Length * sizeof(uint), chunk.MeshChunk.VisionDrawOrder, BufferUsageHint.DynamicDraw);
            VisibleDrawOrderLen = chunk.MeshChunk.VisionDrawOrder.Length;

            //fog tiles
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, FogElementBuffer);
            GL.BufferData(BufferTarget.ElementArrayBuffer, chunk.MeshChunk.FogDrawOrder.Length * sizeof(uint), chunk.MeshChunk.FogDrawOrder, BufferUsageHint.DynamicDraw);
            FogDrawOrderLen = chunk.MeshChunk.FogDrawOrder.Length;
        }

        /// <summary>
        /// Place vertex data into the vertex buffer.
        /// </summary>
        private void FillVertexBuffers(TileChunk chunk)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, chunk.MeshChunk.Mesh.Vertices.Length * sizeof(float), chunk.MeshChunk.Mesh.Vertices, BufferUsageHint.DynamicDraw); //take the raw vertices
        }

        /// <summary>
        /// Place tile texture data into the texture info buffer.
        /// </summary>
        public void FillTextureBuffers(TileChunk chunk)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, TextureInfoBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, chunk.MeshChunk.TextureInfo.Length * sizeof(float), chunk.MeshChunk.TextureInfo, BufferUsageHint.DynamicDraw); //take the raw vertices
        }

        /// <summary>
        /// Place chunk transformation data into the instanced data buffer
        /// </summary>
        public void FillTransformationData(TileChunk chunk)
        {
            InsertMatrixDataIntoArray(ref Renderer._instancedRenderArray, ref chunk.MeshChunk.Mesh.Transformations, 0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, InstancedDataBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, 16 * sizeof(float), Renderer._instancedRenderArray, BufferUsageHint.DynamicDraw);
        }

        public override void PrepareInstancedRenderFunc()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBuffer);
            const int vertexDataLength = 8 * sizeof(float);

            //Whenever this data is changed the instanceDataOffset parameter needs to be updated
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, vertexDataLength, 0); //vertex data
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, vertexDataLength, 3 * sizeof(float)); //Texture coordinate data
            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, vertexDataLength, 5 * sizeof(float)); //Normal coordinate data


            GL.BindBuffer(BufferTarget.ArrayBuffer, InstancedDataBuffer);

            const int dataLength = 16 * sizeof(float);
            GL.VertexAttribPointer(3, 4, VertexAttribPointerType.Float, false, dataLength, 0);                  //| Transformation matrix data
            GL.VertexAttribPointer(4, 4, VertexAttribPointerType.Float, false, dataLength, 4 * sizeof(float));  //|
            GL.VertexAttribPointer(5, 4, VertexAttribPointerType.Float, false, dataLength, 8 * sizeof(float));  //|
            GL.VertexAttribPointer(6, 4, VertexAttribPointerType.Float, false, dataLength, 12 * sizeof(float)); //|

            GL.BindBuffer(BufferTarget.ArrayBuffer, TextureInfoBuffer);
            const int textureDataLength = 2 * sizeof(float);

            GL.VertexAttribPointer(7, 2, VertexAttribPointerType.Float, false, textureDataLength, 0); //Contains the spritesheet position and the texture uniform index
        }

        public override void EnableInstancedShaderAttributes()
        {
            for (int i = 0; i < 7; i++)
            {
                GL.EnableVertexAttribArray(i);
            }
            for (int i = 3; i < 6; i++)
            {
                GL.VertexAttribDivisor(i, 25 * TileChunk.DefaultChunkWidth * TileChunk.DefaultChunkHeight);
            }

            GL.VertexAttribDivisor(7, 25);
        }
        public override void DisableInstancedShaderAttributes()
        {
            for (int i = 2; i < 7; i++)
            {
                GL.DisableVertexAttribArray(i);
            }

            for (int i = 3; i < 7; i++)
            {
                GL.VertexAttribDivisor(i, 0);
            }
        }
    }
}
