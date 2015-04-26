using CommandLine;
using CommandLine.Text;

namespace HashSearch
{
    public class Options
    {
        [ValueOption(0)]
        [Option('a', "algorithm", DefaultValue = "MD5", HelpText = "Hash Algorithm")]
        public string Algorithm { get; set; }

        [ValueOption(1)]
        [Option('s', "seed", DefaultValue = "0x00", Required = false, HelpText = "Seed (Starting) Value")]
        public string Seed { get; set; }

        [ValueOption(2)]
        [Option('t', "threshold", DefaultValue = 1, Required = false, HelpText = "Similarity Threshold")]
        public int Threshold { get; set; }

        [Option('h', "help", DefaultValue = false, HelpText = "Show Help and Usage")]
        public bool Help { get; set; }
        [Option('v', "verbose", DefaultValue = false, HelpText = "Additional output")]
        public bool Verbose { get; set; }
        [Option("version", DefaultValue = false, HelpText = "Print Version and Exit")]
        public bool Version { get; set; }
        [Option('u', "unmanaged", DefaultValue = false, HelpText = "Use Unmanaged Hash Algorithms")]
        public bool Unmanaged { get; set; }
        [Option('n', "nonewline", DefaultValue = false, HelpText = "Do not append last new line to output")]
        public bool NoNewLine { get; set; }
        [Option('l', "lowercase", DefaultValue = false, HelpText = "Display hex in lower case")]
        public bool LowerCase { get; set; }
        [Option('c', "color", DefaultValue = false, HelpText = "Disable colored output")]
        public bool NoColor { get; set; }
        [Option('x', "omit0x", DefaultValue = false, HelpText = "Omit 0x Hex Specifier")]
        public bool Omit0x { get; set; }
        [Option('b', "byte", DefaultValue = false, HelpText = "Use byte similarity instead of bit similarity")]
        public bool ByteSimilarity { get; set; }
        [Option('d', "database", DefaultValue = false, HelpText = "Load results to a database")]
        public bool Database { get; set; }

        [Option('e', "chase", DefaultValue = false, HelpText = "Chase Mode")]
        public bool Chase { get; set; }
        [Option('r', "random", DefaultValue = false, HelpText = "Random Mode")]
        public bool Random { get; set; }

        //[HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this, (current) => { });
            //return HelpText.AutoBuild(this,
            //  (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }

        public static Options Default
        {
            get
            {
                Options defaults = new Options();
                Parser.Default.ParseArguments(new string[] { }, defaults);
                return defaults;
            }
        }
    }
}
