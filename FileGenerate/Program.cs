using System;
using System.IO;
using System.Reflection;
using CommandLine;
using FileSort.Core;
using log4net;
using log4net.Config;

namespace FileGenerate
{
    class Program
    {
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        static Program()
        {
            XmlConfigurator.Configure();
        }

        static int Main(string[] args)
        {
            int checkResult = 0;

            var parser = new Parser(cfg => cfg.CaseInsensitiveEnumValues = true);
            parser.ParseArguments<FileGenerateOptions>(args)
                .WithParsed(HandleFileGenerate);

            return checkResult;
        }

        private static void HandleFileGenerate(FileGenerateOptions options)
        {
            try
            {
                _logger.Info($"Starting to generate file '{options.FileName}'...");

                var fileSize = MemorySize.Parse(options.FileSize);
                var fileBufferSize = (int)MemorySize.Parse(options.FileBuffer);
                var memoryBufferSize = Math.Min((int)MemorySize.Parse(options.MemoryBuffer), fileSize);

                using (var fileStream = FileWithBuffer.OpenWrite(options.FileName, fileBufferSize))
                {
                    if (options.Duplicates > 0)
                    {
                        WriteRandomStringsWithDuplicates(
                            fileStream,
                            fileSize,
                            memoryBufferSize,
                            options.Duplicates,
                            options.StringFactory);
                    }
                    else
                    {
                        WriteRandomStringsWithoutDuplicates(
                            fileStream,
                            fileSize,
                            options.StringFactory);
                    }
                }

                _logger.Info($"File '{options.FileName}' has been successfully generated.");
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to generate file '{options.FileName}'.", ex);
            }
        }

        private static void WriteRandomStringsWithoutDuplicates(
            FileStream fileStream,
            long fileSize,
            StringFactory stringFactory)
        {
            using (var streamWriter = new StreamWriter(fileStream))
            {
                WriteRandomStrings(streamWriter, fileSize, stringFactory);
            }
        }

        private static void WriteRandomStringsWithDuplicates(
            FileStream fileStream,
            long fileSize,
            long memoryBufferSize,
            int duplicatesFrequency,
            StringFactory stringFactory)
        {
            var duplicateCounter = 0;

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
                            WriteRandomStrings(streamWriter, memoryBufferSize, stringFactory);
                        }

                        fileStream.Write(buffer, 0, buffer.Length);

                        if (!writeDuplicate)
                        {
                            bufferStream.Seek(0, SeekOrigin.Begin);
                        }

                        duplicateCounter++;
                        if (duplicateCounter >= duplicatesFrequency)
                            duplicateCounter = 0;
                    }
                }
            }

            if (fileStream.Position < fileSize)
            {
                var remainedSize = fileSize - fileStream.Position;

                using (var streamWriter = new StreamWriter(fileStream))
                {
                    WriteRandomStrings(streamWriter, remainedSize, stringFactory);
                }
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
