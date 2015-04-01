using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace HashCompute
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Input: {0}", args[0]);
            Console.WriteLine("UTF8 : {0}", Encoding.UTF8.GetString(GetHashValue(args[0])).Replace("\r", "").Replace("\n", ""));
            Console.WriteLine("Hex  : 0x{0}", ConvertHashBytesToString(GetHashValue(args[0])).ToUpper());
        }

        public string GetHashValueAsString(string plainString)
        {
            byte[] result = GetHashValue(plainString);

            // Return the String representation in HEX format per byte value
            return ConvertHashBytesToString(result);
        }

        public static byte[] GetHashValue(string plainString)
        {
            byte[] plainBytes = ConvertStringToByte(plainString);

            // Get the Hash Value in bytes from SHA2 512 bit algorithm method
            SHA512Managed hashAlgorithm = new SHA512Managed();
            return hashAlgorithm.ComputeHash(plainBytes);
        }

        /// <summary>
        /// Convert Hash bytes to Hexadecimal String format 
        /// </summary>
        private static string ConvertHashBytesToString(byte[] hashBytes)
        {
            StringBuilder hashString = new StringBuilder();
            foreach (byte b in hashBytes)
            {
                hashString.Append(b.ToString("x2"));
            }

            return hashString.ToString();
        }

        /// <summary>
        /// Convert the Input String to byte[] for Hash computation
        /// </summary>
        private static byte[] ConvertStringToByte(string plainText)
        {
            // Convert the string to UTF8Encoding  byte[]
            UTF8Encoding Encode = new UTF8Encoding();
            byte[] plainBytes = Encode.GetBytes(plainText);

            return plainBytes;
        }
    }
}
