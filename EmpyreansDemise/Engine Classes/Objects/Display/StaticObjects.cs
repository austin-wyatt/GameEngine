using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Engine_Classes
{
    internal static class StaticObjects
    {
        public static readonly float[] QUAD_VERTICES = new float[]
        {
            -1f, 1f, 0.0f,  //Top left
            -1f, -1f, 0.0f, //Bottom left
            1f, 1f, 0.0f,   //Top Right

            1f, 1f, 0.0f,   //Top Right
            -1f, -1f, 0.0f, //Bottom left
            1f, -1f, 0.0f   //Bottom Right
        };

        public static readonly float[] TEXTURE_COORDS = new float[]
        {
            0f, 1f,   //Top left
            0f, 0f,   //Bottom left
            1f, 1f,   //Top Right

            1f, 1f,   //Top Right
            0f, 0f,   //Bottom left
            1f, 0f    //Bottom Right
        };
    }
}
