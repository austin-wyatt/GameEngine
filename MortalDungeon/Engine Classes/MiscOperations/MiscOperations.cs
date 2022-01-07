using OpenTK.Mathematics;
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
                s1_x = p1_x - p0_x; 
                s1_y = p1_y - p0_y;

                s2_x = p3_x - p2_x; 
                s2_y = p3_y - p2_y;

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


            private static bool OnSegment(float p0_x, float p0_y, float p1_x, float p1_y, float p2_x, float p2_y)
            {
                if (p1_x <= Math.Max(p0_x, p2_x) && p1_x >= Math.Min(p0_x, p2_x) &&
                    p1_y <= Math.Max(p0_y, p2_y) && p1_y >= Math.Min(p0_y, p2_y))
                    return true;

                return false;
            }

            /// <summary>
            /// returns 0 if the points are collinear <para/>
            /// returns 1 if the points are clockwise <para/>
            /// returns 2 if the points are counter clockwise <para/>
            /// </summary>
            private static int Orientation(float p0_x, float p0_y, float p1_x, float p1_y, float p2_x, float p2_y)
            {
                int val = (int)((p1_y - p0_y) * (p2_x - p1_x) -
                        (p1_x - p0_x) * (p2_y - p1_y));

                if (val == 0) 
                    return 0; //collinear

                return (val > 0) ? 1 : 2; //clock or counterclock wise
            }

            public static bool GetLinesIntersect(Vector2 p1, Vector2 q1, Vector2 p2, Vector2 q2)
            {
                // Find the four orientations needed for general and
                // special cases
                int o1 = Orientation(p1.X, p1.Y, q1.X, q1.Y, p2.X, p2.Y);
                int o2 = Orientation(p1.X, p1.Y, q1.X, q1.Y, q2.X, q2.Y);
                int o3 = Orientation(p2.X, p2.Y, q2.X, q2.Y, p1.X, p1.Y);
                int o4 = Orientation(p2.X, p2.Y, q2.X, q2.Y, q1.X, q1.Y);

                // General case
                if (o1 != o2 && o3 != o4)
                    return true;

                // Special Cases
                // p1, q1 and p2 are collinear and p2 lies on segment p1q1
                if (o1 == 0 && OnSegment(p1.X, p1.Y, p2.X, p2.Y, q1.X, q1.Y)) 
                    return true;

                // p1, q1 and q2 are collinear and q2 lies on segment p1q1
                if (o2 == 0 && OnSegment(p1.X, p1.Y, q2.X, q2.Y, q1.X, q1.Y)) 
                    return true;

                // p2, q2 and p1 are collinear and p1 lies on segment p2q2
                if (o3 == 0 && OnSegment(p2.X, p2.Y, p1.X, p1.Y, q2.X, q2.Y)) 
                    return true;

                // p2, q2 and q1 are collinear and q1 lies on segment p2q2
                if (o4 == 0 && OnSegment(p2.X, p2.Y, q1.X, q1.Y, q2.X, q2.Y)) 
                    return true;

                return false; // Doesn't fall in any of the above cases
            }
        }
    }
}
