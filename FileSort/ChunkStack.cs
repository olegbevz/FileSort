using FileSort.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace FileSort
{

    public class ChunkStack<T>
    {
        private Stack<IChunkReference<T>> _stack = new Stack<IChunkReference<T>>();        
        private readonly ISizeCalculator<T> _sizeCalcuator;
        private readonly IChunkReaderWriter<T> _readerWriter;
        private readonly long _bufferSize;
        private readonly string _fileName;

        private long _currentSize;

        public ChunkStack(
            long bufferSize, 
            ISizeCalculator<T> sizeCalculator, 
            IChunkReaderWriter<T> readerWriter,
            string fileName)
        {
            _bufferSize = bufferSize;
            _sizeCalcuator = sizeCalculator;
            _readerWriter = readerWriter;
            _fileName = fileName;
        }

        public int Count
        {
            get
            {
                return _stack.Count;
            }
        }

        public int LastChunkLength
        {
            get
            {
                return _stack.Peek().Count;
            }
        }

        public IChunkReference<T> Pop()
        {
            var chunk =  _stack.Pop();
            _currentSize -= chunk.MemorySize;
            return chunk;
        }

        public void Push(T[] chunk)
        {
            var chunkSize = chunk.Sum(x => _sizeCalcuator.GetBytesCount(x));

            if (_currentSize + chunkSize > _bufferSize)
            {
                ResizeStack(chunkSize);
            }

            _stack.Push(new MemoryChunkReference(chunk, chunkSize));
            _currentSize += chunkSize;
        }

        public void Push(IChunkReference<T> chunkWriter)
        {
            if (chunkWriter is MemoryChunkReference memoryWriter)
            {
                Push(memoryWriter.ToArray());
                return;
            }

            throw new NotImplementedException();
        }

        public IChunkWriter<T> CreateChunkForMerge(IChunkReference<T> leftChunk, IChunkReference<T> rightChunk)
        {
            return new MemoryChunkReference(leftChunk.Count + rightChunk.Count, 0);
        }

        public IChunkReference<T> CreateChunk(T[] chunk)
        {
            return new MemoryChunkReference(chunk, chunk.Length);
        }

        private void ResizeStack(long reqiredSpaceToAdd)
        {
            var sizeCounter = reqiredSpaceToAdd;
            var array = new IChunkReference<T>[_stack.Count];
            var arrayIndex = _stack.Count - 1;

            foreach (var chunk in _stack)
            {
                sizeCounter += chunk.MemorySize;

                if (sizeCounter > _bufferSize)
                {
                    array[arrayIndex] = WriteToFile(chunk.Value.ToArray());
                    _currentSize -= chunk.MemorySize;
                }
                else
                {
                    array[arrayIndex] = chunk;
                }

                arrayIndex--;
            }

            _stack = new Stack<IChunkReference<T>>(array);
        }

        private FileChunkReference WriteToFile(T[] chunk)
        {
            using (var fileStream = File.OpenWrite(_fileName))
            {
                _readerWriter.WriteToStream(fileStream, chunk);

                return new FileChunkReference(
                    _fileName,
                    0,
                    fileStream.Position,
                    _readerWriter,
                    chunk.Length);
            }
        }

        public class MemoryChunkReference : IChunkWriter<T>
        {
            private readonly T[] _array;
            private long _index;

            public MemoryChunkReference(long count, long size)
            {
                _array = new T[count];
                MemorySize = size;
            }

            public MemoryChunkReference(T[] value, long size)
            {
                _array = value;
                MemorySize = size;
            }

            public long MemorySize { get; private set; }
            public int Count { get { return _array.Length; } }
            public IEnumerable<T> Value { get { return _array; } }

            public long TotalSize => MemorySize;

            public void Write(T value)
            {
                _array[_index] = value;
                Interlocked.Increment(ref _index);
            }

            public T[] ToArray()
            {
                return _array;
            }
        }

        private class FileChunkReference : IChunkReference<T>
        {
            private readonly string _fileName;
            private readonly long _startPosition;
            private readonly long _length;
            private readonly IChunkReaderWriter<T> _readerWriter;

            public FileChunkReference(
                string fileName, 
                long startPosition, 
                long length, 
                IChunkReaderWriter<T> readerWriter, 
                int count)
            {
                _fileName = fileName;
                _startPosition = startPosition;
                _length = length;
                _readerWriter = readerWriter;
                Count = count;
            }

            public long MemorySize { get { return 0; } }
            public int Count { get; private set; }
            public IEnumerable<T> Value
            {
                get
                {
                    var fileStream = File.OpenRead(_fileName);
                    var rangeStream = new RangeStream(fileStream, _startPosition, _length);
                    return _readerWriter.ReadFromStream(rangeStream);
                }
            }

            public long TotalSize => _length;
        }
    }

    public interface IChunkReference<T>
    {
        long MemorySize { get; }
        long TotalSize { get; }
        int Count { get; }
        IEnumerable<T> Value { get; }
    }

    public interface IChunkWriter<T> : IChunkReference<T>
    {
        void Write(T value);
    }

    public interface IChunkReaderWriter<T>
    {
        void WriteToStream(Stream stream, IEnumerable<T> source);
        IEnumerable<T> ReadFromStream(Stream stream);
    }

    public class FileLineReaderWriter : IChunkReaderWriter<FileLine>
    {
        public IEnumerable<FileLine> ReadFromStream(Stream stream)
        {
            return new StreamEnumerable(stream).Select(FileLine.Parse);
        }

        public void WriteToStream(Stream stream, IEnumerable<FileLine> source)
        {
            var streamWriter = new StreamWriter(stream);
            foreach (var line in source)
                streamWriter.WriteLine(line.ToString());
        }
    }
}
