using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HashSearch
{
    public static class Extensions
    {
        public static int ByteSimilarity(this byte[] input, byte[] other)
        {
            if (input == null || other == null)
                throw new ArgumentException("Byte Arrays not provided");
            if (input.Length != other.Length)
                throw new ArgumentException("Cannot XOR differing length byte arrays");

            int xcount = input.Select((t, i) => t ^ other[i]).Count(xor => xor == 0);
            return xcount - 1;
        }

        public static int BitSimilarity(this byte[] input, byte[] other)
        {
            if (input == null || other == null)
                throw new ArgumentException("Byte Arrays not provided");
            if (input.Length != other.Length)
                throw new ArgumentException("Cannot XOR differing length byte arrays");

            int xcount = 0;
            for (int i = 0; i < input.Length; i++)
            {
                //Todo: Use precomputed array for runtime speed
                int xor = input[i] ^ other[i];
                while (xor != 0)
                {
                    xcount++;
                    xor &= (xor - 1);
                }
            }
            return xcount;
        }

        public static byte[] GetBytes(this string input)
        {
            var sc = StringComparison.OrdinalIgnoreCase;
            input = input.Trim();
            if (input.StartsWith("0x", sc))
                return GetHexBytes(input.Substring(2));
            if (input.StartsWith("16#"))
                return GetHexBytes(input.Substring(3));
            if (input.StartsWith("0b", sc) || input.StartsWith("2#"))
                return GetBinaryBytes(input.Substring(2));
            if (input.StartsWith("0"))
                return GetOctalBytes(input.Substring(1));
            if (input.StartsWith("8#"))
                return GetOctalBytes(input.Substring(2));
            if (input.StartsWith("10#"))
                return GetDecimalBytes(input.Substring(3));
            return GetDecimalBytes(input);
        }

        public static byte[] GetHexBytes(this string hex)
        {
            var sc = StringComparison.OrdinalIgnoreCase;
            hex = hex.Trim();
            if (hex.StartsWith("0x", sc))
                hex = hex.Substring(2);
            else if (hex.StartsWith("16#"))
                hex = hex.Substring(3);

            return Enumerable.Range(0, hex.Length)
                 .Where(x => x % 2 == 0)
                 .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                 .ToArray();
        }

        public static byte[] GetOctalBytes(this string octal)
        {
            octal = octal.Trim();
            if (octal.StartsWith("8#"))
                octal = octal.Substring(2);

            return Enumerable.Range(0, octal.Length)
                 .Select(x => Convert.ToByte(octal.Substring(x, 1), 8))
                 .ToArray();
        }

        public static byte[] GetBinaryBytes(this string binary)
        {
            var sc = StringComparison.OrdinalIgnoreCase;
            binary = binary.Trim();
            if (binary.StartsWith("0b", sc) || binary.StartsWith("2#"))
                binary = binary.Substring(2);

            return Enumerable.Range(0, binary.Length)
                 .Where(x => x % 8 == 0)
                 .Select(x => Convert.ToByte(binary.Substring(x, 8), 2))
                 .ToArray();
        }

        public static byte[] GetDecimalBytes(this string dec)
        {
            dec = dec.Trim();
            if (dec.StartsWith("10#"))
                dec = dec.Substring(3);

            return System.Numerics.BigInteger
                 .Parse(dec)
                 .ToByteArray()
                 .Reverse()
                 .SkipWhile(item => item == 0)
                 .ToArray();
        }

        public static byte[] AddOne(this byte[] input)
        {
            for (int i = input.Length - 1; i >= 0; i--)
            {
                if (input[i] == 0xFF)
                {
                    //carry
                    input[i] = 0x00;
                    if (i == 0)
                        throw new OverflowException("Byte Array Overflow");
                }
                else
                {
                    input[i]++;
                    break;
                }
            }
            return input;
        }
    }
}
