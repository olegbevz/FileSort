using NUnit.Framework;
using System;
using System.IO;

namespace FileSort.IntegrationTests
{
    [TestFixture]
    [NonParallelizable]
    public class FileCheckTests
    {
        [TestCase("simple_expected.txt", 0, TestName = "ShouldCheckSortedFile")]
        [TestCase("simple.txt", 1, TestName = "ShouldCheckNotSortedFile")]
        [TestCase("simple.txt", 0, "--check-format", TestName = "ShouldCheckFormatOfNotSortedFile")]
        public void ShouldCheckFile(string fileName, int expectedExitCode, string arguments = null)
        {
            fileName = Path.Combine("Content", fileName);

            var checkProcess = ProcessRunner.RunProcess("FileCheck.exe", $"{fileName} {arguments}");
            Console.WriteLine($"File '{fileName}' was checked in {checkProcess.TotalProcessorTime}.");
            ProcessAssert.HasExitCode(checkProcess, expectedExitCode, $"File check process complete with wrong error code.");
        }
    }
}
