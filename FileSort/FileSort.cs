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

        public FileSort(int fileBufferSize)
        {
            _fileBufferSize = fileBufferSize;
        }

        public void Sort(string inputFileName, string outputFileName)
        {
            using (var fileStream = FileWithBuffer.OpenRead(inputFileName, _fileBufferSize))
            {
                var inputFileEntries = new StreamEnumerable(fileStream).Select(FileEntry.Parse);
                var sorter = new OppositeMergeSort();
                var outputFileEntries = sorter.Sort(inputFileEntries);
                var outputLines = outputFileEntries.Select(x => x.ToString());
                File.WriteAllLines(outputFileName, outputLines);
            }
        }

        private class StreamEnumerable : IEnumerable<string>
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
}
