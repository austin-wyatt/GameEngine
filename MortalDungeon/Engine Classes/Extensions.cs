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
    }
}
