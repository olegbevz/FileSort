using System;
using System.IO;
using CommandLine;
using FileSort.Core;

namespace FileGenerate
{
    class Program
    {
        static void Main(string[] args)
        {
            var parser = new Parser(cfg => cfg.CaseInsensitiveEnumValues = true);
            parser.ParseArguments<FileGenerateOptions>(args)
                .WithParsed(HandleFileGenerate);
        }

        private static void HandleFileGenerate(FileGenerateOptions options)
        {
            try
            {
                var fileBufferSize = (int)MemorySize.Parse(options.FileBuffer).GetTotalBytes();

                using (var fileStream = FileWithBuffer.OpenWrite(options.FileName, fileBufferSize))
                {
                    using (var streamWriter = new StreamWriter(fileStream))
                    {
                        var randomStringSource = new RandomStringEnumerable(
                                MemorySize.Parse(options.FileSize).GetTotalBytes(),
                                streamWriter.Encoding,
                                streamWriter.NewLine,
                                CreateStringFactory(options.StringFactory));

                        foreach (var line in randomStringSource)
                            streamWriter.WriteLine(line);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static IRandomStringFactory CreateStringFactory(StringFactory stringFactory)
        {
            switch (stringFactory)
            {
                case StringFactory.Constant: return new ConstantStringFactory("32. Cherry is the best");
                case StringFactory.Sequence: return new SequenceStringFactory();
                case StringFactory.Random: return new RandomStringFactory();
                case StringFactory.Bogus: return new BogusStringFactory();
                case StringFactory.AutoFixture: return new AutoFixtureStringFactory();
                default: throw new NotSupportedException(nameof(stringFactory));
            }
        }
    }
}
