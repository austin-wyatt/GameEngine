﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MortalDungeon.Engine_Classes
{
    internal static class Extensions
    {
        internal static bool Replace<T>(this List<T> list, T itemToReplace, T replacement)
        {
            int index = list.FindIndex(i => i.Equals(itemToReplace));
            
            if (index == -1) return false;

            list.RemoveAt(index);
            list.Insert(index, replacement);

            return true;
        }

        private static Random rng = new Random();
        internal static void Randomize<T>(this IList<T> source)
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
    }
}
