using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using CommandLine;

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
                using (var fileStream = File.Open(options.OutputFileName, FileMode.Create))
                using (var streamWriter = new StreamWriter(fileStream))
                {
                    var randomStringSource = new RandomStringEnumerable(
                            MemorySize.Parse(options.FileSize).GetTotalBytes(),
                            streamWriter.Encoding,
                            streamWriter.NewLine,
                            CreateStringFactory(options.StringFactory));

                    var writeTask = Task.CompletedTask;

                    var stopWatch = Stopwatch.StartNew();
                    foreach (var line in randomStringSource)
                        streamWriter.WriteLine(line);

                    var elapsed = stopWatch.Elapsed;
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
