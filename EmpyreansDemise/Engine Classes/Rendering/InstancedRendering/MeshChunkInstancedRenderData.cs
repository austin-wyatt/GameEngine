using Empyrean.Game.Map.BlendControls;
using Empyrean.Game.Tiles;
using Empyrean.Game.Tiles.Meshes;
using Empyrean.Objects;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Engine_Classes.Rendering
{
    public enum MeshChunkDrawType
    {
        Visible,
        Fog,
    }

    public class MeshChunkInstancedRenderData : InstancedRenderData
    {
        public int FogElementBuffer;

        public int FogDrawOrderLen = 0;
        public int VisibleDrawOrderLen = 0;

        TileChunk ChunkHandle;

        const int FLOAT_SIZE = 4;

        public MeshChunkInstancedRenderData() : base()
        {
            FogElementBuffer = GL.GenBuffer();

            Shader = Shaders.CHUNK_SHADER;
        }

        private static Dictionary<int, int> ShaderTextures = new Dictionary<int, int>();

        private const string ORIGIN_UNIFORM_NAME = "BlendMapOrigin";
        private const string BLEND_MAP_UNIFORM_NAME = "BlendMap";
        public void Draw(MeshChunkDrawType drawType = MeshChunkDrawType.Visible)
        {
            if (!IsValid)
                return;

            ChunkHandle.BlendMap.Texture.Use(TextureUnit.Texture15);

            Texture tex;
            int handle;

            for (int i = 0; i < ChunkHandle.BlendMap.Palette.Length; i++)
            {
                if(ChunkHandle.BlendMap.Palette[i] != TileType.None)
                {
                    tex = BlendTextureManager.GetTileTexture(ChunkHandle.BlendMap.Palette[i]);

                    if (!(ShaderTextures.TryGetValue(i, out handle) && tex.Handle == handle))
                    {
                        tex.Use(TextureUnit.Texture0 + i);
                        Shader.SetInt(Renderer.MATERIAL_SHADER_STRINGS[i * 3], i);

                        ShaderTextures.AddOrSet(i, tex.Handle);
                    }
                }
            }

            #region background texture
            const int BACKGROUND_INDEX = 3;
            tex = BlendTextureManager.GetTileTexture(ChunkHandle.BlendMap.Background);

            if (!(ShaderTextures.TryGetValue(BACKGROUND_INDEX, out handle) && tex.Handle == handle))
            {
                tex.Use(TextureUnit.Texture0 + BACKGROUND_INDEX);
                Shader.SetInt(Renderer.MATERIAL_SHADER_STRINGS[BACKGROUND_INDEX * 3], BACKGROUND_INDEX);

                ShaderTextures.AddOrSet(BACKGROUND_INDEX, tex.Handle);
            }
            #endregion

            Shader.SetVector3(ORIGIN_UNIFORM_NAME, ref ChunkHandle.MeshChunk.Origin);
            Shader.SetInt(BLEND_MAP_UNIFORM_NAME, 15);

            //EnableInstancedShaderAttributes();
            PrepareInstancedRenderFunc();

            VerticesCount = 0;

            //GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBuffer);

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


            GL.DrawElementsInstanced(PrimitiveType.Triangles, VerticesCount, DrawElementsType.UnsignedInt, new IntPtr(), 1);

            Renderer.DrawCount++;
            Renderer.ObjectsDrawn += 1;

            //DisableInstancedShaderAttributes();
        }

        public override void CleanUp()
        {
            GL.DeleteBuffers(1, new int[]{ FogElementBuffer });

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

            ChunkHandle = chunk;
            IsValid = true;
        }

        /// <summary>
        /// Fills the fog and vision element array buffers using the data in a tile chunk's mesh chunk.
        /// </summary>
        /// <param name="chunk"></param>
        public void FillVisionBuffers(TileChunk chunk)
        {
            //visible tiles
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBuffer);
            GL.BufferData(BufferTarget.ElementArrayBuffer, chunk.MeshChunk.VisionDrawOrderLength * sizeof(uint), chunk.MeshChunk.VisionDrawOrder, BufferUsageHint.DynamicDraw);
            VisibleDrawOrderLen = chunk.MeshChunk.VisionDrawOrderLength;

            //fog tiles
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, FogElementBuffer);
            GL.BufferData(BufferTarget.ElementArrayBuffer, chunk.MeshChunk.FogDrawOrderLength * sizeof(uint), chunk.MeshChunk.FogDrawOrder, BufferUsageHint.DynamicDraw);
            FogDrawOrderLen = chunk.MeshChunk.FogDrawOrderLength;
        }

        /// <summary>
        /// Place vertex data into the vertex buffer.
        /// </summary>
        public void FillVertexBuffers(TileChunk chunk)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, chunk.MeshChunk.Mesh.Vertices.Length * FLOAT_SIZE, chunk.MeshChunk.Mesh.Vertices, BufferUsageHint.DynamicDraw); //take the raw vertices
        }

        /// <summary>
        /// Place chunk transformation data into the instanced data buffer
        /// </summary>
        public void FillTransformationData(TileChunk chunk)
        {
            int index = 0;
            InsertMatrixDataIntoArray(ref Renderer._instancedRenderArray, ref chunk.MeshChunk.Mesh.Transformations, ref index);

            GL.BindBuffer(BufferTarget.ArrayBuffer, InstancedDataBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, 16 * FLOAT_SIZE, Renderer._instancedRenderArray, BufferUsageHint.DynamicDraw);
        }

        const int _vertexDataLength = MeshTile.VERTEX_OFFSET * FLOAT_SIZE;
        const int _dataLength = 16 * FLOAT_SIZE;
        public override void PrepareInstancedRenderFunc()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBuffer);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, _vertexDataLength, 0); //vertex data
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, _vertexDataLength, 3 * FLOAT_SIZE); //Texture coordinate data
            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, _vertexDataLength, 5 * FLOAT_SIZE); //Normal coordinate data
            GL.VertexAttribPointer(3, 2, VertexAttribPointerType.Float, false, _vertexDataLength, 8 * FLOAT_SIZE); //Contains the spritesheet position and the texture uniform index
            GL.VertexAttribPointer(4, 4, VertexAttribPointerType.Float, false, _vertexDataLength, 10 * FLOAT_SIZE); //Contains the color data for the vertex
            GL.VertexAttribPointer(5, 1, VertexAttribPointerType.Float, false, _vertexDataLength, 14 * FLOAT_SIZE); //the blend percentage for the passed colo

            GL.BindBuffer(BufferTarget.ArrayBuffer, InstancedDataBuffer);
            
            GL.VertexAttribPointer(6, 4, VertexAttribPointerType.Float, false, _dataLength, 0);                  //| Transformation matrix data
            GL.VertexAttribPointer(7, 4, VertexAttribPointerType.Float, false, _dataLength, 4 * FLOAT_SIZE);  //|
            GL.VertexAttribPointer(8, 4, VertexAttribPointerType.Float, false, _dataLength, 8 * FLOAT_SIZE);  //|
            GL.VertexAttribPointer(9, 4, VertexAttribPointerType.Float, false, _dataLength, 12 * FLOAT_SIZE); //|

        }

        public override void EnableInstancedShaderAttributes()
        {
            for (int i = 0; i <= 9; i++)
            {
                GL.EnableVertexAttribArray(i);
            }
            for (int i = 6; i <= 9; i++)
            {
                GL.VertexAttribDivisor(i, 25 * TileChunk.DefaultChunkWidth * TileChunk.DefaultChunkHeight);
            }
        }
        public override void DisableInstancedShaderAttributes()
        {
            for (int i = 2; i <= 9; i++)
            {
                GL.DisableVertexAttribArray(i);
            }

            for (int i = 3; i <= 9; i++)
            {
                GL.VertexAttribDivisor(i, 0);
            }

            ShaderTextures.Clear();
        }
    }
}
