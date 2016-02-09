using System;
using System.Timers;
using System.Linq;
using System.Security.Cryptography;
using Core;

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

        private static Timer UpdateTimer;
        public static bool UpdateNeeded = false;

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

                        bool hasFinal = options.Final != null;
                        byte[] finalValue = new byte[ha.HashSize / 8];
                        if (hasFinal)
                        {
                            byte[] finalInput = options.Final.GetBytes();
                            int copyFrom = ha.HashSize / 8 - finalInput.Length;
                            Buffer.BlockCopy(finalInput, 0, finalValue, copyFrom, finalInput.Length);
                        }

                        if (options.Database)
                            InitializeUpdateTimer();
                        
                        Random random = new Random();
                        long inputCount = 0;
                        byte[] input = new byte[ha.HashSize / 8];
                        byte[] result;
                        int similarity;
                        //ChainLength Mode
                        long chainLength = 0;
                        byte[] chainStart = new byte[ha.HashSize / 8];

                        int copyStart = input.Length - seed.Length;
                        Buffer.BlockCopy(seed, 0, input, copyStart, seed.Length);
                        Buffer.BlockCopy(input, 0, chainStart, 0, ha.HashSize / 8);

                        string searchMode = GetSearchMode(options);
                        int? searchID = null;
                        if (options.Database)
                            searchID = DataAccess.SearchStart(options.Algorithm, Environment.MachineName, searchMode, input);

                        while (true)
                        {
                            result = ha.ComputeHash(input);

                            if (options.ChainLength)
                            {
                                chainLength++;
                                if (result.SequenceEqual(chainStart))
                                {
                                    if (Color)
                                        Console.ForegroundColor = ConsoleColor.Magenta;
                                    Console.WriteLine("Chain of Length {0} found: {1}.", chainLength, chainStart.GetString(UpperCase));
                                    Console.ResetColor();

                                    if (options.Database)
                                        DataAccess.ChainLengthInsert(options.Algorithm, chainStart, chainLength);

                                    try
                                    {
                                        chainStart = chainStart.AddOne();
                                    }
                                    catch (OverflowException ex)
                                    {
                                        //Chainlength Mode has overflown; domain exhausted
                                        if (Color)
                                            Console.ForegroundColor = ConsoleColor.Green;
                                        Console.Write("Processing Finished: Domain Exhausted", ex.Message);
                                        Console.ResetColor();

                                        retCode = SUCCESS;
                                        break;
                                    }
                                    chainLength = 0;
                                    Buffer.BlockCopy(chainStart, 0, result, 0, ha.HashSize / 8);
                                    inputCount++;

                                    if (hasFinal && chainStart.SequenceEqual(finalValue))
                                    {
                                        retCode = SUCCESS;
                                        break;
                                    }
                                }
                                else if (options.MaxChain != 0 && chainLength > options.MaxChain)
                                {
                                    if (Verbose)
                                    {
                                        if (Color)
                                            Console.ForegroundColor = ConsoleColor.Yellow;
                                        Console.WriteLine("No Chain Found for {0}{1} (Max: {2}).", Omit0x ? "" : "0x", chainStart.GetString(UpperCase), options.MaxChain);
                                        Console.ResetColor();
                                    }

                                    try
                                    {
                                        chainStart = chainStart.AddOne();
                                    }
                                    catch (OverflowException ex)
                                    {
                                        //Chainlength Mode has overflown; domain exhausted
                                        if (Color)
                                            Console.ForegroundColor = ConsoleColor.Green;
                                        Console.Write("Processing Finished: Domain Exhausted", ex.Message);
                                        Console.ResetColor();

                                        retCode = SUCCESS;
                                        break;
                                    }
                                    chainLength = 0;
                                    Buffer.BlockCopy(chainStart, 0, result, 0, ha.HashSize / 8);
                                    inputCount++;

                                    if (hasFinal && chainStart.SequenceEqual(finalValue))
                                    {
                                        retCode = SUCCESS;
                                        break;
                                    }
                                }
                                else if (Verbose)
                                {
                                    Console.WriteLine("  - {0}{1} => {0}{2} ({3})", Omit0x ? "" : "0x", chainStart.GetString(UpperCase), result.GetString(UpperCase), chainLength);
                                }
                            }
                            else
                            {
                                inputCount++;
                                if (options.ByteSimilarity)
                                    similarity = result.ByteSimilarity(input);
                                else
                                    similarity = result.BitSimilarity(input);

                                bool fixPoint = input.SequenceEqual(result);
                                if (similarity >= options.Threshold || fixPoint)
                                {
                                    if (fixPoint)
                                    {
                                        if (Color)
                                            Console.ForegroundColor = ConsoleColor.Magenta;
                                        Console.WriteLine("Fix Point Found!!!");
                                        Console.ResetColor();
                                    }

                                    Console.WriteLine("Input {0}{1} Has Similarity Index {2}.", Omit0x ? "" : "0x", input.GetString(UpperCase), similarity);
                                    if (Verbose)
                                        Console.WriteLine("(Hash {0}{1})", Omit0x ? "" : "0x", result.GetString(UpperCase));
                                    if (options.Database)
                                    {
                                        if (options.ByteSimilarity)
                                            DataAccess.SimilarityInsert(options.Algorithm, input, result, null, similarity, fixPoint);
                                        else
                                            DataAccess.SimilarityInsert(options.Algorithm, input, result, similarity, null, fixPoint);
                                    }

                                    if (fixPoint && options.Chase)
                                    {
                                        //Chase Mode has found the Fix Point, and would then loop infinitely
                                        retCode = SUCCESS;
                                        break;
                                    }
                                }
                            }

                            if (Console.KeyAvailable)
                            {
                                var key = Console.ReadKey(true);
                                if (key.Key == ConsoleKey.C || key.Key == ConsoleKey.S )
                                {
                                    if (Color)
                                        Console.ForegroundColor = ConsoleColor.Cyan;
                                    if (options.ChainLength)
                                        Console.WriteLine("{4}, Mode: {3}, Chain Start: {0}{1}, Chain Length: {2}", Omit0x ? "" : "0x", chainStart.GetString(UpperCase), chainLength, searchMode, options.Algorithm.ToUpper());
                                    else
                                        Console.WriteLine("{4}, Mode: {3}, Current Value: {0}{1}, Inputs: {2}", Omit0x ? "" : "0x", input.GetString(UpperCase), inputCount, searchMode, options.Algorithm.ToUpper());
                                    Console.ResetColor();
                                }
                                if (key.Key == ConsoleKey.P)
                                {
                                    if (Color)
                                        Console.ForegroundColor = ConsoleColor.Green;
                                    if (options.ChainLength)
                                        Console.WriteLine("{4}, Mode: {3}, Chain Start: {0}{1}, Chain Length: {2}", Omit0x ? "" : "0x", chainStart.GetString(UpperCase), chainLength, searchMode, options.Algorithm.ToUpper());
                                    else
                                        Console.WriteLine("{4}, Mode: {3}, Current Value: {0}{1}, Inputs: {2}", Omit0x ? "" : "0x", input.GetString(UpperCase), inputCount, searchMode, options.Algorithm.ToUpper());
                                    Console.WriteLine("Paused. Press any key to continue...");
                                    Console.ReadKey(true);
                                    Console.ResetColor();
                                }
                                if (key.Key == ConsoleKey.Q)
                                {
                                    if (Color)
                                        Console.ForegroundColor = ConsoleColor.Green;
                                    if (options.ChainLength)
                                        Console.WriteLine("{4}, Mode: {3}, Chain Start: {0}{1}, Chain Length: {2}", Omit0x ? "" : "0x", chainStart.GetString(UpperCase), chainLength, searchMode, options.Algorithm.ToUpper());
                                    else
                                        Console.WriteLine("{4}, Mode: {3}, Current Value: {0}{1}, Inputs: {2}", Omit0x ? "" : "0x", input.GetString(UpperCase), inputCount, searchMode, options.Algorithm.ToUpper());
                                    Console.WriteLine("Quit.");
                                    retCode = SUCCESS;
                                    break;
                                }
                            }

                            if (options.Database && UpdateNeeded)
                            {
                                try
                                {
                                    DataAccess.SearchUpdate(searchID.Value, inputCount, options.ChainLength ? chainStart : input);
                                }
                                catch (Exception ex)
                                {
                                    if (Color)
                                        Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine("Could not update search: {0}", ex.Message);
                                    Console.ResetColor();
                                }
                                UpdateNeeded = false;
                            }

                            if (options.Chase || options.ChainLength || options.ChainStore)
                                Buffer.BlockCopy(result, 0, input, 0, ha.HashSize / 8);
                            else if (options.Random)
                                random.NextBytes(input);
                            else //Sequential
                            {
                                try
                                {
                                    input = input.AddOne();

                                    if (hasFinal && input.SequenceEqual(finalValue))
                                        throw new OverflowException("Final Value has been reached");
                                }
                                catch (OverflowException ex)
                                {
                                    //Sequential Mode has overflown; domain exhausted
                                    if (Color)
                                        Console.ForegroundColor = ConsoleColor.Green;
                                    Console.Write("Processing Finished: {0}", ex.Message.StartsWith("Final") ? ex.Message : "Domain Exhausted");
                                    Console.ResetColor();

                                    retCode = SUCCESS;
                                    break;
                                }
                            }
                        }

                        //End Search in DB
                        if (options.Database && searchID.HasValue)
                            DataAccess.SearchEnd(searchID.Value, inputCount, options.ChainLength ? chainStart : input);
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

        public static string GetSearchMode(Options options)
        {
            if (options.Chase)
                return "Chase";
            if (options.Random)
                return "Random";
            if (options.ChainLength)
                return "ChainLength";
            if (options.ChainStore)
                return "ChainStore";
            return "Sequential";
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
            Console.WriteLine(" > HashSearch [Algorithm] [Seed] [Threshold]");
            Console.WriteLine(" > HashSearch");
            Console.WriteLine(" > HashSearch sha256 -uvnlx8");
            Console.WriteLine(" > HashSearch --algorithm=SHA1 --seed=0x9bc30f485ad7 --threshold=5 --unmanaged --nonewline --lowercase");
            Console.WriteLine(" > HashSearch [? | /? | -? | -h | --help | --version]");
            Console.WriteLine();
            Console.WriteLine("Options");
            Console.WriteLine(" -v/--verbose    : Add additional output");
            Console.WriteLine(" -n/--nonewline  : Output without trailing newline");
            Console.WriteLine(" -l/--lowercase  : Output hex with lowercase (0DE3 => 0de3)");
            Console.WriteLine(" -x/--omit0x     : Omit 0x prefix from hex output");
            Console.WriteLine(" -u/--unmanaged  : Use unmanaged hash algorithm, if available");
            Console.WriteLine(" -c/--color      : Disable colored output");
            Console.WriteLine(" -b/--byte       : Use byte similarity (not bit)");
            Console.WriteLine(" -d/--database   : Enable Database Interaction");
            Console.WriteLine(" -e/--chase      : Search in Chase mode");
            Console.WriteLine(" -r/--random     : Search in Random mode");
            Console.WriteLine(" -g/--chainlength: Search in ChainLength mode");
            Console.WriteLine(" -s/--chainstore : Search in ChainStore mode");
            Console.WriteLine(" -m/--maxchain   : Maximum Chain Length");
            Console.WriteLine(" -f/--final      : Final Value (optional)");
            Console.WriteLine();
            Console.Write("Supported Algorithms: MD5, SHA1, SHA256, SHA384, SHA512, RIPEMD");
        }

        public static void InitializeUpdateTimer()
        {
            UpdateTimer = new Timer(AppSettings.UpdateInterval.TotalMilliseconds);
            UpdateTimer.Elapsed += SetUpdateNeeded;
            UpdateTimer.AutoReset = true;
            UpdateTimer.Enabled = true;
        }

        public static void PauseUpdateTimer()
        {
            if (UpdateTimer != null)
                UpdateTimer.Enabled = false;
        }

        public static void ResumeUpdateTimer()
        {
            if (UpdateTimer != null)
                UpdateTimer.Enabled = true;
        }

        public static void CancelUpdateTimer()
        {
            if (UpdateTimer != null)
                UpdateTimer.Dispose();
        }

        private static void SetUpdateNeeded(object source, ElapsedEventArgs e)
        {
            UpdateNeeded = true;
        }
    }
}
