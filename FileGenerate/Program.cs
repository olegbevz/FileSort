using System;
using System.IO;
using System.Linq;
using System.Threading;
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
                var fileName = options.FileName;

                if (options.Parts == 1)
                {
                    FilePartGenerate(options.FileName, 0, totalSize, options);
                }
                else
                {
                    var partRatios = Enumerable.Range(1, options.Parts).Select(x => Math.Pow(2, x)).ToArray();
                    var targetDirectory = Path.GetDirectoryName(fileName);
                    var fileExtension = Path.GetExtension(fileName);
                    var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
                    var partNames = partRatios.Select(x => Path.Combine(
                        targetDirectory,
                        $"{fileNameWithoutExtension}_part{x}{fileExtension}")).ToArray();
                    var ratioSize = totalSize / partRatios.Sum();
                    var partSizes = partRatios.Select(x => (long)(x * ratioSize)).ToArray();
                    var sizeDefference = partSizes.Sum() - totalSize;
                    partSizes[0] += sizeDefference;

                    var writeTasks = new Task[options.Parts];
                    long currentPosition = 0;
                    for (int i = 0; i < options.Parts; i++)
                    {
                        var partName = i == options.Parts - 1 ? fileName : partNames[i];
                        var partSize = partSizes[i];
                        writeTasks[i] = Task.Run(() => FilePartGenerate(partName, currentPosition, partSize, options));     
                        currentPosition += partSize;

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
