using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;

namespace HashCompute
{
    public class Options
    {
        [ValueOption(0)]
        public string Input { get; set; }

        [ValueOption(1)]
        [Option('a', "algorithm", DefaultValue = "Default", HelpText = "Hash Algorithm")]
        public string Algorithm { get; set; }

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

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this,
              (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }
}
