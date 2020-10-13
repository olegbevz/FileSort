using FileGenerate;
using FileSort.Core;
using NUnit.Framework;
using System;
using System.Diagnostics;
using System.IO;

namespace FileSort.IntegrationTests
{
    [TestFixture]
    [NonParallelizable]
    public class FileSortTests
    {
        [TestCase("simple.txt", "simple_sorted.txt", "simple_expected.txt", TestName = "ShouldSortSimpleFile")]
        [TestCase("oneline.txt", "oneline_sorted.txt", "oneline_expected.txt", TestName = "ShouldSortOneLineFile")]
        [TestCase("empty.txt", "empty_sorted.txt", "empty_expected.txt", TestName = "ShouldSortEmptyFile")]
        [TestCase("six_line.txt", "six_line_sorted.txt", "six_line_expected.txt", TestName = "ShouldSortSixLineFile")]
        [TestCase("500bytes.txt", "500bytes_sorted.txt", "500bytes_expected.txt", TestName = "ShouldSort500BytesFile")]
        public void ShouldSortFileFromContent(string inputFileName, string outputFileName, string expectedFileName)
        {
            var process = RunProcess(
                "FileSort.exe", 
                $"{Path.Combine("Content", inputFileName)} {Path.Combine("Content", outputFileName)}");

            ProcessAssert.HasZeroExitCode(process, $"File '{inputFileName}' sort failed.");

            FileAssert.AreEqual(
                Path.Combine(process.StartInfo.WorkingDirectory, "Content", expectedFileName),
                Path.Combine(process.StartInfo.WorkingDirectory, "Content", outputFileName));
        }

        [TestCase("simple.txt", "simple_sorted.txt", "simple_expected.txt", TestName = "ShouldSortSimpleFileWithoutMemory")]
        [TestCase("oneline.txt", "oneline_sorted.txt", "oneline_expected.txt", TestName = "ShouldSortOneLineFileWithoutMemory")]
        [TestCase("empty.txt", "empty_sorted.txt", "empty_expected.txt", TestName = "ShouldSortEmptyFileWithoutMemory")]
        [TestCase("four_line.txt", "four_line_sorted.txt", "four_line_expected.txt", TestName = "ShouldSortFourLineFileWithoutMemory")]
        [TestCase("six_line.txt", "six_line_sorted.txt", "six_line_expected.txt", TestName = "ShouldSortSixLineFileWithoutMemory")]
        [TestCase("eight_line.txt", "eight_line_sorted.txt", "eight_line_expected.txt", TestName = "ShouldSortEightLineFileWithoutMemory")]
        [TestCase("500bytes.txt", "500bytes_sorted.txt", "500bytes_expected.txt", TestName = "ShouldSort500BytesFileWithoutMemory")]
        public void ShouldSortFileWithoutMemory(string inputFileName, string outputFileName, string expectedFileName)
        {
            var process = RunProcess(
                "FileSort.exe",
                $"{Path.Combine("Content", inputFileName)} {Path.Combine("Content", outputFileName)} --memory-buffer 0");

            ProcessAssert.HasZeroExitCode(process, $"File '{inputFileName}' sort failed.");

            FileAssert.AreEqual(
                Path.Combine(process.StartInfo.WorkingDirectory, "Content", expectedFileName),
                Path.Combine(process.StartInfo.WorkingDirectory, "Content", outputFileName));
        }

        [TestCase("0mb", TestName = "ShouldSort0MBRandomFile")]
        [TestCase("15bytes", TestName = "ShouldSort15BytesRandomFile")]
        [TestCase("100bytes", TestName = "ShouldSort100BytesRandomFile")]
        [TestCase("1kb", TestName = "ShouldSort1KBRandomFile")]
        [TestCase("10KB", TestName = "ShouldSort10KBRandomFile")]
        [TestCase("10kb", "--memory-buffer 0", TestName = "ShouldSort1KBRandomFileWithoutMemory")]
        [TestCase("100KB", TestName = "ShouldSort100KBRandomFile")]
        [TestCase("100KB", "--memory-buffer 0", TestName = "ShouldSort100KBRandomFileWithoutMemory")]
        [TestCase("1MB", TestName = "ShouldSort1MBRandomFile")]
        [TestCase("10mb", TestName = "ShouldSort10MBRandomFile")]
        [TestCase("10MB", "--memory-buffer 10MB", TestName = "ShouldSort10MBRandomFileWith10MBLimit")]
        [TestCase("100MB", TestName = "ShouldSort100MBRandomFile")]
        [TestCase("1GB", TestName = "ShouldSort1GBRandomFile", IgnoreReason = "Test is too long. Should be run manually")]
        [TestCase("10GB", TestName = "ShouldSort10GBRandomFile", IgnoreReason = "Test is too long. Should be run manually")]
        [TestCase("100GB", TestName = "ShouldSort100GBRandomFile", IgnoreReason = "Test is too long. Should be run manually")]
        public void ShouldSortGeneratedFile(string fileSize, string sortArguments = null)
        {
            var inputFileName = Path.Combine("Content", fileSize + ".txt");
            var outputFileName = Path.Combine("Content", fileSize + "_sorted.txt");

            var generateProcess = RunProcess(
                "FileGenerate.exe",
                $"{inputFileName} -s {fileSize}");

            ProcessAssert.HasZeroExitCode(generateProcess, $"File '{inputFileName}' generation failed.");

            inputFileName = Path.Combine(generateProcess.StartInfo.WorkingDirectory, inputFileName);
            outputFileName = Path.Combine(generateProcess.StartInfo.WorkingDirectory, outputFileName);

            Console.WriteLine($"File '{inputFileName}' was generated in {generateProcess.TotalProcessorTime}.");
            
            var expectedFileSize = MemorySize.Parse(fileSize);

            FileSizeAssert.HasSize(inputFileName, expectedFileSize);

            var sortProcess = RunProcess(
                "FileSort.exe",
                $"{inputFileName} {outputFileName} {sortArguments}");

            ProcessAssert.HasZeroExitCode(sortProcess, $"File '{inputFileName}' sort failed.");

            Console.WriteLine($"File '{outputFileName}' was sorted in {sortProcess.TotalProcessorTime}.");

            FileSizeAssert.HasSize(outputFileName, expectedFileSize);

            var checkProcess = RunProcess(
                "FileCheck.exe",
                $"{outputFileName}");

            Console.WriteLine($"File '{outputFileName}' was checked in {checkProcess.TotalProcessorTime}.");

            ProcessAssert.HasZeroExitCode(checkProcess, $"File '{inputFileName}' was not properly sorted or check failed.");         
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
