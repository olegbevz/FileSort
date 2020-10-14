using CommandLine;
using FileSort.Core;
using log4net;
using log4net.Config;
using System;
using System.Reflection;

namespace FileSort
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
            var exitCode = 0;

            Parser.Default.ParseArguments<FileSortOptions>(args)
                .WithParsed(options => exitCode = HandleFileSort(options))
                .WithNotParsed(options => exitCode = 1);

            return exitCode;
        }

        private static int HandleFileSort(FileSortOptions options)
        {
            try
            {
                _logger.Info($"Starting to sort file '{options.InputFileName}'...");

                var fileBufferSize = (int)MemorySize.Parse(options.FileBuffer);
                var memoryBufferSize = MemorySize.Parse(options.MemoryBuffer);
                var fileSort = new FileSort(fileBufferSize, memoryBufferSize);
                fileSort.Sort(options.InputFileName, options.OutputFileName);
                _logger.Info($"File '{options.InputFileName}' has been successfully sorted.");
                return 0;
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to sort file '{options.InputFileName}'.", ex);
                return 1;
            }
        }
    }
}
