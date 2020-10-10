using NUnit.Framework;
using System;
using System.Diagnostics;
using System.IO;

namespace FileSort.IntegrationTests
{
    [TestFixture]
    public class FileSortTests
    {
        [TestCase("simple.txt", "simple_sorted.txt", "simple_expected.txt", TestName = "ShouldSortSimpleFile")]
        [TestCase("oneline.txt", "oneline_sorted.txt", "oneline_expected.txt", TestName = "ShouldSortOneLineFile")]
        [TestCase("empty.txt", "empty_sorted.txt", "empty_expected.txt", TestName = "ShouldSortEmptyFile")]
        public void ShouldSortFileFromContent(string inputFileName, string outputFileName, string expectedFileName)
        {
            var process = RunProcess(
                "FileSort.exe", 
                $"{Path.Combine("Content", inputFileName)} {Path.Combine("Content", outputFileName)}");

            FileAssert.AreEqual(
                Path.Combine(process.StartInfo.WorkingDirectory, "Content", expectedFileName),
                Path.Combine(process.StartInfo.WorkingDirectory, "Content", outputFileName));
        }

        [TestCase("10mb", TestName = "ShouldSort10MBRandomFile")]
        public void ShouldSortGeneratedFile(string fileSize)
        {
            var inputFileName = Path.Combine("Content", fileSize + ".txt");
            var outputFileName = Path.Combine("Content", fileSize + "_sorted.txt");

            RunProcess(
                "FileGenerate.exe",
                $"{inputFileName} -s {fileSize}");

            RunProcess(
                "FileSort.exe",
                $"{inputFileName} {outputFileName}");
        }

        private Process RunProcess(string executable, string arguments)
        {
            var currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var executablePath = Path.Combine(currentDirectory, executable);

            var startInfo = new ProcessStartInfo
            {
                FileName = executablePath,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = currentDirectory
            };

            var process = new Process { StartInfo = startInfo };
            process.Start();

            while (!process.StandardOutput.EndOfStream)
            {
                var line = process.StandardOutput.ReadLine();
                Console.WriteLine(line);
            }

            process.WaitForExit();

            return process;
        }
    }
}
