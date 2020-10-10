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

        [TestCase("0mb", TestName = "ShouldSort0MBRandomFile")]
        [TestCase("1bytes", TestName = "ShouldSort1BytesRandomFile")]
        [TestCase("10bytes", TestName = "ShouldSort10BytesRandomFile")]
        [TestCase("100bytes", TestName = "ShouldSort100BytesRandomFile")]
        [TestCase("1kb", TestName = "ShouldSort1KBRandomFile")]
        [TestCase("10KB", TestName = "ShouldSort10KBRandomFile")]
        [TestCase("100KB", TestName = "ShouldSort100KBRandomFile")]        
        [TestCase("1MB", TestName = "ShouldSort1MBRandomFile")]
        [TestCase("10mb", TestName = "ShouldSort10MBRandomFile")]
        [TestCase("100MB", TestName = "ShouldSort100MBRandomFile")]
        [TestCase("1GB", TestName = "ShouldSort1GBRandomFile", IgnoreReason = "Test is too long. Should be run manually")]
        [TestCase("10GB", TestName = "ShouldSort10GBRandomFile", IgnoreReason = "Test is too long. Should be run manually")]
        [TestCase("100GB", TestName = "ShouldSort100GBRandomFile", IgnoreReason = "Test is too long. Should be run manually")]
        public void ShouldSortGeneratedFile(string fileSize)
        {
            var inputFileName = Path.Combine("Content", fileSize + ".txt");
            var outputFileName = Path.Combine("Content", fileSize + "_sorted.txt");

            var elapsedTime = TimeSpan.Zero;
            var stopwatch = Stopwatch.StartNew();

            RunProcess(
                "FileGenerate.exe",
                $"{inputFileName} -s {fileSize}");

            elapsedTime = stopwatch.Elapsed;
            stopwatch.Restart();
            Console.WriteLine($"File '{inputFileName}' was generated in {elapsedTime}.");

            stopwatch.Start();

            RunProcess(
                "FileSort.exe",
                $"{inputFileName} {outputFileName}");

            elapsedTime = stopwatch.Elapsed;
            stopwatch.Restart();
            Console.WriteLine($"File '{inputFileName}' was sorted in {elapsedTime}.");

            stopwatch.Start();

            var checkProcess = RunProcess(
                "FileCheck.exe",
                $"{outputFileName}");

            elapsedTime = stopwatch.Elapsed;
            stopwatch.Stop();
            Console.WriteLine($"File '{outputFileName}' was checked in {elapsedTime}.");

            Assert.AreEqual(0, checkProcess.ExitCode, $"File '{inputFileName}' was not properly sorted or check failed.");
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
