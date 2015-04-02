using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace HashCompute
{
    class Program
    {
        private static bool Verbose = false;
        private static bool Managed = true;

        static void Main(string[] args)
        {
            var options = new Options();
            if (CommandLine.Parser.Default.ParseArguments(args, options))
            {
                Verbose = options.Verbose;
                Managed = !options.Unmanaged;

                if (options.Version)
                    Console.WriteLine("HashCompute.exe v{0}.{1}", ApplicationInfo.Version.Major, ApplicationInfo.Version.Minor);
                else if (options.Help || args.Any(a => a.Equals("?")))
                    ShowHelp();
                else
                    ComputeHash(options.Input, options.Algorithm);

                if (!options.NoNewLine)
                    Console.WriteLine();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Unknown Arguments: {0}", String.Join(" ", args));
                Console.ResetColor();

                ShowHelp();
            }

            Console.ResetColor();
        }

        public static void ComputeHash(string input, string algorithm = null)
        {
            try
            {
                HashAlgorithm ha = GetHashAlgorithm(algorithm ?? "Default", Managed);
                byte[] hash = GetHash(input, ha);

                if (Verbose)
                {
                    Console.WriteLine("Input: {0}", input);
                    Console.WriteLine("Hash : {0}", ha.GetType().Name);
                    Console.WriteLine("UTF8 : {0}", Encoding.UTF8.GetString(hash).Replace("\r", "").Replace("\n", ""));
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("Hex  : 0x{0}", hash.GetString());
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("0x{0}", hash.GetString());
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("{0}: {1}", ex.GetType().Name, ex.Message);
            }
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

        public static HashAlgorithm GetHashAlgorithm(string algorithm = "SHA512", bool managed = true)
        {
            switch (algorithm.ToAlphanumeric().ToUpper())
            {
                case "SHA512":
                case "512":
                case "DEFAULT":
                    return managed ? (HashAlgorithm)new SHA512Managed() : new SHA512CryptoServiceProvider();

                case "SHA256":
                case "256":
                    return managed ? (HashAlgorithm)new SHA256Managed() : new SHA256CryptoServiceProvider();

                case "SHA384":
                case "384":
                    return managed ? (HashAlgorithm)new SHA384Managed() : new SHA384CryptoServiceProvider();

                case "SHA1":
                case "1":
                    return managed ? (HashAlgorithm)new SHA1Managed() : new SHA1CryptoServiceProvider();

                case "MD5":
                case "MD":
                case "5":
                    //Unmanaged Only
                    return new MD5CryptoServiceProvider();

                case "RIPEMD":
                case "RIP":
                case "EMD":
                case "160":
                    //Managed Only
                    return new RIPEMD160Managed();

                default:
                    throw new NotSupportedException(String.Format("Unsupported Hash Algorithm ({0})", algorithm));
            }
        }

        public static void ShowHelp()
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(" === HashCompute v{0}.{1} ===", ApplicationInfo.Version.Major, ApplicationInfo.Version.Minor);
            Console.ResetColor();
            Console.WriteLine("Computes the hash of the terminal input (as a UTF-8 String)");
            Console.WriteLine("Defaults to SHA512.");
            Console.WriteLine();
            Console.WriteLine("Usage: ");
            Console.WriteLine(" - HashCompute (Input) [Algorithm] [Options]");
            Console.WriteLine();
            Console.WriteLine("Examples: ");
            Console.WriteLine(" - HashCompute test");
            Console.WriteLine(" - HashCompute test MD5");
            Console.WriteLine(" - HashCompute test SHA256 -uvn");
            Console.WriteLine(" - HashCompute test --algorithm=SHA1 --unmanaged --verbose --nonewline");
            Console.WriteLine(" - HashCompute [? | /? | -? | -h | --help | --version]");
            Console.WriteLine();
            Console.WriteLine("Supported Algorithms: MD5, SHA1, SHA256, SHA384, SHA512, RIPEMD");
        }
    }
}
