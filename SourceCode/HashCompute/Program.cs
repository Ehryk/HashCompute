using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;

namespace HashCompute
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
        public static bool FileMode = false;

        public static int Main(string[] args)
        {
            int retCode = FAILURE_UNSPECIFIED;

            try
            {
                string stdin = GetStdInput();

                var options = new Options();
                if (CommandLine.Parser.Default.ParseArguments(args, options))
                {
                    Verbose = options.Verbose;
                    Managed = !options.Unmanaged;
                    UpperCase = !options.LowerCase;
                    Color = !options.NoColor;
                    Omit0x = options.Omit0x;

                    string input = stdin ?? options.Input;
                    string algorithm = options.Algorithm;
                    if (!String.IsNullOrEmpty(stdin) && !String.IsNullOrEmpty(options.Input))
                    {
                        //Both Provided; is args[0] (Input) an algorithm?
                        try
                        {
                            Hashes.GetHashAlgorithm(options.Input, !options.Unmanaged);
                            algorithm = options.Input;
                        }
                        catch (NotSupportedException ex) { var message = ex.Message; }
                    }
                    HashAlgorithm ha = Hashes.GetHashAlgorithm(algorithm, !options.Unmanaged);
                    string encoding = options.Encoding;
                    if (encoding == Options.Default.Encoding)
                    {
                        if (!String.IsNullOrEmpty(stdin) && !String.IsNullOrEmpty(options.Input))
                        {
                            //Both Provided; is args[0] (Input) an encoding?
                            try
                            {
                                Encodings.GetEncoding(options.Input, !options.BigEndian);
                                encoding = options.Input;
                            }
                            catch (NotSupportedException) { }
                        }
                        //Is args[1] (Algorithm) an encoding?
                        if (algorithm != Options.Default.Algorithm)
                        {
                            try
                            {
                                Encodings.GetEncoding(options.Algorithm, !options.BigEndian);
                                encoding = options.Algorithm;
                            }
                            catch (NotSupportedException) { }
                        }
                    }
                    Encoding enc = Encodings.GetEncoding(encoding, !options.BigEndian);
                    options.Algorithm = algorithm;
                    options.Encoding = encoding;

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
                    else if (options.RickRoll)
                    {
                        //Easter Egg!
                        Console.Write("Rick Roll'D!");
                        Process.Start("http://pause.ly/11");

                        retCode = SUCCESS;
                    }
                    else if (String.IsNullOrEmpty(input))
                    {
                        //No Input Provided - nothing to do
                        if (Color)
                            Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("No input provided.");
                        Console.ResetColor();

                        ShowHelp();

                        retCode = FAILURE_NO_INPUT;
                    }
                    else if (options.FileMode || options.TextMode)
                    {
                        //Interpret input as file(s)
                        string[] filePaths = input.Split(new[] {"\r\n", "\n", ";", ","}, StringSplitOptions.RemoveEmptyEntries);
                        int index = 0;
                        int elements = filePaths.Length;

                        foreach (var filePath in filePaths)
                        {
                            //File Input
                            try
                            {
                                if (File.Exists(filePath))
                                {
                                    byte[] hash;

                                    if (options.TextMode)
                                    {
                                        string fileContents = File.ReadAllText(filePath, enc);
                                        hash = Hashes.GetHash(fileContents, ha, enc);
                                    }
                                    else
                                    {
                                        byte[] fileContents = File.ReadAllBytes(filePath);
                                        hash = Hashes.GetHash(fileContents, ha);
                                    }

                                    if (Verbose)
                                    {
                                        Console.WriteLine("Input> {0}", Path.GetFullPath(filePath));
                                        Console.WriteLine("Hash : {0}", ha.GetType().Name);
                                        if (options.ShowUTF8)
                                            Console.WriteLine("UTF8 : {0}", Encoding.UTF8.GetString(hash).Replace("\r", "").Replace("\n", ""));
                                        else if (options.Encoding != Options.Default.Encoding)
                                            Console.WriteLine("{0} : {1}", enc.EncodingName, enc.GetString(hash).Replace("\r", "").Replace("\n", ""));
                                        if (Color)
                                            Console.ForegroundColor = ConsoleColor.White;
                                        Console.Write("Hex  : {0}{1}", Omit0x ? "" : "0x", hash.GetString(UpperCase));
                                    }
                                    else
                                    {
                                        if (Color)
                                            Console.ForegroundColor = ConsoleColor.White;
                                        if (options.ShowUTF8)
                                            Console.Write("{0} {1}{2}", Encoding.UTF8.GetString(hash).Replace("\r", "").Replace("\n", ""), options.TextMode ? " " : "*", filePath);
                                        if (options.HashOnly)
                                            Console.Write("{0}{1}", Omit0x ? "" : "0x", hash.GetString(UpperCase));
                                        else
                                            Console.Write("{0}{1} {2}{3}", Omit0x ? "" : "0x", hash.GetString(UpperCase), options.TextMode ? " " : "*", filePath);
                                    }
                                }
                                else
                                {
                                    if (Color)
                                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                                    Console.Write("File {0} does not exist or is inaccessible.", filePath);

                                    if (index != elements - 1)
                                        Console.WriteLine();
                                }
                            }
                            catch (Exception ex)
                            {
                                if (Color)
                                    Console.ForegroundColor = ConsoleColor.Red;
                                Console.Write("{0}: {1}", ex.GetType().Name, ex.Message);
                            }

                            if (index != elements - 1)
                                Console.WriteLine();

                            index++;
                            Console.ResetColor();
                        }

                        retCode = SUCCESS;
                    }
                    else
                    {
                        byte[] hash;
                        if (options.HexMode)
                        {
                            //Interpret the input as a Hex String
                            hash = Hashes.GetHash(input.GetHexBytes(), ha);
                        }
                        else
                        {
                            //Interpret the input as a String
                            hash = Hashes.GetHash(input, ha, enc);
                        }

                        if (options.Verbose)
                        {
                            Console.WriteLine("Input: {0}{1}", input, options.HexMode ? " (hex)" : "");
                            Console.WriteLine("Hash : {0}", ha.GetType().Name);
                            if (options.ShowUTF8)
                                Console.WriteLine("UTF8 : {0}", Encoding.UTF8.GetString(hash).Replace("\r", "").Replace("\n", ""));
                            else if (options.Encoding != Options.Default.Encoding)
                                Console.WriteLine("{0} : {1}", enc.EncodingName, enc.GetString(hash).Replace("\r", "").Replace("\n", ""));
                            if (Color)
                                Console.ForegroundColor = ConsoleColor.White;
                            Console.Write("Hex  : {0}{1}", Omit0x ? "" : "0x", hash.GetString(UpperCase));
                        }
                        else
                        {
                            if (Color)
                                Console.ForegroundColor = ConsoleColor.White;
                            if (options.ShowUTF8)
                                Console.Write("{0}", Encoding.UTF8.GetString(hash).Replace("\r", "").Replace("\n", ""));
                            else
                                Console.Write("{0}{1}", Omit0x ? "" : "0x", hash.GetString(UpperCase));
                        }

                        retCode = SUCCESS;
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

        public static string GetStdInput()
        {
            if (Console.IsInputRedirected)
                return Console.In.ReadToEnd().Trim();

            return null;
        }

        public static void ShowHelp()
        {
            Console.WriteLine();
            if (Color)
                Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(" === HashCompute v{0}.{1} ===", ApplicationInfo.Version.Major, ApplicationInfo.Version.Minor);
            Console.ResetColor();
            Console.WriteLine("Computes the hash of input from the terminal (stdin or first argument)");
            Console.WriteLine("Multiple algorithms available, defaults to SHA512.");
            Console.WriteLine();
            Console.WriteLine("Usage and Examples: ");
            Console.WriteLine(" > HashCompute (Input) [Algorithm] [Options]");
            Console.WriteLine(" > HashCompute test");
            Console.WriteLine(" > HashCompute test sha256 -uvnlx8");
            Console.WriteLine(" > echo|set /P=test | HashCompute");
            Console.WriteLine(" > HashCompute.exe \"HashCompute.exe,not_exist;HashCompute.exe\" md5 -ft");
            Console.WriteLine(" > dir /b | HashCompute.exe --algorithm=md5 --file -xlv");
            Console.WriteLine(" > HashCompute -itest -aRIPEMD --verbose --color --utf8");
            Console.WriteLine(" > HashCompute --input=test --algorithm=SHA1 --unmanaged --nonewline --lowercase");
            Console.WriteLine(" > HashCompute [? | /? | -? | -h | --help | --version]");
            Console.WriteLine();
            Console.WriteLine("Options");
            Console.WriteLine(" -f/--file       : Interpret input as a list of file(s) and hash as binary");
            Console.WriteLine(" -t/--text       : Interpret input as a list of file(s) and hash as text (w/-e)");
            Console.WriteLine(" -e/--encoding   : Encoding to use (default: SystemDefault)");
            Console.WriteLine(" -d/--hex        : Interpret input as a hexadecimal string");
            Console.WriteLine(" -v/--verbose    : Add additional output");
            Console.WriteLine(" -n/--nonewline  : Output without trailing newline");
            Console.WriteLine(" -l/--lowercase  : Output hex with lowercase (0DE3 => 0de3)");
            Console.WriteLine(" -b/--big-endian : Use big-endian version of encoding (multi-byte encodings only)");
            Console.WriteLine(" -x/--omit0x     : Omit 0x prefix from hex output");
            Console.WriteLine(" -s/--hash-only  : Omit filenames from hash output");
            Console.WriteLine(" -u/--unmanaged  : Use unmanaged hash algorithm, if available");
            Console.WriteLine(" -8/--utf8       : Print the UTF-8 string of the hash");
            Console.WriteLine(" -c/--color      : Disable colored output");
            Console.WriteLine();
            Console.WriteLine("Supported Algorithms: MD5, SHA1, SHA256, SHA384, SHA512, RIPEMD");
            Console.Write("Supported Encodings: Default, ASCII, UTF7, UTF8, UTF16/Unicode (-b), UTF32 (-b)");
        }
    }
}
