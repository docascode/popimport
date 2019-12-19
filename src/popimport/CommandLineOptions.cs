using CommandLine;

namespace popimport
{
    public class CommandLineOptions
    {
        [Option('f', "frameworks", Required = true, HelpText = "Path to frameworks.xml (just the folder)")]
        public string FrameworksPath
        {
            get;
            set;
        }
    }
}