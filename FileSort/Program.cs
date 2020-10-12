using CommandLine;
using FileSort.Core;
using System;

namespace FileSort
{
    partial class Program
    {
        static int Main(string[] args)
        {
            var exitCode = 0;

            Parser.Default.ParseArguments<FileSortOptions>(args)
                .WithParsed(options => exitCode = HandleFileSort(options));

            return exitCode;
        }

        private static int HandleFileSort(FileSortOptions options)
        {
            try
            {
                var fileBufferSize = (int)MemorySize.Parse(options.FileBuffer).GetTotalBytes();
                var memoryBufferSize = MemorySize.Parse(options.MemoryBuffer).GetTotalBytes();
                var fileSort = new FileSort(fileBufferSize, memoryBufferSize);
                fileSort.Sort(options.InputFileName, options.OutputFileName);
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 1;
            }
        }
    }
}
