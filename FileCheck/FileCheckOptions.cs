using CommandLine;

namespace FileCheck
{
    public class FileCheckOptions
    {
        [Value(0, Required = true, HelpText = "Path to the sorted file that need to be checked")]
        public string FileName { get; set; }
        [Option("file-buffer", Required = false, Default = "1MB", HelpText = "Size of FileStream internal buffer")]
        public string FileBuffer { get; set; }
        [Option("check-format", Required = false)]
        public bool OnlyCheckFormat { get; set; }
    }
}
