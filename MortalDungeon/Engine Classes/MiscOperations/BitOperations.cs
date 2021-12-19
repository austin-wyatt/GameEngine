using System;
using System.Collections.Generic;
using System.Text;

namespace MortalDungeon.Engine_Classes
{
    internal static class BitOps
    {
        internal static bool GetBit(int num, int bitNumber)
        {
            return (num & (1 << bitNumber)) != 0;
        }
    }
}
