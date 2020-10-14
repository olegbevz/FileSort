using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace FileSort.Core
{
    public class FileLineReader : IEnumerable<FileLine>
    {
        private readonly Stream _stream;

        public FileLineReader(Stream stream)
        {
            _stream = stream;
        }

        public IEnumerator<FileLine> GetEnumerator()
        {
            return new FileLineEnumerator(_stream);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        private class FileLineEnumerator : IEnumerator<FileLine>
        {
            private readonly StreamReader _streamReader;
            private readonly Stream _stream;

            public FileLineEnumerator(Stream stream)
            {
                _stream = stream;
                _streamReader = new StreamReader(_stream);
            }

            public FileLine Current { get; private set; }

            object IEnumerator.Current
            {
                get
                {
                    return Current;
                }
            }

            public void Dispose()
            {
                _streamReader.Dispose();
            }

            public bool MoveNext()
            {
                if (_streamReader.EndOfStream)
                    return false;

                Current = FileLine.Parse(_streamReader);
                return true;
            }

            public void Reset()
            {
                throw new NotImplementedException();
            }
        }
    }
}
