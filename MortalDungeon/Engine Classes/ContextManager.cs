using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Engine_Classes
{
    public class ContextManager<T> where T : Enum
    {
        protected Dictionary<T, bool> _flags = new Dictionary<T, bool>();

        private object _lock = new object();

        public void SetFlag(T flag, bool value)
        {
            lock (_lock)
            {
                _flags[flag] = value;
            }
        }

        public bool GetFlag(T flag)
        {
            lock (_lock)
            {
                bool value;
                _flags.TryGetValue(flag, out value);

                return value;
            }
        }
    }
}
