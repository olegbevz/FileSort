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

                using (var fileStream = FileWithBuffer.OpenRead(options.FileName, fileBufferSize))
                {
                    using (var streamReader = new StreamReader(fileStream))
                    {
                        bool compareFileLines = !options.OnlyCheckFormat;
                        bool firstLineReaden = false;
                        FileLine previousLine = FileLine.None;                        

                        while (!streamReader.EndOfStream)
                        {
                            var line = streamReader.ReadLine();
                            if (string.IsNullOrEmpty(line) && streamReader.EndOfStream)
                                continue;
                            var currentLine = FileLine.Parse(line);

                            if (compareFileLines && firstLineReaden)
                            {
                                if (previousLine.CompareTo(currentLine) > 0)
                                {
                                    Console.WriteLine($"File '{options.FileName}' is not properly sorted.");
                                    Console.WriteLine($"Line '{currentLine}' should be before line '{previousLine}'.");
                                    return 1;
                                }
                            }

                            previousLine = currentLine;
                            firstLineReaden = true;
                        }
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
