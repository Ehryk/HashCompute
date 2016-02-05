using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HashSearch
{
    public static class PopCount
    {
        public static readonly int[] PopTable8 = {
            0, 1, 1, 2, 1, 2, 2, 3, 1, 2, 2, 3, 2, 3, 3, 4,
            1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3, 4, 3, 4, 4, 5,
            1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3, 4, 3, 4, 4, 5,
            2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6,
            1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3, 4, 3, 4, 4, 5,
            2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6,
            2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6,
            3, 4, 4, 5, 4, 5, 5, 6, 4, 5, 5, 6, 5, 6, 6, 7,
            1, 2, 2, 3, 2, 3, 3, 4, 2, 3, 3, 4, 3, 4, 4, 5,
            2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6,
            2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6,
            3, 4, 4, 5, 4, 5, 5, 6, 4, 5, 5, 6, 5, 6, 6, 7,
            2, 3, 3, 4, 3, 4, 4, 5, 3, 4, 4, 5, 4, 5, 5, 6,
            3, 4, 4, 5, 4, 5, 5, 6, 4, 5, 5, 6, 5, 6, 6, 7,
            3, 4, 4, 5, 4, 5, 5, 6, 4, 5, 5, 6, 5, 6, 6, 7,
            4, 5, 5, 6, 5, 6, 6, 7, 5, 6, 6, 7, 6, 7, 7, 8,
        };
        private static int[] poptable = PopTable8;
        private static int bit = 8;

        public static int Bit
        {
            get { return bit; }
            set
            {
                if (value > Math.Floor(Math.Log(2147483591, 2)))
                    throw new ArgumentOutOfRangeException("value", "Bit Value cannot exceed C# index maximum value (2147483591, just below Int32.MaxValue)");
                if (bit != value)
                    poptable = null;
                bit = value;
            }
        }

        public static int[] PopTable
        {
            get
            {
                if (poptable == null)
                    poptable = GenerateTable(bit);
                return poptable;
            }
        }

        public static int[] GenerateTable(int bits)
        {
            int max = 1 << bits;
            int[] table = new int[max];

            for (int i = 0; i < max; i++)
            {
                table[i] = (i & 1) + table[i/2];
            }

            return table;
        }

        public static int Lookup(short value, bool check = true)
        {
            return Lookup((ulong)value, check);
        }
        public static int Lookup(ushort value, bool check = true)
        {
            return Lookup((ulong)value, check);
        }
        public static int Lookup(int value, bool check = true)
        {
            return Lookup((ulong)value, check);
        }
        public static int Lookup(uint value, bool check = true)
        {
            return Lookup((ulong)value, check);
        }
        public static int Lookup(long value, bool check = true)
        {
            return Lookup((ulong)value, check);
        }
        public static int Lookup(byte value, bool check = true)
        {
            return Lookup((ulong)value, check);
        }
        public static int Lookup(byte[] value, bool check = true)
        {
            return value.Sum(b => Lookup((ulong) b, check));
        }

        public static int Lookup(ulong value, bool check = true)
        {
            if (check && Math.Floor(Math.Log(value, 2)) > bit)
                throw new ArgumentOutOfRangeException("value", "Value passed was outside the bit size of the PopTable (2^Bit)");
            return PopTable[value];
        }

        public static int SparseLoop(ulong value)
        {
            int popcount = 0;
            while (value != 0)
            {
                popcount++;
                value &= value - 1; //Reset LS1B
            }
            return popcount;
        }

        public static int DjinnIsenberg(ulong value)
        {
            ulong result = value - ((value >> 1) & 0x5555555555555555ul);
            result = (result & 0x3333333333333333ul) + ((result >> 2) & 0x3333333333333333ul);
            return (int)(unchecked(((result + (result >> 4)) & 0xF0F0F0F0F0F0F0Ful) * 0x101010101010101ul) >> 56);
        }

        public static int DivideConquer32(int i)
        {
            i = i & 0x55555555 + (i >> 1) & 0x55555555;
            i = i & 0x33333333 + (i >> 2) & 0x33333333;
            i = i & 0x0f0f0f0f + (i >> 4) & 0x0f0f0f0f;
            i = i & 0x00ff00ff + (i >> 8) & 0x00ff00ff;
            return (unchecked(i & 0x0000ffff + (i >> 16) & 0x0000ffff));
        }

        public static int DivideConquer64(long i)
        {
            i = i & 0x5555555555555555 + (i >> 1) & 0x5555555555555555;
            i = i & 0x3333333333333333 + (i >> 2) & 0x3333333333333333;
            i = i & 0x0f0f0f0f0f0f0f0f + (i >> 4) & 0x0f0f0f0f0f0f0f0f;
            i = i & 0x00ff00ff00ff00ff + (i >> 8) & 0x00ff00ff00ff00ff;
            i = i & 0x0000ffff0000ffff + (i >> 16) & 0x0000ffff0000ffff;
            return (int)(unchecked(i & 0x00000000ffffffff + (i >> 32) & 0x00000000ffffffff));
        }
    }
}
