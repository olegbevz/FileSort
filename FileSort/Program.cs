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
                var fileSort = new FileSort((int)MemorySize.Parse(options.FileBuffer).GetTotalBytes());
                fileSort.Sort(options.InputFileName, options.OutputFileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
