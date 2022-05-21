using Empyrean.Game.Tiles;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace Empyrean.Engine_Classes
{
    public struct IntRange
    {
        public int Start;
        public int End;

        public IntRange(int start)
        {
            Start = start;
            End = start;
        }

        public IntRange(int start, int end)
        {
            Start = start;
            End = end;
        }

        public int GetValueInRange(ConsistentRandom random)
        {
            return random.Next(Start, End);
        }
    }

    public struct FloatRange
    {
        public float Start;
        public float End;

        public FloatRange(float start)
        {
            Start = start;
            End = start;
        }

        public FloatRange(float start, float end)
        {
            Start = start;
            End = end;
        }

        public float GetValueInRange(ConsistentRandom random)
        {
            float val = (float)random.NextDouble();

            val *= Start - End;
            val += Start;

            return val;
        }
    }

    /// <summary>
    /// Game misc math library
    /// </summary>
    public static class GMath
    {
        private static Vector3 RADIAN_0_VEC = new Vector3(1, 0, 0);

        public static float AngleOfPoints(Vector3 source, Vector3 destination)
        {
            Vector3 lineToDest = source - destination;
            lineToDest.Normalize();

            //source.Normalize();
            //destination.Normalize();

            Vector3.Dot(in lineToDest, in RADIAN_0_VEC, out float dot);
            //float direction = Vector3.Dot(Vector3.Cross(RADIAN_0_VEC, lineToDest), new Vector3(0, 0, 1));
            float det = lineToDest.X * RADIAN_0_VEC.Y - lineToDest.Y * RADIAN_0_VEC.X;

            //Vector3.Dot(in source, in destination, out float dot);
            ////float direction = Vector3.Dot(Vector3.Cross(RADIAN_0_VEC, lineToDest), new Vector3(0, 0, 1));
            //float det = source.X * destination.Y - source.Y * destination.X;

            return (float)MathHelper.Atan2(det, dot);
        }

        /// <summary>
        /// Checks if the testAngle is inside of angle1 and angle2 <para/>
        /// Checks counter clockwise
        /// </summary>
        /// <param name="testAngle"></param>
        /// <param name="angle1"></param>
        /// <param name="angle2"></param>
        /// <returns></returns>
        public static bool IsAngleBetween(float testAngle, float angle1, float angle2)
        {
            testAngle = NormalizeAngle(testAngle); //normalize angles to be 1-360 degrees
            angle1 = NormalizeAngle(angle1);
            angle2 = NormalizeAngle(angle2);

            if(angle2 > angle1)
            {
                return InsideBounds(testAngle, 0, angle1) || InsideBounds(testAngle, angle2, MathHelper.TwoPi);
            }

            return InsideBounds(testAngle, angle2, angle1);

            //if (angle1 < angle2)
            //    return angle1 <= testAngle && testAngle <= angle2;
            //return angle1 <= testAngle || testAngle <= angle2;
        }

        public static float NormalizeAngle(float angle)
        {
            if (angle < 0)
                angle += MathHelper.TwoPi;

            return angle;
        }

        /// <summary>
        /// Performs a modulo operation on a negative value as if it were an offset from the modTarget. <para/>
        /// Ex. NegMod(-1, 20) is equivalent to 18 % 20
        /// </summary>
        public static int NegMod(int A, int modTarget)
        {
            if (A < 0)
            {
                int modVal = Math.Abs(A) % modTarget;

                return modVal == 0 ? 0 : modTarget - Math.Abs(A) % modTarget;
            }

            return A % modTarget;
        }

        public static float NegMod(float A, float modTarget)
        {
            if (A < 0)
            {
                float modVal = Math.Abs(A) % modTarget;

                return modVal == 0 ? 0 : modTarget - Math.Abs(A) % modTarget;
            }

            return A % modTarget;
        }

        public static int AngleOfDirection(Direction dir)
        {
            return ((int)dir + 2) * 60; //default direction is north
        }


        public static bool InsideBounds(int value, int min, int max)
        {
            return value >= min && value <= max;
        }

        public static bool InsideBounds(float value, float min, float max)
        {
            return value >= min && value <= max;
        }

        public static float SmoothLerp(float a, float b, float t)
        {
            t = MathF.Pow(t, 3) * (t * (6 * t - 15) + 10);
            return a + t * (b - a);
        }

        public static float LnLerp(float a, float b, float t)
        {
            t = (float)(MathHelper.Log(t + 1) * 1.442f);

            return a + t * (b - a);
        }

        /// <summary>
        /// Quickly reaches the end at t = 0.5 then quickly returns to the beginning at t = 1
        /// </summary>
        public static float SpikeLerp(float a, float b, float t)
        {
            if(t <= 0.5f)
            {
                return SmoothLerp(a, b, t * 2);
            }
            else
            {
                return SmoothLerp(a, b, (1 - t) * 2);
            }
        }

        /// <summary>
        /// Dips below the X axis then smoothly reaches the end
        /// </summary>
        public static float EaseInBackLerp(float a, float b, float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1;

            t = c3 * t * t * t - c1 * t * t;

            return a + t * (b - a);
        }

        /// <summary>
        /// Dips below the X axis then dips above X = 1 then returns to the end
        /// </summary>
        public static float EaseInOutBackLerp(float a, float b, float t)
        {
            const float c1 = 1.70158f;
            const float c2 = c1 * 1.525f;

            t = (float)(t < 0.5f
              ? (Math.Pow(2 * t, 2) * ((c2 + 1) * 2 * t - c2)) * 0.5f
              : (Math.Pow(2 * t - 2, 2) * ((c2 + 1) * (t * 2 - 2) + c2) + 2) *0.5f);

            return a + t * (b - a);
        }

        /// <summary>
        /// Bounces a bit near the end before finishing
        /// </summary>
        public static float EaseInElasticLerp(float a, float b, float t)
        {
            const float c4 = (float)(2 * Math.PI) * 0.33333f;

            t = (float)(t == 0
              ? 0
              : t == 1
              ? 1
              : -Math.Pow(2, 10 * t - 10) * Math.Sin((t * 10 - 10.75) * c4));

            return a + t * (b - a);
        }

        /// <summary>
        /// Bounces a bit near the beinning before finishing
        /// </summary>
        public static float EaseOutElasticLerp(float a, float b, float t)
        {
            const float c4 = (float)(2 * Math.PI) * 0.33333f;

            t = (float)(t == 0
              ? 0
              : t == 1
              ? 1
              : Math.Pow(2, -10 * t) * Math.Sin((t * 10 - 0.75) * c4) + 1);

            return a + t * (b - a);
        }

        ///// <summary>
        ///// Bounces several times before reaching the end
        ///// </summary>
        //public static float EaseInBounceLerp(float a, float b, float t)
        //{
        //    const float c4 = (float)(2 * Math.PI) * 0.33333f;

        //    t = (float)(t == 0
        //      ? 0
        //      : t == 1
        //      ? 1
        //      : Math.Pow(2, -10 * t) * Math.Sin((t * 10 - 0.75) * c4) + 1);

        //    return a + t * (b - a);
        //}

        /// <summary>
        /// Bounces several times before reaching the end
        /// </summary>
        public static float EaseOutBounceLerp(float a, float b, float t)
        {
            const float n1 = 7.5625f;
            const float d1 = 2.75f;

            if (t < 1 / d1)
            {
                t = n1 * t * t;
            }
            else if (t < 2 / d1)
            {
                t = n1 * (t - 1.5f / d1) * t + 0.75f;
            }
            else if (t < 2.5f / d1)
            {
                t = n1 * (t - 2.25f / d1) * t + 0.9375f;
            }
            else
            {
                t = n1 * (t - 2.625f / d1) * t + 0.984375f;
            }

            return a + t * (b - a);
        }


        public static float GradualSlowDownLerp(float a, float b, float t)
        {
            const float INNER_OFFSET = 0.05161f;
            const float OUTER_OFFSET = 0.9838f;

            t = (float)(MathHelper.Log(t + INNER_OFFSET) * 0.33333f + OUTER_OFFSET);

            return a + t * (b - a);
        }
    }
}
