﻿using System;
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
                using (var fileStream = File.Open(options.FileName, FileMode.Open))
                using (var streamReader = new StreamReader(fileStream))
                {
                    string previousLine = null;

                    while (!streamReader.EndOfStream)
                    {
                        var currentLine = streamReader.ReadLine();

                        if (previousLine != null)
                        {
                            if (FileEntry.Parse(previousLine).CompareTo(FileEntry.Parse(currentLine)) > 0)
                            {
                                Console.WriteLine($"File '{options.FileName}' is not properly sorted.");
                                Console.WriteLine($"Line '{currentLine}' should be before line '{previousLine}'.");
                                return 1;
                            }
                        }

                        previousLine = currentLine;
                    }
                }

                Console.WriteLine($"File {options.FileName} has been successfully checked for lines order.");
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