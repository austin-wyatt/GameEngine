using Empyrean.Objects;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Engine_Classes.Rendering
{
    public enum RenderBatchType
    {
        Default,
        Scissor,
        Text
    }
    public class RenderBatch
    {
        const int OBJECT_POOL_SIZE = 20;
        public const int BATCH_SIZE = 2000;
        public RenderBatchType RenderBatchType = RenderBatchType.Default;

        public List<object> Items = new List<object>(BATCH_SIZE);

        /// <summary>
        /// Dependent batches would change their handling based on the RenderBatchType <para/>
        /// For the scissor batch type, the dependent batches list would hold a batch whose
        /// scissor area would need to be intersected with the current batches scissor data
        /// </summary>
        public List<RenderBatch> DependentBatches = new List<RenderBatch>();
        public ScissorData ScissorData;

        //Render batches should be created in place of the List<GameObject> handling that is being done in render queue
        //What constitutes a render batch would be determined per type that is being batched (for example, UI can be scissored)

        //Create a variation of ObjectPool that allows for buffers to be sorted and retrieved by size. When a request for a buffer is made,
        //return the smallest buffer that fits the requested data. 
        //These buffers can be expanded by the consumer and they should be resorted into position when returned

        //How about a pool of RenderBatches exists but with a maximum amount of objects per batch.
        //Render batches would only be used for cases where a special operation needs to be done like UI

        public void CopyParameters(RenderBatch batch)
        {
            RenderBatchType = batch.RenderBatchType;
            for(int i = 0; i < batch.DependentBatches.Count; i++)
            {
                DependentBatches.Add(batch.DependentBatches[i]);
            }
            ScissorData = batch.ScissorData;
        }

        public static RenderBatch Get()
        {
            return Pool.GetObject();
        }

        public void Free()
        {
            Items.Clear();
            ScissorData = null;
            RenderBatchType = RenderBatchType.Default;
            DependentBatches.Clear();
            Pool.FreeObject(this);
        }

        public void DrawScissorQuad()
        {
            GL.Disable(EnableCap.DepthTest);

            Shaders.UI_SCISSOR_SHADER.Use();
            Shaders.UI_SCISSOR_SHADER.SetMatrix4("transform", ref ScissorData.ScissoredArea.Transformations);

            float[] vertices = ScissorData.ScissoredArea.Vertices;
            int verticesSizeBytes = vertices.Length * sizeof(float);

            GL.BindBuffer(BufferTarget.ArrayBuffer, Renderer._generalArrayBuffer);

            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);

            GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, verticesSizeBytes, vertices);

            GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

            GL.Enable(EnableCap.DepthTest);
        }

        private static ObjectPool<RenderBatch> Pool = new ObjectPool<RenderBatch>(OBJECT_POOL_SIZE);
    }
}
