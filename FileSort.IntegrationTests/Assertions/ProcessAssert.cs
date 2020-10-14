using NUnit.Framework;
using System.Diagnostics;

namespace FileSort.IntegrationTests
{
    public static class ProcessAssert
    {
        public static void HasZeroExitCode(Process process, string message)
        {
            Assert.AreEqual(0, process.ExitCode, message);
        }

        public static void HasExitCode(Process process, int expectedExitCode, string message)
        {
            Assert.AreEqual(expectedExitCode, process.ExitCode, message);
        }
    }
}
