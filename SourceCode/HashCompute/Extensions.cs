using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HashCompute
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
        /// Convert Hash bytes to Hexadecimal String format 
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
    }
}
