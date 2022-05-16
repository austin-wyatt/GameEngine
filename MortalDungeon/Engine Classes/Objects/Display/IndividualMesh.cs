using Empyrean.Game.Tiles;
using Empyrean.Game.Tiles.Meshes;
using Empyrean.Objects;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Empyrean.Engine_Classes
{
    public class IndividualMesh : TransformableMesh
    {
        public Shader Shader = Shaders.INDIVIDUAL_MESH_SHADER;

        public SimpleTexture Texture;

        public Transformations2D TextureTransformations = new Transformations2D();

        public Vector4 Color = _Colors.White;

        private bool _initialized = false;
        private bool _disposed = false;

        public bool Render = true;

        public IndividualMesh()
        {
            InitializeBuffers();
        }

        ~IndividualMesh()
        {
            Dispose();
        }

        public void LoadTexture() 
        {
            if(Thread.CurrentThread.ManagedThreadId == WindowConstants.MainThreadId)
            {
                Texture.LoadTextureImmediate();
            }
            else
            {
                Texture.LoadTexture();
            }
        }

        private void InitializeBuffers()
        {
            if (_disposed) 
                return;

            if (Thread.CurrentThread.ManagedThreadId == WindowConstants.MainThreadId)
            {
                VertexBuffer = GL.GenBuffer();
                ElementBuffer = GL.GenBuffer();

                _initialized = true;
            }
            else
            {
                Window.QueueToRenderCycle(() =>
                {
                    if (_disposed) 
                        return;

                    VertexBuffer = GL.GenBuffer();
                    ElementBuffer = GL.GenBuffer();

                    _initialized = true;
                });
            }
        }
        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            if (_initialized)
            {
                if (Thread.CurrentThread.ManagedThreadId == WindowConstants.MainThreadId)
                {
                    GL.DeleteBuffers(2, new int[] { VertexBuffer, ElementBuffer });
                }
                else
                {
                    Window.QueueToRenderCycle(() =>
                    {
                        GL.DeleteBuffers(2, new int[] { VertexBuffer, ElementBuffer });
                    });
                }
            }
        }

        private int VertexBuffer;
        private int ElementBuffer;

        private const int _vertexDataLength = 8;
        private const int FLOAT_SIZE = 4;
        public void StageBuffers()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBuffer);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, _vertexDataLength * FLOAT_SIZE, 0); //vertex
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, _vertexDataLength * FLOAT_SIZE, 3 * FLOAT_SIZE); //texture coordinates
            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, _vertexDataLength * FLOAT_SIZE, 5 * FLOAT_SIZE); //normal

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBuffer);
        }

        public void FillVertexBuffer()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, Vertices.Length * sizeof(float), Vertices, BufferUsageHint.DynamicDraw);
        }

        public void FillElementArrayBuffer()
        {
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBuffer);
            GL.BufferData(BufferTarget.ElementArrayBuffer, VertexDrawOrder.Length * sizeof(uint), VertexDrawOrder, BufferUsageHint.DynamicDraw);
        }

        private const string _transformationUniform = "Transform";
        private const string _texTransformUniform = "TexTransform";
        private const string _textureUniform = "Texture";
        private const string _colorUniform = "Color";
        public void Draw()
        {
            if (!Texture.TextureLoaded || !Render)
                return;

            Shader.Use();
            StageBuffers();
            Texture.Texture.Use(TextureUnit.Texture0);

            Shader.SetInt(_textureUniform, 0);
            Shader.SetMatrix4(_transformationUniform, ref Transformations);
            Shader.SetMatrix3(_texTransformUniform, ref TextureTransformations.Transformations);
            Shader.SetVector4(_colorUniform, ref Color);

            GL.DrawElements(PrimitiveType.Triangles, VertexDrawOrder.Length, DrawElementsType.UnsignedInt, 0);
        }

        public void FillFromMeshTile(MeshTile meshTile)
        {
            int offset = meshTile.GetVertexOffset();

            if(Vertices == null || Vertices.Length != _vertexDataLength * MeshTile.VERTEX_COUNT)
            {
                Vertices = new float[_vertexDataLength * MeshTile.VERTEX_COUNT];
            }

            VertexDrawOrder = new uint[MeshTile.FACES.Length];
            Array.Copy(MeshTile.FACES, VertexDrawOrder, MeshTile.FACES.Length);

            int vertIndex = 0;
            for (int i = 0; i < _vertexDataLength * MeshTile.VERTEX_COUNT; i += _vertexDataLength)
            {
                Vertices[i] = MeshTile.VERTICES[vertIndex * 3] - 0.5f;
                Vertices[i + 1] = MeshTile.VERTICES[vertIndex * 3 + 1] - 0.5f;
                Vertices[i + 2] = MeshTile.VERTICES[vertIndex * 3 + 2] + meshTile.Weights[vertIndex] - meshTile.Weights[^1];
                Vertices[i + 3] = meshTile.VerticesHandle[offset + 3];
                Vertices[i + 4] = meshTile.VerticesHandle[offset + 4];
                Vertices[i + 5] = meshTile.VerticesHandle[offset + 5];
                Vertices[i + 6] = meshTile.VerticesHandle[offset + 6];
                Vertices[i + 7] = meshTile.VerticesHandle[offset + 7];

                offset += MeshTile.VERTEX_OFFSET;
                vertIndex++;
            }

            Window.QueueToRenderCycle(FillVertexBuffer);
            Window.QueueToRenderCycle(FillElementArrayBuffer);
        }

        public void FillFromTiles(List<Tile> tiles, bool quadTexCoords = true)
        {
            if (tiles.Count == 0) 
                return;

            Vector2i firstChunkPos = tiles[0].Chunk.GetGlobalChunkPoint();

            if (Vertices == null || Vertices.Length != (_vertexDataLength * MeshTile.VERTEX_COUNT * tiles.Count)) 
            {
                Vertices = new float[_vertexDataLength * MeshTile.VERTEX_COUNT * tiles.Count];
            }

            if(VertexDrawOrder == null || VertexDrawOrder.Length != (MeshTile.FACES.Length * tiles.Count))
            {
                VertexDrawOrder = new uint[MeshTile.FACES.Length * tiles.Count];
            }

            Vector2i currChunkOffset;


            Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 max = new Vector2(float.MinValue, float.MinValue);

            int offset = 0;
            for (int i = 0; i < tiles.Count; i++)
            {
                offset = tiles[i].MeshTileHandle.GetVertexOffset();

                for (int f = 0; f < MeshTile.FACES.Length; f++)
                {
                    VertexDrawOrder[i * MeshTile.FACES.Length + f] = MeshTile.FACES[f] + (uint)(i * MeshTile.VERTEX_COUNT);
                }

                currChunkOffset = tiles[i].Chunk.GetGlobalChunkPoint();
                currChunkOffset.X = firstChunkPos.X - currChunkOffset.X;
                currChunkOffset.Y -= firstChunkPos.Y;

                int tileOffset = i * _vertexDataLength * MeshTile.VERTEX_COUNT;

                for (int j = 0; j < _vertexDataLength * MeshTile.VERTEX_COUNT; j += _vertexDataLength)
                {
                    Vertices[j + tileOffset] = tiles[i].MeshTileHandle.VerticesHandle[offset] - currChunkOffset.X * (MeshTile.CHUNK_WIDTH - 0.25f);
                    Vertices[j + tileOffset + 1] = tiles[i].MeshTileHandle.VerticesHandle[offset + 1] - currChunkOffset.Y * (MeshTile.CHUNK_HEIGHT - MeshTile.TILE_HEIGHT * 0.5f);
                    //Vertices[j + tileOffset] = tiles[i].VerticesHandle[offset];
                    //Vertices[j + tileOffset + 1] = tiles[i].VerticesHandle[offset + 1];
                    Vertices[j + tileOffset + 2] = tiles[i].MeshTileHandle.VerticesHandle[offset + 2];


                    min.X = Vertices[j + tileOffset] < min.X ? Vertices[j + tileOffset] : min.X;
                    max.X = Vertices[j + tileOffset] > max.X ? Vertices[j + tileOffset] : max.X;
                    min.Y = Vertices[j + tileOffset + 1] < min.Y ? Vertices[j + tileOffset + 1] : min.Y;
                    max.Y = Vertices[j + tileOffset + 1] > max.Y ? Vertices[j + tileOffset + 1] : max.Y;

                    Vertices[j + tileOffset + 3] = tiles[i].MeshTileHandle.VerticesHandle[offset + 3];
                    Vertices[j + tileOffset + 4] = tiles[i].MeshTileHandle.VerticesHandle[offset + 4];
                    
                    Vertices[j + tileOffset + 5] = tiles[i].MeshTileHandle.VerticesHandle[offset + 5];
                    Vertices[j + tileOffset + 6] = tiles[i].MeshTileHandle.VerticesHandle[offset + 6];
                    Vertices[j + tileOffset + 7] = tiles[i].MeshTileHandle.VerticesHandle[offset + 7];

                    offset += MeshTile.VERTEX_OFFSET;
                }
            }

            Vector3 center = new Vector3();

            offset = tiles[0].MeshTileHandle.GetVertexOffset() + (MeshTile.VERTEX_COUNT - 1) * MeshTile.VERTEX_OFFSET;
            center.X = tiles[0].MeshTileHandle.VerticesHandle[offset];
            center.Y = tiles[0].MeshTileHandle.VerticesHandle[offset + 1];
            center.Z = tiles[0].MeshTileHandle.VerticesHandle[offset + 2];


            //this should be the center tile's 25th vertex position and not the average of the min and max vertices
            Vector2 minMaxDiffReciprocal = new Vector2(1 / (max.X - min.X), 1 / (max.Y - min.Y));

            for (int i = 0; i < Vertices.Length; i += _vertexDataLength)
            {
                if (quadTexCoords)
                {
                    //Convert each vertex's texture coordinate to a point between 0 and 1 
                    //according to its relation to the minimum and maximum vertices
                    Vertices[i + 3] = (Vertices[i] - min.X) * minMaxDiffReciprocal.X;
                    Vertices[i + 4] = (Vertices[i + 1] - min.Y) * minMaxDiffReciprocal.Y;
                }

                //pull the center point to 0, 0 so that our object is centered
                Vertices[i] -= center.X;
                Vertices[i + 1] -= center.Y;
                Vertices[i + 2] -= center.Z;
            }

            Window.QueueToRenderCycle(FillVertexBuffer);
            Window.QueueToRenderCycle(FillElementArrayBuffer);
        }
    }
}
