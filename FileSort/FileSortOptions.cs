using CommandLine;

namespace FileSort
{
    partial class Program
    {
        public class FileSortOptions
        {
            [Value(0, Required = true, HelpText = "Path to the source file")]
            public string InputFileName { get; set; }
            [Value(1, Required = true, HelpText = "Path to the target file")]
            public string OutputFileName { get; set; }
        }
    }
}
