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
                var fileSize = MemorySize.Parse(options.FileSize).GetTotalBytes();
                var fileBufferSize = (int)MemorySize.Parse(options.FileBuffer).GetTotalBytes();
                var memoryBufferSize = Math.Min((int)MemorySize.Parse(options.MemoryBuffer).GetTotalBytes(), fileSize);

                var duplicateFrequency = options.Duplicates;
                var duplicateCounter = 0;

                using (var fileStream = FileWithBuffer.OpenWrite(options.FileName, fileBufferSize))
                {
                    var buffer = new byte[memoryBufferSize];
                    using (var bufferStream = new MemoryStream(buffer))
                    {
                        using (var streamWriter = new StreamWriter(bufferStream))
                        {
                            while (fileStream.Position + memoryBufferSize < fileSize)
                            {
                                var writeDuplicate = duplicateCounter > 0;

                                if (!writeDuplicate)
                                {
                                    WriteRandomStrings(streamWriter, memoryBufferSize, options.StringFactory);
                                }

                                fileStream.Write(buffer, 0, buffer.Length);

                                if (!writeDuplicate)
                                {
                                    bufferStream.Seek(0, SeekOrigin.Begin);
                                }

                                duplicateCounter++;
                                if (duplicateCounter >= duplicateFrequency)
                                    duplicateCounter = 0;
                            }
                        }
                    }

                    if (fileStream.Position < fileSize)
                    {
                        var remainedSize = fileSize - fileStream.Position;

                        using(var streamWriter = new StreamWriter(fileStream))
                        {
                            WriteRandomStrings(streamWriter, remainedSize, options.StringFactory);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static void WriteRandomStrings(
            StreamWriter streamWriter, 
            long targetSize, 
            StringFactory stringFactory)
        {
            var randomStringSource = new RandomStringEnumerable(
                targetSize,
                streamWriter.Encoding,
                streamWriter.NewLine,
                CreateStringFactory(stringFactory));

            foreach (var line in randomStringSource)
                streamWriter.WriteLine(line);
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
