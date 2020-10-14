using System;
using System.IO;
using CommandLine;
using FileSort.Core;

namespace FileCheck
{
    class Program
    {
        static int Main(string[] args)
        {
            int checkResult = 0;

            Parser.Default.ParseArguments<FileCheckOptions>(args)
                .WithParsed(options => checkResult = HandleFileCheck(options));

            return checkResult;
        }

        private static int HandleFileCheck(FileCheckOptions options)
        {
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
                                Console.WriteLine($"File '{options.FileName}' is not properly sorted.");
                                Console.WriteLine($"Line '{fileLine}' should be before line '{previousLine}'.");
                                return 1;
                            }
                        }

                        previousLine = fileLine;
                        firstLineReaden = true;
                    }
                }

                Console.WriteLine($"File '{options.FileName}' has been successfully checked for lines order.");
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return -1;
            }
        }
    }
}
