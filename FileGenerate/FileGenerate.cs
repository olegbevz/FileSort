using System;
using System.IO;
using FileSort.Core;

namespace FileGenerate
{
    /// <summary>
    /// FileGenerate contains random file generation logic
    /// </summary>
    public class FileGenerate
    {
        private readonly int _fileBuffer;
        private readonly long _memoryBuffer;
        private readonly int _duplicates;
        private readonly IRandomStringFactory _stringFactory;

        public FileGenerate(int fileBuffer, long memoryBuffer, int duplicates, IRandomStringFactory stringFactory)
        {
            _fileBuffer = fileBuffer;
            _memoryBuffer = memoryBuffer;
            _duplicates = duplicates;
            _stringFactory = stringFactory;
        }

        public void Generate(string fileName, long fileSize)
        {
            using (var fileStream = FileWithBuffer.OpenWrite(fileName, _fileBuffer))
            {
                if (_duplicates > 0)
                {
                    WriteRandomStringsWithDuplicates(
                        fileStream,
                        fileSize);
                }
                else
                {
                    WriteRandomStringsWithoutDuplicates(
                        fileStream,
                        fileSize);
                }
            }
        }

        private void WriteRandomStringsWithoutDuplicates(
            FileStream fileStream,
            long fileSize)
        {
            using (var streamWriter = new StreamWriter(fileStream))
            {
                WriteRandomStrings(streamWriter, fileSize);
            }
        }

        private void WriteRandomStringsWithDuplicates(
            FileStream fileStream,
            long fileSize)
        {
            var duplicateCounter = 0;

            var buffer = new byte[_memoryBuffer];
            using (var bufferStream = new MemoryStream(buffer))
            {
                using (var streamWriter = new StreamWriter(bufferStream))
                {
                    while (fileStream.Position + _memoryBuffer < fileSize)
                    {
                        var writeDuplicate = duplicateCounter > 0;

                        if (!writeDuplicate)
                        {
                            WriteRandomStrings(streamWriter, _memoryBuffer);
                            streamWriter.Flush();
                        }

                        fileStream.Write(buffer, 0, buffer.Length);

                        if (!writeDuplicate)
                        {
                            bufferStream.Seek(0, SeekOrigin.Begin);
                        }

                        duplicateCounter++;
                        if (duplicateCounter >= _duplicates)
                            duplicateCounter = 0;
                    }
                }
            }

            if (fileStream.Position < fileSize)
            {
                var remainedSize = fileSize - fileStream.Position;

                using (var streamWriter = new StreamWriter(fileStream))
                {
                    WriteRandomStrings(streamWriter, remainedSize);
                }
            }
        }

        private void WriteRandomStrings(
            StreamWriter streamWriter,
            long targetSize)
        {
            var randomStringSource = new RandomStringReader(
                targetSize,
                streamWriter.Encoding,
                streamWriter.NewLine,
               _stringFactory);

            foreach (var line in randomStringSource)
                streamWriter.WriteLine(line);
        }
    }
}
