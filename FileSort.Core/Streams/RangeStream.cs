using System;
using System.IO;

namespace FileSort.Core
{
    public class RangeStream : Stream
    {
        private readonly Stream _underlyingStream;
        private readonly long _startPosititon;
        private readonly long _length;
        private readonly long _endPosition;

        public RangeStream(Stream stream, long startPosititon, long endPosition)
        {
            _underlyingStream = stream;
            _startPosititon = startPosititon;
            _length = endPosition - startPosititon;
            _endPosition = endPosition;
        }

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => _length;

        public override long Position
        {
            get => _underlyingStream.Position;
            set => _underlyingStream.Position = value;
        }

        protected override void Dispose(bool disposing)
        {
            _underlyingStream.Dispose();
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_underlyingStream.Position < _startPosititon)
                _underlyingStream.Seek(_startPosititon, SeekOrigin.Begin);

            if (_underlyingStream.Position + count >= _endPosition)
                count = (int)(_endPosition - _underlyingStream.Position);

            return _underlyingStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }
}
