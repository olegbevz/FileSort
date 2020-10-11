using System;
using System.IO;

namespace FileSort
{
    public class RangeStream : Stream
    {
        private readonly Stream _underlyingStream;
        private readonly long _startPosititon;
        private readonly long _length;

        public RangeStream(Stream underlyingStream, long startPosititon, long length)
        {
            _underlyingStream = underlyingStream;
            _underlyingStream.Seek(_startPosititon, SeekOrigin.Begin);
            _startPosititon = startPosititon;
            _length = length;
        }

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => _length;

        public override long Position { get => _underlyingStream.Position; set => _underlyingStream.Position = value; }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
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
