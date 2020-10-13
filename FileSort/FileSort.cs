using FileSort.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FileSort
{
    public class FileSort
    {
        private readonly int _fileBufferSize;
        private readonly long _memoryBufferSize;

        public FileSort(int fileBufferSize, long memoryBufferSize)
        {
            _fileBufferSize = fileBufferSize;
            _memoryBufferSize = memoryBufferSize;
            
        }

        public void Sort(string inputFileName, string outputFileName)
        {
            using (var fileStream = FileWithBuffer.OpenRead(inputFileName, _fileBufferSize))
            {
                var fileSize = fileStream.Length;
                var readerWriter = new FileLineReaderWriter();

                var targetChunkStorage = new ChunkFileStorage<FileLine>(
                    outputFileName, 
                    _fileBufferSize,
                    readerWriter);

                var tempFileName = Path.Combine(
                    Path.GetDirectoryName(outputFileName),
                    $"{Path.GetFileNameWithoutExtension(outputFileName)}_temp{Path.GetExtension(outputFileName)}");

                var tempChunkStorage = new ChunkFileStorage<FileLine>(
                    tempFileName,
                    _fileBufferSize,
                    readerWriter);

                var chunkStack = new ChunkStack<FileLine>(
                    _memoryBufferSize,
                    new FileLineSizeCalculator(),
                    targetChunkStorage,
                    tempChunkStorage);

                var sorter = new OppositeMergeSort<FileLine>(chunkStack);

                var inputFileEntries = new StreamEnumerable(fileStream).Select(FileLine.Parse);
                var chunkReference = sorter.SortAsChunk(inputFileEntries);

                if (chunkReference.MemorySize == 0)
                {
                    return;
                }
                else
                {
                    var outputLines = chunkReference.GetValue().Select(x => x.ToString());
                    File.WriteAllLines(outputFileName, outputLines);
                }
            }
        }
    }

    public class StreamEnumerable : IEnumerable<string>
    {
        private readonly Stream _stream;

        public StreamEnumerable(Stream stream)
        {
            _stream = stream;
        }

        public IEnumerator<string> GetEnumerator()
        {
            return new StreamEnumerator(_stream);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        private class StreamEnumerator : IEnumerator<string>
        {
            private readonly StreamReader _streamReader;

            public StreamEnumerator(Stream stream)
            {
                _streamReader = new StreamReader(stream);
            }

            public string Current { get; private set; }

            object IEnumerator.Current => Current;

            public void Dispose()
            {
                _streamReader.Dispose();
            }

            public bool MoveNext()
            {
                if (_streamReader.EndOfStream)
                    return false;

                Current = _streamReader.ReadLine();
                return true;
            }

            public void Reset()
            {
                throw new NotImplementedException();
            }
        }
    }
}
