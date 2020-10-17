using System;
using System.Reflection;
using CommandLine;
using FileSort.Core;
using log4net;
using log4net.Config;

namespace FileCheck
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

            Parser.Default.ParseArguments<FileCheckOptions>(args)
                .WithParsed(options => checkResult = HandleFileCheck(options));

            return checkResult;
        }

        private static int HandleFileCheck(FileCheckOptions options)
        {
            _logger.Info($"Starting to check file '{options.FileName}' for lines order...");

            try
            {
                var fileBufferSize = (int)MemorySize.Parse(options.FileBuffer);
                var streamBufferSize = (int)MemorySize.Parse(options.StreamBuffer);

                var fileCheck = new FileCheck(fileBufferSize, streamBufferSize, options.OnlyCheckFormat);

                var checkResult = fileCheck.Check(options.FileName);

                _logger.Info($"File '{options.FileName}' has been successfully checked for lines order.");
                return checkResult ? 0 : 1;
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to parse file '{options.FileName}' for sorted lines." , ex);
                return -1;
            }
        }
    }
}
