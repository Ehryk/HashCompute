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
        /// Convert a byte array to a Hexadecimal String 
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
        /// Converts a string to a byte array. Defaults to UTF8 Encoding.
        /// </summary>
        public static byte[] ToBytes(this string input, Encoding encoding = null)
        {
            encoding = encoding ?? new UTF8Encoding();
            return encoding.GetBytes(input);
        }

        /// <summary>
        /// Determines whether this string and a specified System.String object have the same value, while ignoring case by default (OrdinalIgnoreCase).
        /// </summary>
        public static bool EqualsIgnoreCase(this string input, string other, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            return input.Equals(other, comparison);
        }

        /// <summary>
        /// Remove any non-alphanumeric characters from a string (and optionally whitespace as well)
        /// </summary>
        public static string ToAlphanumeric(this string input, bool allowWhiteSpace = false)
        {
            if (allowWhiteSpace)
                return new string(Array.FindAll(input.ToCharArray(), (c => (char.IsLetterOrDigit(c) || char.IsWhiteSpace(c)))));
            
            return new string(Array.FindAll(input.ToCharArray(), (c => (char.IsLetterOrDigit(c)))));
        }

        /// <summary>
        /// Returns the similarity index (how many bytes in the arrays are identical)
        /// </summary>
        public static int ByteSimilarity(this byte[] input, byte[] other)
        {
            if (input == null || other == null)
                throw new ArgumentException("Byte Arrays not provided");
            if (input.Length != other.Length)
                throw new ArgumentException("Cannot XOR differing length byte arrays");

            //Precompiled Implementation
            //int xcount = input.Select((t, i) => Precompiled.XOR[t, other[i]]).Count(xor => xor == 0);

            //.NET Implentation
            int xcount = input.Select((t, i) => t ^ other[i]).Count(xor => xor == 0);

            return xcount;
        }

        /// <summary>
        /// Returns the similarity index (how many bits in the arrays are identical)
        /// </summary>
        public static int BitSimilarity(this byte[] input, byte[] other)
        {
            if (input == null || other == null)
                throw new ArgumentException("Byte Arrays not provided");
            if (input.Length != other.Length)
                throw new ArgumentException("Cannot XOR differing length byte arrays");

            int xcount = 0;
            for (int i = 0; i < input.Length; i++)
            {
                //Precompiled Implementation
                //int xor = Precompiled.XOR[input[i], other[i]];

                //.NET Implentation
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

            BitArray bits = new BitArray(octal
                .Reverse()
                .SelectMany(x =>
                    {
                        byte value = (byte)(x - '0');
                        return new bool[] { (value & 0x01) == 1, (value & 0x02) == 2, (value & 0x04) == 4 };
                    })
                .ToArray());

            byte[] bytes = new byte[bits.Length / 8 + 1];
            bits.CopyTo(bytes, 0);

            if (bytes[bytes.Length - 1] == 0x00)
                bytes = bytes.Take(bytes.Length - 1).ToArray();
                    
            return bytes;
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

            return dec
                // interpret the string as a list of digits with position
                .Reverse()
                // transfer from list of positioned digits to list of actual bit positions,
                // by repeatedly multiplying with 10
                // the resulting bits need to be added for the final result
                .SelectMany((x, i) =>
                {
                    // digit value
                    var val1 = x - '0';
                    var res1 = new List<int>();
                    // to bit positions, as if it was the first digit
                    for (int j = 0; j < 8; j++)
                    {
                        if ((val1 & (1 << j)) != 0) res1.Add(j);
                    }
                    // to absolute bit positions, taking the digit position into account
                    for (int j = 1; j <= i; j++)
                    {
                        var res = new List<int>();
                        // multiply by 10, until actual position is reached
                        foreach (var item in res1)
                        {
                            res.Add(item + 1);
                            res.Add(item + 3);
                        }
                        // compress bits
                        res1 = res.Aggregate(new HashSet<int>(), (set, i1) =>
                        {
                            // two bits in the same position add up to one bit in a higher position
                            while (set.Contains(i1))
                            {
                                set.Remove(i1);
                                i1++;
                            }
                            set.Add(i1);
                            return set;
                        }).ToList();
                    }
                    return res1;
                }).
                // final elimination of duplicate bit indices
                Aggregate(new HashSet<int>(), (set, i) =>
                {
                    while (set.Contains(i))
                    {
                        set.Remove(i);
                        i++;
                    }
                    set.Add(i);
                    return set;
                })
                // transfer bit positions into a byte array - lowest bit is the last bit of the first byte
                .Aggregate(new byte[dec.Length / 2], (res, bitpos) =>
                {
                    res[bitpos / 8] |= (byte)(1 << (bitpos % 8));
                    return res;
                });
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
