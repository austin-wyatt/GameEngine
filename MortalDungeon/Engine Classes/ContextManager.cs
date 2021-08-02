﻿using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Engine_Classes
{
    public class ContextManager<T> where T : Enum
    {
        protected Dictionary<T, bool> _flags = new Dictionary<T, bool>();

        public void SetFlag(T flag, bool value)
        {
            _flags[flag] = value;
        }

        public bool GetFlag(T flag)
        {
            bool value = false;
            _flags.TryGetValue(flag, out value);

            return value;
        }
    }
}
