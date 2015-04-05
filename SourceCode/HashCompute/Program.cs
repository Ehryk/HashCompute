﻿using System;
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

                    string input = options.Input ?? stdin;
                    HashAlgorithm ha = GetHashAlgorithm(options.Algorithm, !options.Unmanaged);

                    if (options.Version)
                        Console.Write("HashCompute.exe v{0}.{1}", ApplicationInfo.Version.Major, ApplicationInfo.Version.Minor);
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
                        string[] filePaths = input.Split(new[] {"\r\n", "\n"}, StringSplitOptions.RemoveEmptyEntries);

                        foreach (var filePath in filePaths)
                        {
                            //File Input
                            try
                            {
                                if (File.Exists(filePath))
                                {
                                    byte[] fileContents = File.ReadAllBytes(filePath);
                                    byte[] hash = GetHash(fileContents, ha);

                                    if (Verbose)
                                    {
                                        Console.WriteLine("Input: ->{0}", Path.GetFullPath(filePath));
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
                                        Console.Write("{0}{1}", Omit0x ? "" : "0x", hash.GetString(UpperCase));
                                    }

                                    if (Array.IndexOf(filePaths, filePath) != filePaths.Length - 1)
                                        Console.WriteLine();
                                }
                                else
                                {
                                    if (Color)
                                        Console.ForegroundColor = ConsoleColor.DarkYellow;
                                    Console.Write("File {0} does not exist or is inaccessible.", filePath);

                                    if (Array.IndexOf(filePaths, filePath) != filePaths.Length - 1)
                                        Console.WriteLine();
                                }
                            }
                            catch (Exception ex)
                            {
                                if (Color)
                                    Console.ForegroundColor = ConsoleColor.Red;
                                Console.Write("{0}: {1}", ex.GetType().Name, ex.Message);
                            }
                        }
                    }
                    else
                    {
                        //String Input
                        byte[] hash = GetHash(input, options.Algorithm);

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
                        if (options.NoColor)
                            Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Unknown Arguments: {0}", String.Join(" ", args));
                        Console.ResetColor();
                    }

                    ShowHelp();
                }
            }
            catch (Exception ex)
            {
                if (Color)
                    Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("{0}: {1}", ex.GetType().Name, ex.Message);
            }

            Console.ResetColor();
        }

        public static string GetStdInput()
        {
            if (Console.IsInputRedirected)
                return Console.In.ReadToEnd().Trim();

            return null;
        }

        public static byte[] GetHash(string input, string algorithm = null)
        {
            try
            {
                HashAlgorithm ha = GetHashAlgorithm(algorithm ?? "Default", Managed);
                byte[] hash = GetHash(input, ha);
                return hash;
            }
            catch (Exception ex)
            {
                if (Color)
                    Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("{0}: {1}", ex.GetType().Name, ex.Message);
            }
            return null;
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

        public static HashAlgorithm GetHashAlgorithm(string algorithm = "Default", bool managed = true)
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
            if (Color)
                Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(" === HashCompute v{0}.{1} ===", ApplicationInfo.Version.Major, ApplicationInfo.Version.Minor);
            Console.ResetColor();
            Console.WriteLine("Computes the hash of the terminal input (as a UTF-8 String)");
            Console.WriteLine("Defaults to SHA512.");
            Console.WriteLine();
            Console.WriteLine("Usage and Examples: ");
            Console.WriteLine(" - HashCompute (Input) [Algorithm] [Options]");
            Console.WriteLine(" - HashCompute test");
            Console.WriteLine(" - echo|set /P=test | HashCompute");
            Console.WriteLine(" - HashCompute -itest -aMD5 --verbose --color");
            Console.WriteLine(" - HashCompute test SHA256 -uvnlx");
            Console.WriteLine(" - HashCompute --input=test --algorithm=SHA1 --unmanaged --nonewline --lowercase");
            Console.WriteLine(" - HashCompute [? | /? | -? | -h | --help | --version]");
            Console.WriteLine();
            Console.WriteLine("Options");
            Console.WriteLine(" - -v/--verbose   : Add additional output");
            Console.WriteLine(" - -n/--nonewline : Output without trailing newline");
            Console.WriteLine(" - -l/--lowercase : Output hex with lowercase");
            Console.WriteLine(" - -x/--omit0x    : Omit 0x prefix from hex output");
            Console.WriteLine(" - -u/--unmanaged : Use unmanaged algorithm, if available");
            Console.WriteLine(" - -c/--color     : Disable colored output");
            Console.WriteLine();
            Console.WriteLine("Supported Algorithms: MD5, SHA1, SHA256, SHA384, SHA512, RIPEMD");
        }
    }
}
