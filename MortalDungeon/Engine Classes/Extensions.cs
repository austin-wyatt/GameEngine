using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Empyrean.Engine_Classes
{
    public static class Extensions
    {
        public static bool Replace<T>(this List<T> list, T itemToReplace, T replacement)
        {
            int index = list.FindIndex(i => i.Equals(itemToReplace));
            
            if (index == -1) return false;

            list.RemoveAt(index);
            list.Insert(index, replacement);

            return true;
        }

        private static Random rng = new ConsistentRandom();
        public static void Randomize<T>(this IList<T> source)
        {
            int n = source.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = source[k];
                source[k] = source[n];
                source[n] = value;
            }
        }

        public static T GetRandom<T>(this IList<T> source)
        {
            return source[rng.Next(source.Count)];
        }

        public static string CreateRandom(this string charset, int seed, int length)
        {
            var stringChars = new char[length];
            var random = new ConsistentRandom(seed);

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = charset[random.Next(charset.Length)];
            }

            var finalString = new string(stringChars);

            return finalString;
        }

        public static bool GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, out TValue data)
        {
            if(!dict.TryGetValue(key, out data))
            {
                data = (TValue)GetDefaultValue(typeof(TValue));
                dict.AddOrSet(key, data);
            }

            return true;
        }

        public static void AddOrSet<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue value)
        {
            if(!dict.TryAdd(key, value))
            {
                dict[key] = value;
            }
        }

        public static TValue LazyGet<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key)
        {
            if(dict.TryGetValue(key, out TValue value))
            {
                return value;
            }

            return (TValue)GetDefaultValue(typeof(TValue));
        }

        public static object GetDefaultValue(Type t)
        {
            return Activator.CreateInstance(t);
        }

        #region Vectors
        #region Vector3
        public static void Add(this ref Vector3 output, ref Vector3 a, ref Vector3 b)
        {
            output.X = a.X + b.X;
            output.Y = a.Y + b.Y;
            output.Z = a.Z + b.Z;
        }

        public static void Sub(this ref Vector3 output, ref Vector3 a, ref Vector3 b)
        {
            output.X = a.X - b.X;
            output.Y = a.Y - b.Y;
            output.Z = a.Z - b.Z;
        }

        public static void Mult(this ref Vector3 output, ref Vector3 a, ref Vector3 b)
        {
            output.X = a.X * b.X;
            output.Y = a.Y * b.Y;
            output.Z = a.Z * b.Z;
        }

        public static void Divide(this ref Vector3 output, ref Vector3 a, ref Vector3 b)
        {
            output.X = a.X / b.X;
            output.Y = a.Y / b.Y;
            output.Z = a.Z / b.Z;
        }
        #endregion
        #region Vector3i
        public static void Add(this ref Vector3i output, ref Vector3i a, ref Vector3i b)
        {
            output.X = a.X + b.X;
            output.Y = a.Y + b.Y;
            output.Z = a.Z + b.Z;
        }

        public static void Sub(this ref Vector3i output, ref Vector3i a, ref Vector3i b)
        {
            output.X = a.X - b.X;
            output.Y = a.Y - b.Y;
            output.Z = a.Z - b.Z;
        }

        public static void Mult(this ref Vector3i output, ref Vector3i a, ref Vector3i b)
        {
            output.X = a.X * b.X;
            output.Y = a.Y * b.Y;
            output.Z = a.Z * b.Z;
        }

        public static void Divide(this ref Vector3i output, ref Vector3i a, ref Vector3i b)
        {
            output.X = a.X / b.X;
            output.Y = a.Y / b.Y;
            output.Z = a.Z / b.Z;
        }
        #endregion
        #endregion

        public static void MultInPlace(this ref Matrix3 left, ref Matrix3 right)
        {
            float x = left.Row0.X;
            float y = left.Row0.Y;
            float z = left.Row0.Z;
            float x2 = left.Row1.X;
            float y2 = left.Row1.Y;
            float z2 = left.Row1.Z;
            float x3 = left.Row2.X;
            float y3 = left.Row2.Y;
            float z3 = left.Row2.Z;
            float x4 = right.Row0.X;
            float y4 = right.Row0.Y;
            float z4 = right.Row0.Z;
            float x5 = right.Row1.X;
            float y5 = right.Row1.Y;
            float z5 = right.Row1.Z;
            float x6 = right.Row2.X;
            float y6 = right.Row2.Y;
            float z6 = right.Row2.Z;
            left.Row0.X = x * x4 + y * x5 + z * x6;
            left.Row0.Y = x * y4 + y * y5 + z * y6;
            left.Row0.Z = x * z4 + y * z5 + z * z6;
            left.Row1.X = x2 * x4 + y2 * x5 + z2 * x6;
            left.Row1.Y = x2 * y4 + y2 * y5 + z2 * y6;
            left.Row1.Z = x2 * z4 + y2 * z5 + z2 * z6;
            left.Row2.X = x3 * x4 + y3 * x5 + z3 * x6;
            left.Row2.Y = x3 * y4 + y3 * y5 + z3 * y6;
            left.Row2.Z = x3 * z4 + y3 * z5 + z3 * z6;
        }
    }
}
