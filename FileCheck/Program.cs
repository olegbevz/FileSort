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
                bool compareFileLines = !options.OnlyCheckFormat;

                using (var fileStream = FileWithBuffer.OpenRead(options.FileName, fileBufferSize))
                {
                    foreach (var fileLine in new FileLineReader(fileStream))
                    {                        
                        bool firstLineReaden = false;
                        FileLine previousLine = FileLine.None;

                        if (compareFileLines && firstLineReaden)
                        {
                            if (previousLine.CompareTo(fileLine) > 0)
                            {
                                _logger.Warn($"File '{options.FileName}' is not properly sorted.");
                                _logger.Warn($"Line '{fileLine}' should be before line '{previousLine}'.");
                                return 1;
                            }
                        }

                        previousLine = fileLine;
                        firstLineReaden = true;
                    }
                }

                _logger.Info($"File '{options.FileName}' has been successfully checked for lines order.");
                return 0;
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to parse file '{options.FileName}' for sorted lines." , ex);
                return -1;
            }
        }
    }
}
