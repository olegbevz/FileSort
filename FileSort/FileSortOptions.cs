using CommandLine;

namespace FileSort
{
    public class FileSortOptions
    {
        [Value(0, Required = true, HelpText = "Path to the source file")]
        public string InputFileName { get; set; }
        [Value(1, Required = true, HelpText = "Path to the target file")]
        public string OutputFileName { get; set; }
        [Option("file-buffer", Required = false, Default = "1MB", HelpText = "Size of FileStream internal buffer")]
        public string FileBuffer { get; set; }
        [Option("memory-buffer", Required = false, Default = "100MB", HelpText = "Size of memory buffer")]
        public string MemoryBuffer { get; set; }
    }
}
