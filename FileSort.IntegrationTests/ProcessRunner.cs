using System;
using System.Diagnostics;
using System.IO;

namespace FileSort.IntegrationTests
{
    public static class ProcessRunner
    {
        public static Process RunProcess(string executable, string arguments)
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
