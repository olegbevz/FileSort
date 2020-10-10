using CommandLine;

namespace FileGenerate
{
    public class FileGenerateOptions
    {
        [Value(0, Required = true, HelpText = "Target file path")]
        public string FileName { get; set; }
        [Option('s', "size", Required = false, HelpText = "Target file size", Default = (long)10 * 1024 * 1024)]
        public string FileSize { get; set; }
        [Option('g', "generator", Required = false, HelpText = "Random string generation algorithm (sequence, random, bogus, autofixture)", Default = StringFactory.Random)]
        public StringFactory StringFactory { get; set; }
        [Option("file-buffer", Required = false, Default = "1MB", HelpText = "Size of FileStream internal buffer")]
        public string FileBuffer { get; set; }
    }
}
