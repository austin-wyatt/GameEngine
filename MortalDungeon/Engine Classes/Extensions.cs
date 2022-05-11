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
    }
}
