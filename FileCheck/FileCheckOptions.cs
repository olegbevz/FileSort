using CommandLine;

namespace FileCheck
{
    public class FileCheckOptions
    {
        [Value(0, Required = true, HelpText = "Path to the sorted file that need to be checked")]
        public string FileName { get; set; }
    }
}
