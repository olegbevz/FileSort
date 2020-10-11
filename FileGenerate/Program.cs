using System;
using System.IO;
using System.Threading.Tasks;
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
                var totalSize = MemorySize.Parse(options.FileSize).GetTotalBytes();

                if (options.Parts == 1)
                {
                    FilePartGenerate(options.FileName, 0, totalSize, options);
                }
                else
                {
                    var partSize = totalSize / options.Parts;
                    var writeTasks = new Task[options.Parts];
                    for (int i = 0; i < options.Parts; i++)
                    {
                        long startPosition = i * partSize; 
                        writeTasks[i] = Task.Run(() => FilePartGenerate(options.FileName, startPosition, partSize, options));
                        
                    }
                    Task.WaitAll(writeTasks);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static void FilePartGenerate(string fileName, long startPosition, long partSize, FileGenerateOptions options)
        {
            var fileBufferSize = (int)MemorySize.Parse(options.FileBuffer).GetTotalBytes();

            using (var fileStream = FileWithBuffer.OpenWrite(fileName, fileBufferSize))
            {
                fileStream.Seek(startPosition, SeekOrigin.Begin);

                using (var streamWriter = new StreamWriter(fileStream))
                {
                    var randomStringSource = new RandomStringEnumerable(
                            partSize,
                            streamWriter.Encoding,
                            streamWriter.NewLine,
                            CreateStringFactory(options.StringFactory));

                    foreach (var line in randomStringSource)
                        streamWriter.WriteLine(line);
                }
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
