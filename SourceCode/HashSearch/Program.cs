using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using HashCompute;

namespace HashSearch
{
    class Program
    {
        public const int SUCCESS = 0;
        public const int FAILURE_UNSPECIFIED = 10;
        public const int FAILURE_EXCEPTION = 20;
        public const int FAILURE_NO_INPUT = 30;
        public const int FAILURE_ARGUMENTS = 40;

        public static bool Verbose = false;
        public static bool Managed = true;
        public static bool UpperCase = false;
        public static bool Color = true;
        public static bool Omit0x = false;

        static int Main(string[] args)
        {
            int retCode = FAILURE_UNSPECIFIED;

            try
            {
                var options = new Options();
                if (CommandLine.Parser.Default.ParseArguments(args, options))
                {
                    Verbose = options.Verbose;
                    Managed = !options.Unmanaged;
                    UpperCase = !options.LowerCase;
                    Color = !options.NoColor;
                    Omit0x = options.Omit0x;

                    if (options.Version)
                    {
                        //Show Version Information and exit
                        Console.Write("{0} v{1}.{2}.{3}.{4} ({5})", ApplicationInfo.Title, ApplicationInfo.Version.Major, ApplicationInfo.Version.Minor, ApplicationInfo.Version.Build, ApplicationInfo.Version.Revision, ApplicationInfo.CopyrightHolder);
                        retCode = SUCCESS;
                    }
                    else if (options.Help || args.Any(a => a.Equals("?") || a.Equals("-?") || a.Equals("/?") || a.Equals("--?")))
                    {
                        //Show Usage/Help and exit
                        ShowHelp();
                        retCode = SUCCESS;
                    }
                    else
                    {
                        HashAlgorithm ha = Hashes.GetHashAlgorithm(options.Algorithm, !options.Unmanaged);

                        byte[] seed = options.Seed.GetBytes();
                        Random random = new Random();
                        byte[] previous = new byte[ha.HashSize / 8];
                        byte[] next;
                        int similarity;

                        int copyStart = previous.Length - seed.Length;
                        Buffer.BlockCopy(seed, 0, previous, copyStart, seed.Length);
                        while (true)
                        {
                            next = ha.ComputeHash(previous);
                            similarity = next.ByteSimilarity(previous);

                            if (similarity >= options.Threshold)
                            {
                                Console.WriteLine("Input {0}{1} Has Similarity Index {2}.", Omit0x ? "" : "0x", previous.GetString(UpperCase), similarity);
                                if (Verbose)
                                    Console.WriteLine("(Hash {0}{1})", Omit0x ? "" : "0x", next.GetString(UpperCase));
                            }

                            if (Console.KeyAvailable)
                            {
                                var key = Console.ReadKey(true);
                                if (key.Key == ConsoleKey.C)
                                {
                                    if (Color)
                                        Console.ForegroundColor = ConsoleColor.Cyan;
                                    Console.WriteLine("Current Value: {0}{1}", Omit0x ? "" : "0x", previous.GetString(UpperCase));
                                    Console.ResetColor();
                                }
                                if (key.Key == ConsoleKey.P)
                                {
                                    if (Color)
                                        Console.ForegroundColor = ConsoleColor.Green;
                                    Console.WriteLine("Paused. Press any key to continue...");
                                    Console.ReadKey(true);
                                    Console.ResetColor();
                                }
                                if (key.Key == ConsoleKey.Q)
                                {
                                    retCode = SUCCESS;
                                    break;
                                }
                            }

                            if (options.Chase)
                                previous = next;
                            else if (options.Random)
                                random.NextBytes(previous);
                            else //Sequential
                                previous = previous.AddOne();
                        }
                    }

                    if (!options.NoNewLine)
                        Console.WriteLine();
                }
                else
                {
                    //Failed Parsing Command Line Options
                    if (args.Length > 0 && !args.Any(a => a.Equals("?") || a.Equals("-?") || a.Equals("/?") || a.Equals("--?")))
                    {
                        if (Color)
                            Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Unknown Arguments: {0}", String.Join(" ", args));
                        Console.ResetColor();
                    }

                    ShowHelp();

                    Console.WriteLine();
                    retCode = FAILURE_ARGUMENTS;
                }
            }
            catch (Exception ex)
            {
                //Display Exception Message
                if (Color)
                    Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine();
                Console.WriteLine("{0}: {1}", ex.GetType().Name, ex.Message);
                retCode = FAILURE_EXCEPTION;
            }

            Console.ResetColor();
            return retCode;
        }

        public static void ShowHelp()
        {
            Console.WriteLine();
            if (Color)
                Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(" === HashCompute v{0}.{1} ===", ApplicationInfo.Version.Major, ApplicationInfo.Version.Minor);
            Console.ResetColor();
            Console.WriteLine("Searches through hash domains");
            Console.WriteLine("Multiple algorithms available, defaults to MD5.");
            Console.WriteLine();
            Console.WriteLine("Usage and Examples: ");
            Console.WriteLine(" > HashCompute [Algorithm] [Seed]");
            Console.WriteLine(" > HashCompute");
            Console.WriteLine(" > HashCompute sha256 -uvnlx8");
            Console.WriteLine(" > HashCompute --algorithm=SHA1 --seed=0x9bc30f485ad7 --unmanaged --nonewline --lowercase");
            Console.WriteLine(" > HashCompute [? | /? | -? | -h | --help | --version]");
            Console.WriteLine();
            Console.WriteLine("Options");
            Console.WriteLine(" -v/--verbose    : Add additional output");
            Console.WriteLine(" -n/--nonewline  : Output without trailing newline");
            Console.WriteLine(" -l/--lowercase  : Output hex with lowercase (0DE3 => 0de3)");
            Console.WriteLine(" -x/--omit0x     : Omit 0x prefix from hex output");
            Console.WriteLine(" -u/--unmanaged  : Use unmanaged hash algorithm, if available");
            Console.WriteLine(" -c/--color      : Disable colored output");
            Console.WriteLine();
            Console.WriteLine("Supported Algorithms: MD5, SHA1, SHA256, SHA384, SHA512, RIPEMD");
        }
    }
}
