using System;
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
                var memoryBufferSize = Math.Min(MemorySize.Parse(options.MemoryBuffer), fileSize);

                var fileGenerate = new FileGenerate(
                    fileBufferSize, 
                    memoryBufferSize, 
                    options.Duplicates, 
                    CreateStringFactory(options.StringFactory));

                fileGenerate.Generate(options.FileName, fileSize);

                _logger.Info($"File '{options.FileName}' has been successfully generated.");
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to generate file '{options.FileName}'.", ex);
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
