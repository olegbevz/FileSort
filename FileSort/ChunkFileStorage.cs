using FileSort.Core;
using System;
using System.Collections.Generic;
using System.IO;

namespace FileSort
{
    public class ChunkFileStorage<T> : IChunkStorage<T>
    {
        private readonly string _fileName;
        private readonly int _fileBuffer;
        private readonly IChunkReaderWriter<T> _readerWriter;

        private long _currentPosition = 0;
        private long _currentSize = 0;

        public long Size { get { return _currentSize; } }
        public bool IsEmpty { get { return _currentSize == 0; } }

        public ChunkFileStorage(string fileName, int fileBuffer, IChunkReaderWriter<T> readerWriter)
        {
            _fileName = fileName;
            _fileBuffer = fileBuffer;
            _readerWriter = readerWriter;
        }

        public long Push(IEnumerable<T> source)
        {
            using (var fileStream = FileWithBuffer.OpenAppend(_fileName, _fileBuffer))
            {
                fileStream.Seek(_currentPosition, SeekOrigin.Begin);
                using (var streamWriter = new StreamWriter(fileStream))
                {
                    _readerWriter.WriteToStream(streamWriter, source);
                    streamWriter.Flush();
                    var size = fileStream.Position - _currentPosition;
                    _currentPosition = fileStream.Position;
                    _currentSize = Math.Max(fileStream.Position, _currentSize);
                    return size;
                }
            }
        }

        public IEnumerable<T> Pop(long size)
        {
            if (size > _currentPosition)
                throw new IndexOutOfRangeException();

            var fileStream = FileWithBuffer.OpenRead(_fileName, _fileBuffer);
            var rangeStream = new RangeStream(fileStream, _currentPosition - size, _currentPosition);
            _currentPosition -= size;
            return _readerWriter.ReadFromStream(rangeStream);
        }

        public IChunkStorageWriter<T> GetWriter()
        {
            return new ChunkFileStorageWriter(
                FileWithBuffer.OpenAppend(_fileName, _fileBuffer),
                _readerWriter,
                this);
        }

        public void CopyTo(IChunkStorage<T> chunkStorage)
        {
            if (chunkStorage is ChunkFileStorage<T> chunkFileStorage)
            {
                File.Delete(chunkFileStorage._fileName);
                File.Move(_fileName, chunkFileStorage._fileName);
            }
        }

        private class ChunkFileStorageWriter : IChunkStorageWriter<T>
        {
            private readonly FileStream _fileStream;
            private readonly StreamWriter _streamWriter;
            private readonly long _startPosition;
            private long _endPosition;
            private readonly IChunkReaderWriter<T> _chunkWriter;
            private readonly ChunkFileStorage<T> _chunkFileStorage;

            public ChunkFileStorageWriter(FileStream fileStream, IChunkReaderWriter<T> chunkWriter, ChunkFileStorage<T> chunkFileStorage)
            {
                _fileStream = fileStream;
                _streamWriter = new StreamWriter(fileStream);
                _startPosition = fileStream.Position;
                _chunkWriter = chunkWriter;
                _chunkFileStorage = chunkFileStorage;
            }

            public void Write(T value)
            {
                _chunkWriter.WriteToStream(_streamWriter, new[] { value });
            }

            public long Complete()
            {
                _streamWriter.Flush();
                _endPosition = _fileStream.Position;

                _chunkFileStorage._currentPosition = _fileStream.Position;
                _chunkFileStorage._currentSize = Math.Max(_fileStream.Position, _chunkFileStorage._currentSize);

                _streamWriter.Dispose();

                return _endPosition - _startPosition;
            }
        }
    }
}
