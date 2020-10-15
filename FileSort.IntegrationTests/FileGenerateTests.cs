using FileSort.Core;
using NUnit.Framework;
using System;
using System.IO;

namespace FileSort.IntegrationTests
{
    [TestFixture]
    [NonParallelizable]
    public class FileGenerateTests
    {
        [TestCase("0bytes", TestName = "ShouldGenerateEmptyFile")]
        [TestCase("10KB", TestName = "ShouldGenerate10KBFile")]
        [TestCase("100KB", TestName = "ShouldGenerate100KBFile")]
        [TestCase("1MB", TestName = "ShouldGenerate1MBFile")]
        [TestCase("10MB", TestName = "ShouldGenerate10MBFile")]
        [TestCase("100MB", TestName = "ShouldGenerate100MBFile")]
        [TestCase("1GB", TestName = "ShouldGenerate1GBFile")]
        [TestCase("10GB", TestName = "ShouldGenerate10GBRandomFile", IgnoreReason = "Test is too long. Should be run manually")]
        [TestCase("100GB", TestName = "ShouldGenerate100GBRandomFile", IgnoreReason = "Test is too long. Should be run manually")]
        public void ShouldGenerateRandomFile(string fileSize)
        {
            var fileName = Path.Combine("Content", fileSize + ".txt");
            var generateProcess = ProcessRunner.RunProcess("FileGenerate.exe", $"{fileName} -s {fileSize}");

            ProcessAssert.HasZeroExitCode(generateProcess, $"File '{fileName}' generation failed.");
            fileName = Path.Combine(generateProcess.StartInfo.WorkingDirectory, fileName);
            Console.WriteLine($"File '{fileName}' was generated in {generateProcess.TotalProcessorTime}.");
            var expectedFileSize = MemorySize.Parse(fileSize);
            FileSizeAssert.HasSize(fileName, expectedFileSize);

            var checkProcess = ProcessRunner.RunProcess("FileCheck.exe", $"{fileName} --check-format");
            Console.WriteLine($"File '{fileName}' was checked in {checkProcess.TotalProcessorTime}.");
            ProcessAssert.HasZeroExitCode(checkProcess, $"File '{fileName}' was not properly sorted or check failed.");
        }
    }
}
