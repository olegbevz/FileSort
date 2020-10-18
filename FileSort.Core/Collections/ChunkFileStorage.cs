using FileSort.Core.Logging;
using System;
using System.Collections.Generic;
using System.IO;

namespace FileSort.Core
{
    /// <summary>
    /// ChunkStack file storage
    /// Puts chunk of data to the disk if it doesn't fit into memory
    /// </summary>
    /// <typeparam name="T">Type of data</typeparam>
    public class ChunkFileStorage<T> : IChunkStorage<T>, IDisposable
    {
        private static readonly ILog _logger = LogProvider.GetCurrentClassLogger();

        private readonly string _fileName;
        private readonly int _fileBuffer;
        private readonly IChunkReaderWriter<T> _readerWriter;
        private readonly bool _removeOnDispose;
        private long _currentPosition = 0;

        public ChunkFileStorage(
            string fileName, 
            int fileBuffer, 
            IChunkReaderWriter<T> readerWriter, 
            bool removeOnDispose = false)
        {
            _fileName = fileName;
            _fileBuffer = fileBuffer;
            _readerWriter = readerWriter;
            _removeOnDispose = removeOnDispose;
        }

        public IEnumerable<T> OpenForRead(long size)
        {
            if (size > _currentPosition)
                throw new IndexOutOfRangeException();

            var fileStream = FileWithBuffer.OpenRead(_fileName, _fileBuffer);

            long endPosition = _currentPosition;
            _currentPosition -= size;
            _logger.Debug($"File '{_fileName}' was opened for read from position {_currentPosition} to {endPosition}.");
            
            var rangeStream = new RangeStream(fileStream, _currentPosition, endPosition);
            return _readerWriter.ReadFromStream(rangeStream);
        }

        public IChunkStorageWriter<T> OpenForWrite()
        {
            _logger.Debug($"File '{_fileName}' was opened for write at position {_currentPosition}.");
            var fileStream = FileWithBuffer.OpenAppend(_fileName, _fileBuffer);
            fileStream.Seek(_currentPosition, SeekOrigin.Begin);
            return new ChunkFileStorageWriter(
                fileStream,
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

        public void Dispose()
        {
            if (_removeOnDispose)
                File.Delete(_fileName);
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
            public void Write(IEnumerable<T> values)
            {
                _chunkWriter.WriteToStream(_streamWriter, values);
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
                _logger.Debug($"Writing to file '{_fileStream.Name}' completed at position {_endPosition}.");
                return _endPosition - _startPosition;
            }

            public void Dispose()
            {
                _streamWriter.Dispose();
            }
        }
    }
}
