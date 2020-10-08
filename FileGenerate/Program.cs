using System;
using System.IO;
using CommandLine;

namespace FileGenerate
{
    class Program
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
                            MemorySize.Parse(options.FileSize).GetTotalBytes(),
                            streamWriter.Encoding,
                            streamWriter.NewLine,
                            new ConstantStringFactory("30432. Something something something"));

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
