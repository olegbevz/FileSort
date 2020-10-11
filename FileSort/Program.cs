using CommandLine;
using FileSort.Core;
using System;

namespace FileSort
{
    partial class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<FileSortOptions>(args)
                .WithParsed(HandleFileSort);
        }

        private static void HandleFileSort(FileSortOptions options)
        {
            try
            {
                var fileBufferSize = (int)MemorySize.Parse(options.FileBuffer).GetTotalBytes();
                var memoryBufferSize = MemorySize.Parse(options.MemoryBuffer).GetTotalBytes();
                var fileSort = new FileSort(fileBufferSize, memoryBufferSize);
                fileSort.Sort(options.InputFileName, options.OutputFileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
