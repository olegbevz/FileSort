using System;
using System.Collections.Generic;
using System.Linq;

namespace FileSort
{

    public class ChunkStack<T>
    {
        private readonly Stack<ChunkWrapper> _stack = new Stack<ChunkWrapper>();        
        private readonly ISizeCalculator<T> _sizeCalcuator;
        private readonly long _bufferSize;

        private long _currentSize;

        public ChunkStack(long bufferSize, ISizeCalculator<T> sizeCalculator)
        {
            _bufferSize = bufferSize;
            _sizeCalcuator = sizeCalculator;
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
                return _stack.Peek().Value.Length;
            }
        }

        public T[] Pop()
        {
            var chunk =  _stack.Pop();
            _currentSize -= chunk.Size;
            return chunk.Value;
        }

        public void Push(T[] chunk)
        {
            var chunkSize = chunk.Sum(x => _sizeCalcuator.GetBytesCount(x));

            if (_currentSize + chunkSize > _bufferSize)
                throw new OutOfMemoryException();

            _stack.Push(new ChunkWrapper { Value = chunk, Size = chunkSize });
            _currentSize += chunkSize;
        }

        private class ChunkWrapper
        {
            public long Size;
            public T[] Value;
        }
    }
}
