using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Core
{
    public static class Extensions
    {
        /// <summary>
        /// Convert Hash bytes to Hexadecimal String format 
        /// </summary>
        public static string GetString(this byte[] hashBytes, bool uppercase = true)
        {
            StringBuilder hashString = new StringBuilder();

            foreach (byte b in hashBytes)
            {
                hashString.Append(b.ToString("x2"));
            }

            return uppercase ? hashString.ToString().ToUpper() : hashString.ToString();
        }

        /// <summary>
        /// Convert Hash bytes to Hexadecimal String format. Defaults to UTF8 Encoding.
        /// </summary>
        public static byte[] ToBytes(this string input, Encoding encoding = null)
        {
            encoding = encoding ?? new UTF8Encoding();
            return encoding.GetBytes(input);
        }

        /// <summary>
        /// Convert Hash bytes to Hexadecimal String format 
        /// </summary>
        public static bool EqualsIgnoreCase(this string input, string other, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            return input.Equals(other, comparison);
        }

        /// <summary>
        /// Remove non-alphanumeric characters from a string (and optionally whitespace as well)
        /// </summary>
        public static string ToAlphanumeric(this string input, bool allowWhiteSpace = false)
        {
            if (allowWhiteSpace)
                return new string(Array.FindAll(input.ToCharArray(), (c => (char.IsLetterOrDigit(c) || char.IsWhiteSpace(c)))));
            
            return new string(Array.FindAll(input.ToCharArray(), (c => (char.IsLetterOrDigit(c)))));
        }

        public static int ByteSimilarity(this byte[] input, byte[] other)
        {
            if (input == null || other == null)
                throw new ArgumentException("Byte Arrays not provided");
            if (input.Length != other.Length)
                throw new ArgumentException("Cannot XOR differing length byte arrays");

            int xcount = input.Select((t, i) => t ^ other[i]).Count(xor => xor == 0);
            return xcount;
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
            input = input.Trim();
            if (input.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                return GetHexBytes(input.Substring(2), true);
            if (input.StartsWith("16#"))
                return GetHexBytes(input.Substring(3), true);
            if (input.StartsWith("0b", StringComparison.OrdinalIgnoreCase) || input.StartsWith("2#"))
                return GetBinaryBytes(input.Substring(2), true);
            if (input.StartsWith("8#") || input.StartsWith("0o"))
                return GetOctalBytes(input.Substring(2), true);
            if (input.StartsWith("0"))
                return GetOctalBytes(input, true);
            if (input.StartsWith("10#"))
                return GetDecimalBytes(input.Substring(3), true);
            return GetDecimalBytes(input, true);
        }

        public static byte[] GetHexBytes(this string hex, bool preTrimmed = false)
        {
            if (!preTrimmed)
            {
                hex = hex.Trim();
                if (hex.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                    hex = hex.Substring(2);
                else if (hex.StartsWith("16#"))
                    hex = hex.Substring(3);
            }

            if (hex.Length % 2 != 0) hex = hex.PadLeft(hex.Length + 1, '0');

            return Enumerable.Range(0, hex.Length)
                 .Where(x => x % 2 == 0)
                 .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                 .ToArray();
        }

        public static byte[] GetOctalBytes(this string octal, bool preTrimmed = false)
        {
            if (!preTrimmed)
            {
                octal = octal.Trim();
                if (octal.StartsWith("8#") || octal.StartsWith("0o"))
                    octal = octal.Substring(2);
            }

            octal = octal.TrimStart('0');

            byte[] octalDigits = Enumerable.Range(0, octal.Length)
                 .Select(x => Convert.ToByte(octal.Substring(x, 1), 8))
                 .ToArray();

            int length = octalDigits.Length % 8 == 0 ? octalDigits.Length * 3 / 8 : octalDigits.Length * 3 / 8 + 1;
            BitArray ba = new BitArray(length * 8);
            
            for (int i = octalDigits.Length - 1; i >= 0; i--)
            {
                ba[(octalDigits.Length - i) * 3 - 3] = (octalDigits[i] & 0x01) == 1;
                ba[(octalDigits.Length - i) * 3 - 2] = (octalDigits[i] & 0x02) == 2;
                ba[(octalDigits.Length - i) * 3 - 1] = (octalDigits[i] & 0x04) == 4;
            }

            byte[] result = new byte[length];

            ba.CopyTo(result, 0);

            if (result[length - 1] == 0x00)
            {
                result = result.Take(result.Length - 1).ToArray();
            }

            return result.Reverse().ToArray();
        }

        public static byte[] GetBinaryBytes(this string binary, bool preTrimmed = false)
        {
            if (!preTrimmed)
            {
                binary = binary.Trim();
                if (binary.StartsWith("0b", StringComparison.OrdinalIgnoreCase) || binary.StartsWith("2#"))
                    binary = binary.Substring(2);
            }

            if (binary.Length % 8 != 0) binary = binary.PadLeft(binary.Length + 8 - binary.Length % 8, '0');

            return Enumerable.Range(0, binary.Length)
                 .Where(x => x % 8 == 0)
                 .Select(x => Convert.ToByte(binary.Substring(x, 8), 2))
                 .ToArray();
        }

        public static byte[] GetDecimalBytes(this string dec, bool preTrimmed = false)
        {
            if (!preTrimmed)
            {
                dec = dec.Trim();
                if (dec.StartsWith("10#"))
                    dec = dec.Substring(3);
            }

            return Enumerable.Range(0, dec.Length)
                 .Select(x => Convert.ToByte(dec.Substring(x, 1), 10))
                 .Reverse()
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
