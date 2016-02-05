using CommandLine;
using CommandLine.Text;

namespace HashCompute
{
    public class Options
    {
        [ValueOption(0)]
        [Option('i', "input", DefaultValue = null, Required = false, HelpText = "Input")]
        public string Input { get; set; }

        [ValueOption(1)]
        [Option('a', "algorithm", DefaultValue = "Default", HelpText = "Hash Algorithm")]
        public string Algorithm { get; set; }

        [ValueOption(2)]
        [Option('e', "encoding", DefaultValue = "Default", HelpText = "Encoding")]
        public string Encoding { get; set; }

        [Option('h', "help", DefaultValue = false, HelpText = "Show Help and Usage")]
        public bool Help { get; set; }
        [Option('v', "verbose", DefaultValue = false, HelpText = "Additional output")]
        public bool Verbose { get; set; }
        [Option("version", DefaultValue = false, HelpText = "Print Version and Exit")]
        public bool Version { get; set; }
        [Option('f', "file", DefaultValue = false, HelpText = "Interpret input as filename(s) and hash as binary")]
        public bool FileMode { get; set; }
        [Option('t', "text", DefaultValue = false, HelpText = "Interpret input as filename(s) and hash as text")]
        public bool TextMode { get; set; }
        [Option('d', "decimal", DefaultValue = false, HelpText = "Interpret input as a string of decimals (supports hex, decimal, octal, and binary)")]
        public bool DecimalMode { get; set; }
        [Option('u', "unmanaged", DefaultValue = false, HelpText = "Use Unmanaged Hash Algorithms")]
        public bool Unmanaged { get; set; }
        [Option('b', "big-endian", DefaultValue = false, HelpText = "Use Big Endian Encoding")]
        public bool BigEndian { get; set; }
        [Option('n', "nonewline", DefaultValue = false, HelpText = "Do not append last new line to output")]
        public bool NoNewLine { get; set; }
        [Option('l', "lowercase", DefaultValue = false, HelpText = "Display hex in lower case")]
        public bool LowerCase { get; set; }
        [Option('c', "color", DefaultValue = false, HelpText = "Disable colored output")]
        public bool NoColor { get; set; }
        [Option('8', "utf8", DefaultValue = false, HelpText = "Show UTF-8 Representation")]
        public bool ShowUTF8 { get; set; }
        [Option('x', "omit0x", DefaultValue = false, HelpText = "Omit 0x Hex Specifier")]
        public bool Omit0x { get; set; }
        [Option('s', "hash", DefaultValue = false, HelpText = "Omit filenames from hash output")]
        public bool HashOnly { get; set; }
        [Option('r', "rickroll", DefaultValue = false, HelpText = "Undocumented Feature!!!")]
        public bool RickRoll { get; set; }

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
                CommandLine.Parser.Default.ParseArguments(new string[] { }, defaults);
                return defaults;
            }
        }
    }
}
