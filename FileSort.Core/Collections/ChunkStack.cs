using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace FileSort.Core
{
    public class ChunkStack<T>
    {
        public static IChunkReference<T> Empty = new MemoryChunkReference(0, 0);

        private Stack<IChunkReference<T>> _stack = new Stack<IChunkReference<T>>();        
        private readonly ISizeCalculator<T> _sizeCalcuator;
        private readonly long _bufferSize;
        private readonly IChunkStorage<T> _chunkStorage;

        private long _currentSize;

        public ChunkStack(
            long bufferSize, 
            ISizeCalculator<T> sizeCalculator,
            IChunkStorage<T> chunkStorage)
        {
            _bufferSize = bufferSize;
            _sizeCalcuator = sizeCalculator;
            _chunkStorage = chunkStorage;
        }

        public int Count
        {
            get
            {
                return _stack.Count;
            }
        }

        public long MemorySize
        {
           get
           {
                return _currentSize;
           }
        }

        public long TotalSize
        {
            get
            {
                // TODO: store total size field instread of calculation
                return _stack.ToArray().Sum(x => x.TotalSize);
            }
        }

        public int? LastChunkLength
        {
            get
            {
                if (_stack.Count == 0)
                    return null;

                return _stack.Peek().Count;
            }
        }

        public IChunkReference<T> Pop()
        {
            var chunk = _stack.Pop();
            _currentSize -= chunk.MemorySize;
            return chunk;
        }

        public void Push(T[] chunk)
        {
            var chunkSize = chunk.Sum(x => _sizeCalcuator.GetBytesCount(x));

            Push(chunk, chunk.Length, chunkSize);
        }

        public void Push(List<T> chunk)
        {
            var chunkSize = chunk.Sum(x => _sizeCalcuator.GetBytesCount(x));

            Push(chunk, chunk.Count(), chunkSize);
        }

        public void Push(IEnumerable<T> chunk, int count, long chunkSize)
        {
            EnsureStakSize(chunkSize);

            IChunkReference<T> chunkReference;
            if (chunkSize > _bufferSize)
            {
                chunkReference = CreateFileChunkReference(chunk, count);
            }
            else
            {
                chunkReference = new MemoryChunkReference(chunk.ToArray(), chunkSize);
            }

            _stack.Push(chunkReference);

            _currentSize += chunkReference.MemorySize;
        }

        private void EnsureStakSize(long chunkSize)
        {
            if (_currentSize + chunkSize > _bufferSize && !_stack.All(x => x is FileChunkReference))
            {
                while (_currentSize + chunkSize > _bufferSize)
                {
                    ShrinkStack();
                }
            }
        }

        public IChunkWriter<T> CreateChunkForMerge(
            IChunkReference<T> leftChunk, 
            IChunkReference<T> rightChunk)
        {
            var totalSize = leftChunk.TotalSize + rightChunk.TotalSize;
            var totalCount = leftChunk.Count + rightChunk.Count;
            return CreateChunkForMerge(totalSize, totalCount);
        }

        public IChunkWriter<T> CreateChunkForMerge(IChunkReference<T>[] chunks)
        {
            var totalSize = chunks.Sum(x => x.TotalSize);
            var totalCount = chunks.Sum(x => x.Count);
            return CreateChunkForMerge(totalSize, totalCount);
        }

        private IChunkWriter<T> CreateChunkForMerge(long totalSize, int totalCount)
        {
            IChunkWriter<T> chunkWriter;
            IChunkReference<T> chunkReference;
            if (_currentSize + totalSize > _bufferSize)
            {
                var fileChunk = new FileChunkReference(totalSize, totalCount, _chunkStorage, true);
                chunkWriter = fileChunk;
                chunkReference = fileChunk;
            }
            else
            {
                var memoryChunk = new MemoryChunkReference(totalCount, totalSize);
                chunkWriter = memoryChunk;
                chunkReference = memoryChunk;
                _currentSize += totalSize;
            }

            _stack.Push(chunkReference);

            return chunkWriter;
        }

        public IChunkReference<T> CreateChunk(T[] chunk)
        {
            var memorySize = chunk.Sum(x => _sizeCalcuator.GetBytesCount(x));
            return new MemoryChunkReference(chunk, memorySize);
        }

        public IChunkReference<T> CreateChunk(List<T> chunk)
        {
            var memorySize = chunk.Sum(x => _sizeCalcuator.GetBytesCount(x));
            return new MemoryChunkReference(chunk.ToArray(), memorySize);
        }

        public IChunkReference<T>[] ToArray()
        {
            return _stack.ToArray();
        }

        public int[] GetChunkSizes()
        {
            return _stack.Select(chunk => chunk.Count).ToArray();
        }

        private void ShrinkStack()
        {
            if (_stack.Count == 0)
                return;

            var sourceArray = _stack.ToArray();

            for (int i = sourceArray.Length - 1; i >= 0; i --)
            {
                var chunk = sourceArray[i];
                if (chunk is MemoryChunkReference)
                {
                    sourceArray[i] = CreateFileChunkReference(chunk.GetValue().ToArray());
                    _currentSize -= chunk.MemorySize;
                    break;
                }
            }

            _stack = new Stack<IChunkReference<T>>(sourceArray.Reverse());
        }

        private FileChunkReference CreateFileChunkReference(T[] chunk)
        {
            return CreateFileChunkReference(chunk, chunk.Length);
        }

        private FileChunkReference CreateFileChunkReference(IEnumerable<T> chunk, int count)
        {
            using (var writer = _chunkStorage.OpenForWrite())
            {
                writer.Write(chunk);
                var size = writer.Complete();
                return new FileChunkReference(size, count, _chunkStorage);
            }
        }

        private class MemoryChunkReference : IChunkReference<T>, IChunkWriter<T>
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
            public long TotalSize => MemorySize;
            public IEnumerable<T> GetValue()
            {
                return _array;
            }

            public void Write(T value)
            {
                _array[_index] = value;
                Interlocked.Increment(ref _index);
            }

            public T[] ToArray()
            {
                return _array;
            }

            public void Flush(IChunkStorage<T> chunkStorage)
            {
                using (var writer = chunkStorage.OpenForWrite())
                {
                    writer.Write(_array);
                    writer.Complete();
                }
            }

            public IEnumerator<T> GetEnumerator()
            {
                return this.GetValue().GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            IChunkReference<T> IChunkWriter<T>.Complete()
            {
                _index = 0;
                return this;
            }

            public void Dispose()
            {
                _index = 0;
            }
        }

        private class FileChunkReference : IChunkReference<T>, IChunkWriter<T>
        {
            private readonly IChunkStorage<T> _chunkStorage;
            private IChunkStorageWriter<T> _chunkStorageWriter;

            public FileChunkReference(long size, int count, IChunkStorage<T> chunkStorage, bool chunkWriter = false)
            {
                _chunkStorage = chunkStorage;                
                TotalSize = size;
                Count = count;

                if (chunkWriter)
                    _chunkStorageWriter = chunkStorage.OpenForWrite();
            }

            public long MemorySize { get { return 0; } }
            public int Count { get; private set; }
            public IEnumerable<T> GetValue()
            {
                return _chunkStorage.OpenForRead(TotalSize);
            }

            public long TotalSize { get; private set; }

            public void Write(T value)
            {
                if (_chunkStorageWriter == null)
                    throw new Exception("Chunk writer was not initialized or already completed");

                _chunkStorageWriter.Write(value);
            }

            public void Flush(IChunkStorage<T> chunkStorage)
            {
                if (_chunkStorage != chunkStorage)
                {
                    _chunkStorage.CopyTo(chunkStorage);
                }
            }

            public IEnumerator<T> GetEnumerator()
            {
                return this.GetValue().GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            IChunkReference<T> IChunkWriter<T>.Complete()
            {
                if (_chunkStorageWriter == null)
                    throw new Exception("Chunk writer was not initialized or already completed");
                TotalSize = _chunkStorageWriter.Complete();                
                return this;
            }

            public void Dispose()
            {
                if (_chunkStorageWriter == null)
                    return;
                _chunkStorageWriter.Dispose();
                _chunkStorageWriter = null;
            }
        }
    }
}
