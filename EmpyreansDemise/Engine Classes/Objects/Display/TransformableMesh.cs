using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Engine_Classes
{
    /// <summary>
    /// Contains all vertex (any combination of position, texture, and normal that is necessary) 
    /// data and transformation data. <para/>
    /// 
    /// Setting vertex, draw order, and stride information is the responsibility of the implementing class. <para/>
    /// 
    /// This class is intended to be a barebones (and more updated) version of RenderableObject which should
    /// hopefully provide some more flexibility for non-standard objects (such as code generated meshes and whatnot)
    /// </summary>
    public class TransformableMesh : Transformations3D
    {
        public float[] Vertices;

        /// <summary>
        /// The order in which the vertices should be drawn to create triangles.
        /// </summary>
        public uint[] VertexDrawOrder;

        /// <summary>
        /// The size in bytes per vertex. If the vertex data includes position, texture, and normal
        /// then the stride would be (3 + 2 + 3) * sizeof(float) = 32.
        /// </summary>
        public int Stride;

        public TransformableMesh() { }

        public TransformableMesh(float[] vertices, uint[] vertexDrawOrder)
        {
            Vertices = vertices;
            VertexDrawOrder = vertexDrawOrder;
        }
    }
}
