using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace HashCompute
{
    class Program
    {
        public static bool Verbose = false;
        public static bool Managed = true;
        public static bool UpperCase = false;
        public static bool Color = true;
        public static bool Omit0x = false;
        public static bool FileMode = false;

        public static void Main(string[] args)
        {
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
                        //Both Provided; is arg[0] (Input) an algorithm?
                        try
                        {
                            Hash.GetHashAlgorithm(options.Input, !options.Unmanaged);
                            algorithm = options.Input;
                        }
                        catch (NotSupportedException ex) { }
                    }
                    HashAlgorithm ha = Hash.GetHashAlgorithm(algorithm, !options.Unmanaged);

                    if (options.Version)
                        Console.Write("{0} v{1}.{2}.{3}.{4} ({5})", ApplicationInfo.Title, ApplicationInfo.Version.Major, ApplicationInfo.Version.Minor, ApplicationInfo.Version.Build, ApplicationInfo.Version.Revision, ApplicationInfo.CopyrightHolder);
                    else if (options.Help || args.Any(a => a.Equals("?") || a.Equals("-?") || a.Equals("/?") || a.Equals("--?")))
                        ShowHelp();
                    else if (options.RickRoll)
                    {
                        Console.Write("Rick Roll'D!");
                        Process.Start("http://pause.ly/11");
                    }
                    else if (String.IsNullOrEmpty(input))
                    {
                        if (Color)
                            Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("No input provided.");
                        Console.ResetColor();

                        ShowHelp();
                    }
                    else if (options.FileMode)
                    {
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
                                    byte[] fileContents = File.ReadAllBytes(filePath);
                                    byte[] hash = Hash.GetHash(fileContents, ha);

                                    if (Verbose)
                                    {
                                        Console.WriteLine("Input> {0}", Path.GetFullPath(filePath));
                                        Console.WriteLine("Hash : {0}", ha.GetType().Name);
                                        if (options.ShowUTF8)
                                            Console.WriteLine("UTF8 : {0}", Encoding.UTF8.GetString(hash).Replace("\r", "").Replace("\n", ""));
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

                                    if (index != elements - 1)
                                        Console.WriteLine();
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

                            index++;
                            Console.ResetColor();
                        }
                    }
                    else
                    {
                        //String Input
                        byte[] hash = Hash.GetHash(input, ha);

                        if (options.Verbose)
                        {
                            Console.WriteLine("Input: {0}", input);
                            Console.WriteLine("Hash : {0}", ha.GetType().Name);
                            if (options.ShowUTF8)
                                Console.WriteLine("UTF8 : {0}", Encoding.UTF8.GetString(hash).Replace("\r", "").Replace("\n", ""));
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
                    }

                    if (!options.NoNewLine)
                        Console.WriteLine();
                }
                else
                {
                    if (args.Length > 0 && !args.Any(a => a.Equals("?") || a.Equals("-?") || a.Equals("/?") || a.Equals("--?")))
                    {
                        if (Color)
                            Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Unknown Arguments: {0}", String.Join(" ", args));
                        Console.ResetColor();
                    }

                    ShowHelp();

                    Console.WriteLine();
                }
            }
            catch (Exception ex)
            {
                if (Color)
                    Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine();
                Console.WriteLine("{0}: {1}", ex.GetType().Name, ex.Message);
            }

            Console.ResetColor();
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
            Console.WriteLine(" - HashCompute (Input) [Algorithm] [Options]");
            Console.WriteLine(" - HashCompute test");
            Console.WriteLine(" - HashCompute test sha256 -uvnlx8");
            Console.WriteLine(" - echo|set /P=test | HashCompute");
            Console.WriteLine(" - HashCompute.exe \"HashCompute.exe,test;HashCompute.exe\" md5 -f");
            Console.WriteLine(" - HashCompute -itest -aRIPEMD --verbose --color --utf8");
            Console.WriteLine(" - HashCompute --input=test --algorithm=SHA1 --unmanaged --nonewline --lowercase");
            Console.WriteLine(" - HashCompute [? | /? | -? | -h | --help | --version]");
            Console.WriteLine();
            Console.WriteLine("Options");
            Console.WriteLine(" - -f/--file      : Interpret input as a (list of) file(s)");
            Console.WriteLine(" - -v/--verbose   : Add additional output");
            Console.WriteLine(" - -n/--nonewline : Output without trailing newline");
            Console.WriteLine(" - -l/--lowercase : Output hex with lowercase");
            Console.WriteLine(" - -x/--omit0x    : Omit 0x prefix from hex output");
            Console.WriteLine(" - -u/--unmanaged : Use unmanaged algorithm, if available");
            Console.WriteLine(" - -8/--utf8      : Print the UTF-8 string of the hash");
            Console.WriteLine(" - -c/--color     : Disable colored output");
            Console.WriteLine();
            Console.Write("Supported Algorithms: MD5, SHA1, SHA256, SHA384, SHA512, RIPEMD");
        }
    }
}
