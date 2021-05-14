using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace MortalDungeon.Engine_Classes.MiscOperations
{
    public static class MiscOperations
    {
        public static class GFG
        {
            public static bool get_line_intersection(float p0_x, float p0_y, float p1_x, float p1_y,
                    float p2_x, float p2_y, float p3_x, float p3_y)
            {
                float s1_x, s1_y, s2_x, s2_y;
                s1_x = p1_x - p0_x; s1_y = p1_y - p0_y;
                s2_x = p3_x - p2_x; s2_y = p3_y - p2_y;

                float s, t;
                s = (-s1_y * (p0_x - p2_x) + s1_x * (p0_y - p2_y)) / (-s2_x * s1_y + s1_x * s2_y);
                t = (s2_x * (p0_y - p2_y) - s2_y * (p0_x - p2_x)) / (-s2_x * s1_y + s1_x * s2_y);

                if (s >= 0 && s <= 1 && t >= 0 && t <= 1)
                {
                    // Collision detected
                    
                    return true;
                }

                return false; // No collision
            }
        }
    }
}
