using NUnit.Framework;
using System;
using System.IO;

namespace FileSort.IntegrationTests
{
    public static class FileSizeAssert
    {
        public static void HasSize(string fileName, long expectedFileSize, int deviation = 1)
        {
            var actualFileSize = new FileInfo(fileName).Length;
            var sizeDifference = Math.Abs(expectedFileSize - actualFileSize);

            Assert.LessOrEqual(
                sizeDifference,
                deviation,
                $"File '{fileName}' should have size {expectedFileSize} but has {actualFileSize}.");
        }
    }
}
