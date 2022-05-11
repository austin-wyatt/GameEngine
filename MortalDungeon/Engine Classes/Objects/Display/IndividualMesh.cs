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
    }
}
