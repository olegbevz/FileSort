using System;
using System.IO;
using CommandLine;

namespace FileGenerate
{
    partial class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<FileGenerateOptions>(args)
                .WithParsed(HandleFileGenerate);
        }

        private static void HandleFileGenerate(FileGenerateOptions options)
        {
            try
            {
                using (var fileStream = File.Open(options.OutputFileName, FileMode.Create))
                using (var streamWriter = new StreamWriter(fileStream))
                {
                    var randomStringSource = new RandomStringEnumerable(
                            options.FileSize,
                            streamWriter.Encoding,
                            streamWriter.NewLine);

                    foreach (var line in randomStringSource)
                        streamWriter.WriteLine(line);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
