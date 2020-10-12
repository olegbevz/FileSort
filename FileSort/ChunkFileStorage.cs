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

        public bool IsEmpty { get { return _currentPosition == 0; } }

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
    }
}
