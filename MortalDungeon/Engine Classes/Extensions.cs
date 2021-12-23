using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MortalDungeon.Engine_Classes
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
    }
}
