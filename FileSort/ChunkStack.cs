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
        private readonly IChunkStorage<T> _tempChunkStorage;

        private long _currentSize;

        public ChunkStack(
            long bufferSize, 
            ISizeCalculator<T> sizeCalculator,
            IChunkStorage<T> chunkStorage,
            IChunkStorage<T> tempChunkStorage)
        {
            _bufferSize = bufferSize;
            _sizeCalcuator = sizeCalculator;
            _chunkStorage = chunkStorage;
            _tempChunkStorage = tempChunkStorage;
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

        private void Push(IEnumerable<T> chunk, int count, long chunkSize)
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
                return;
            }

            if (chunkReference is FileChunkReference fileChunkReference)
            {
                Push(fileChunkReference.GetValue(), fileChunkReference.Count, fileChunkReference.TotalSize);
            }
        }

        public IWritableChunkReference<T> CreateChunkForMerge(IChunkReference<T> leftChunk, IChunkReference<T> rightChunk)
        {
            if (leftChunk is FileChunkReference || rightChunk is FileChunkReference)
            {
                return new FileChunkReference(
                    _tempChunkStorage,
                    0, 
                    leftChunk.Count + rightChunk.Count);
            }
                

            return new MemoryChunkReference(leftChunk.Count + rightChunk.Count, 0);
        }

        public IChunkReference<T> CreateChunk(T[] chunk)
        {
            var memorySize = chunk.Sum(x => _sizeCalcuator.GetBytesCount(x));
            return new MemoryChunkReference(chunk, memorySize);
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

                if (sizeCounter > _bufferSize)
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
            return new FileChunkReference(_chunkStorage, size, count);
        }

        public class MemoryChunkReference : IWritableChunkReference<T>
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
        }

        private class FileChunkReference : IWritableChunkReference<T>
        {
            private readonly IChunkStorage<T> _chunkStorage;

            public FileChunkReference(
                IChunkStorage<T> chunkStorage, 
                long size,
                int count)
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
                TotalSize += _chunkStorage.Push(new[] { value });       
            }
        }
    }
}
