﻿using System;
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
            private FileLine _current;

            public FileLineEnumerator(Stream stream)
            {
                _stream = stream;
                _streamReader = new StreamReader(_stream);
            }

            public FileLine Current
            {
                get
                {
                    return _current;
                }

                private set
                {
                    _current = value;
                }
            }

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
                if (!FileLine.TryParse(_streamReader, out _current))
                {
                    if (_streamReader.EndOfStream)
                        return false;

                    throw new ArgumentException($"Failed to parse stream line '{_streamReader.ReadLine()}'.");
                }

                return true;
            }

            public void Reset()
            {
                throw new NotImplementedException();
            }
        }
    }
}
