using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Engine_Classes
{
    public abstract class TransformationBase
    {
        public Matrix4 Transformations = Matrix4.Identity;
    }
}
