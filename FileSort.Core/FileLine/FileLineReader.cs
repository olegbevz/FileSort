﻿using FileSort.Core.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FileSort.Core
{
    /// <summary>
    /// FileLineReader represents simple FileLine enumeration based on input stream
    /// </summary>
    public class FileLineReader : IEnumerable<FileLine>
    {
        private static readonly ILog _logger = LogProvider.For<FileLineReader>();

        private const long WritePositionFrequency = 1000000;

        private readonly Stream _stream;
        private readonly int _streamBuffer;

        public FileLineReader(Stream stream, int streamBuffer)
        {
            _stream = stream;
            _streamBuffer = streamBuffer;
        }

        public IEnumerator<FileLine> GetEnumerator()
        {
            return new FileLineEnumerator(_stream, _streamBuffer);
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
            private long _currentLine;

            public FileLineEnumerator(Stream stream, int streamBuffer)
            {
                _stream = stream;
                _streamReader = new StreamReader(_stream, Encoding.UTF8, true, streamBuffer);
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
                _currentLine++;

                if (!FileLine.TryParse(_streamReader, out _current))
                {
                    if (_streamReader.EndOfStream)
                        return false;

                    throw new ArgumentException(
                        $"Failed to parse line {_currentLine} '{_streamReader.ReadLine()}' at position {_streamReader.BaseStream.Position}.");
                }

                if (_currentLine % WritePositionFrequency == 0)
                {
                    _logger.Debug($"Currently {_stream.Position} bytes readen");
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
