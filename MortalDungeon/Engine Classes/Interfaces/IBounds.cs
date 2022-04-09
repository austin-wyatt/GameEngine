using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace MortalDungeon.Engine_Classes
{
    internal interface IBounds
    {
        public bool Contains(Vector2 point, Camera camera = null);

        public bool Contains3D(Vector3 pointNear, Vector3 pointFar, Camera camera);

        public Vector3 GetDimensionData();

        public PointF GetTransformedPoint(float x, float y, float z, Camera camera = null);
    }
}
