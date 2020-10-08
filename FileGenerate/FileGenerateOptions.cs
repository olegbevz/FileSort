using CommandLine;

namespace FileGenerate
{
    public class FileGenerateOptions
    {
        [Value(0, Required = true, HelpText = "Target file path")]
        public string OutputFileName { get; set; }
        [Option('s', "size", Required = false, HelpText = "Target file size", Default = (long)10 * 1024 * 1024)]
        public long FileSize { get; set; }
    }
}
