using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace FileSort
{
    public class ChunkStack<T>
    {
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

        public void Push(IEnumerable<T> chunk)
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
                ResizeStack(chunkSize);
            }
        }

        public void Push(IChunkReference<T> chunkReference)
        {
            if (chunkReference is MemoryChunkReference memoryWriter)
            {
                Push(memoryWriter.ToArray());
            }

            if (chunkReference is FileChunkReference fileChunkReference)
            {
                Push(fileChunkReference.GetValue(), fileChunkReference.Count, fileChunkReference.TotalSize);
            }
        }

        public IWritableChunkReference<T> CreateChunkForMerge(
            IChunkReference<T> leftChunk, 
            IChunkReference<T> rightChunk)
        {
            var totalSize = leftChunk.TotalSize + rightChunk.TotalSize;
            var totalCount = leftChunk.Count + rightChunk.Count;
            if (_currentSize + totalSize > _bufferSize)
            {
                return new FileChunkReference(0, totalCount, _chunkStorage);
            }

            return new MemoryChunkReference(totalCount, 0);
        }

        public IWritableChunkReference<T> CreateChunkForMerge(IChunkReference<T>[] chunks)
        {
            var totalSize = chunks.Sum(x => x.TotalSize);
            var totalCount = chunks.Sum(x => x.Count);
            if (_currentSize + totalSize > _bufferSize)
            {
                return new FileChunkReference(0, totalCount, _chunkStorage);
            }

            return new MemoryChunkReference(totalCount, 0);
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

        private void ResizeStack(long reqiredSpaceToAdd)
        {
            if (_stack.Count == 0)
                return;

            var sizeCounter = reqiredSpaceToAdd;
            var array = new IChunkReference<T>[_stack.Count];
            var arrayIndex = _stack.Count - 1;

            foreach (var chunk in _stack)
            {
                sizeCounter += chunk.MemorySize;

                if (sizeCounter > _bufferSize && !(chunk is FileChunkReference))
                {
                    array[arrayIndex] = CreateFileChunkReference(chunk.GetValue().ToArray());
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

        private FileChunkReference CreateFileChunkReference(T[] chunk)
        {
            return CreateFileChunkReference(chunk, chunk.Length);
        }

        private FileChunkReference CreateFileChunkReference(IEnumerable<T> chunk, int count)
        {
            var size = _chunkStorage.Push(chunk);
            return new FileChunkReference(size, count, _chunkStorage);
        }

        private class MemoryChunkReference : IWritableChunkReference<T>
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

            public void Complete()
            {
                _index = 0;
            }

            public void Flush(IChunkStorage<T> chunkStorage)
            {
                chunkStorage.Push(_array);
            }

            public IEnumerator<T> GetEnumerator()
            {
                return this.GetValue().GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }
        }

        private class FileChunkReference : IWritableChunkReference<T>
        {
            private readonly IChunkStorage<T> _chunkStorage;
            private IChunkStorageWriter<T> _chunkStorageWriter;

            public FileChunkReference(long size, int count, IChunkStorage<T> chunkStorage)
            {
                _chunkStorage = chunkStorage;
                TotalSize = size;
                Count = count;
            }

            public long MemorySize { get { return 0; } }
            public int Count { get; private set; }
            public IEnumerable<T> GetValue()
            {
                return _chunkStorage.Pop(TotalSize);
            }

            public long TotalSize { get; private set; }

            public void Write(T value)
            {
                if (_chunkStorageWriter == null)
                    _chunkStorageWriter = _chunkStorage.GetWriter();

                _chunkStorageWriter.Write(value);
            }

            public void Complete()
            {
                TotalSize = _chunkStorageWriter.Complete();
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
        }
    }
}
