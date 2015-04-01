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
            bool help = args.Any(a => a.EqualsIgnoreCase("-h") || a.EqualsIgnoreCase("?") || a.EqualsIgnoreCase("--help") || a.EqualsIgnoreCase("/h"));
            bool version = args.Any(a => a.EqualsIgnoreCase("-v") || a.EqualsIgnoreCase("--version") || a.EqualsIgnoreCase("/v"));
            
            if (!help && !version && args.Length > 0)
            {
                try
                {
                    HashAlgorithm algorithm = GetHashAlgorithm(args.Length > 1 ? args[1] : "Default");
                    byte[] hash = GetHash(args[0], algorithm);

                    Console.WriteLine("Input: {0}", args[0]);
                    Console.WriteLine("Hash : {0}", algorithm.GetType().Name);
                    Console.WriteLine("UTF8 : {0}", Encoding.UTF8.GetString(hash).Replace("\r", "").Replace("\n", ""));
                    Console.WriteLine("Hex  : 0x{0}", hash.GetString());
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("{0}: {1}", ex.GetType().Name, ex.Message);
                }
            }
            else if (version)
            {
                Console.WriteLine("HashCompute.exe v{0}.{1}", ApplicationInfo.Version.Major, ApplicationInfo.Version.Minor);
            }
            else
            {
                Console.WriteLine(" === HashCompute v{0}.{1} ===", ApplicationInfo.Version.Major, ApplicationInfo.Version.Minor);
                Console.WriteLine("Computes the hash of the terminal input (as a UTF-8 String)");
                Console.WriteLine("Defaults to SHA512.");
                Console.WriteLine();
                Console.WriteLine("Usage: ");
                Console.WriteLine(" - HashCompute (input) [algorithm]");
                Console.WriteLine();
                Console.WriteLine("Examples: ");
                Console.WriteLine(" - HashCompute test");
                Console.WriteLine(" - HashCompute test MD5");
                Console.WriteLine(" - HashCompute test SHA256");
                Console.WriteLine();
                Console.WriteLine("Supported Algorithms: MD5, SHA1, SHA256, SHA384, SHA512, RIPEMD");
            }
            Console.ResetColor();
        }

        public static byte[] GetHash(string input, HashAlgorithm algorithm = null, Encoding encoding = null)
        {
            algorithm = algorithm ?? new SHA512Managed();

            return algorithm.ComputeHash(input.ToBytes(encoding));
        }

        public static byte[] GetHash(byte[] input, HashAlgorithm algorithm = null)
        {
            algorithm = algorithm ?? new SHA512Managed();

            return algorithm.ComputeHash(input);
        }

        public static HashAlgorithm GetHashAlgorithm(string input = "SHA512")
        {
            switch (input.Replace("-", "").ToUpper())
            {
                case "SHA512":
                case "512":
                case "DEFAULT":
                    return new SHA512Managed();

                case "SHA256":
                case "256":
                    return new SHA256Managed();

                case "SHA384":
                case "384":
                    return new SHA384Managed();

                case "SHA1":
                case "1":
                    return new SHA1Managed();

                case "MD5":
                case "5":
                    return new MD5CryptoServiceProvider();

                case "RIPEMD":
                case "RIP":
                case "160":
                    return new RIPEMD160Managed();

                default:
                    throw new NotSupportedException(String.Format("Unsupported Hash Algorithm ({0})", input));
            }
        }
    }
}
