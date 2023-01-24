using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Empyrean.Engine_Classes
{
    public static class BitOps
    {
        public static bool GetBit(int num, int bitNumber)
        {
            return (num & (1 << bitNumber)) != 0;
        }

        public static int NearestPowerOf2(uint x)
        {
            return 1 << (sizeof(uint) * 8 - BitOperations.LeadingZeroCount(x - 1));
        }

        public static unsafe float GetNext(this float x)
        {
            int temp = *(int*)&x + 1;
            return *(float*)&temp;
        }

        public static unsafe float GetPrevious(this float x)
        {
            uint temp = *(uint*)&x - 1000;
            return *(float*)&temp;
        }
    }
}
