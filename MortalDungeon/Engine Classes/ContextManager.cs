using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Engine_Classes
{
    internal class ContextManager<T> where T : Enum
    {
        protected Dictionary<T, bool> _flags = new Dictionary<T, bool>();

        internal void SetFlag(T flag, bool value)
        {
            _flags[flag] = value;
        }

        internal bool GetFlag(T flag)
        {
            bool value;
            _flags.TryGetValue(flag, out value);

            return value;
        }
    }
}
