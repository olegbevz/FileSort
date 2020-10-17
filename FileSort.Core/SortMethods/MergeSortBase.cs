using System;
using System.Collections.Generic;
using System.Linq;

namespace FileSort.Core
{
    public abstract class MergeSortBase<T> where T : IComparable
    {
        protected readonly ISortJoin<T> _sortJoin = new MergeSortJoin<T>();

        protected readonly ChunkStack<T> _chunkStack;
        protected readonly ChunkStack<T> _tempChunkStack;

        protected readonly ChunkStackAppender _appender;

        protected MergeSortBase(ChunkStack<T> chunkStack, ChunkStack<T> tempChunkStack)
        {
            _appender = new ChunkStackAppender(chunkStack, tempChunkStack);
            _tempChunkStack = tempChunkStack;
            _chunkStack = chunkStack;
        }

        public void PushToStackRecursively(List<T> chunk)
        {
            _appender.PushToStackRecursively(chunk);
        }

        public void PushToStackRecursively(T[] chunk)
        {
            _appender.PushToStackRecursively(chunk);
        }

        protected IEnumerable<T> ExecuteFinalMerge()
        {
            return _appender.ExecuteFinalMerge();
        }

        protected class ChunkStackAppender
        {
            protected readonly ISortJoin<T> _sortJoin = new MergeSortJoin<T>();

            protected readonly ChunkStack<T> _chunkStack;
            protected readonly ChunkStack<T> _tempChunkStack;
            public ChunkStackAppender(ChunkStack<T> chunkStack, ChunkStack<T> tempChunkStack)
            {
                _chunkStack = chunkStack;
                _tempChunkStack = tempChunkStack;
            }

            protected ChunkStack<T> GetOtherChunkStack(ChunkStack<T> chunkStack)
            {
                return chunkStack == _chunkStack ? _tempChunkStack : _chunkStack;
            }

            public void PushToStackRecursively(List<T> chunk)
            {
                if (_chunkStack.LastChunkLength != chunk.Count)
                {
                    _chunkStack.Push(chunk);
                }
                else
                {
                    var chunkReference = _chunkStack.CreateChunk(chunk);
                    PushToStackRecursively(chunkReference);
                }
            }

            public void PushToStackRecursively(T[] chunk)
            {
                if (_chunkStack.LastChunkLength != chunk.Length)
                {
                    _chunkStack.Push(chunk);
                }
                else
                {
                    var chunkReference = _chunkStack.CreateChunk(chunk);
                    PushToStackRecursively(chunkReference);
                }
            }

            protected void PushToStackRecursively(IChunkReference<T> chunkReference)
            {
                var currentStack = _chunkStack;
                var otherStack = GetOtherChunkStack(_chunkStack);
                while (currentStack.LastChunkLength == chunkReference.Count)
                {
                    var previousChunkLength = otherStack.LastChunkLength;
                    chunkReference = Merge(chunkReference, currentStack.Pop(), otherStack);

                    if (previousChunkLength == chunkReference.Count)
                    {
                        currentStack = otherStack;
                        otherStack = GetOtherChunkStack(currentStack);
                        chunkReference = currentStack.Pop();
                    }
                }
            }

            protected IChunkReference<T> Merge(
                IChunkReference<T> left,
                IChunkReference<T> right,
                ChunkStack<T> chunkStack)
            {
                using (var chunkWriter = chunkStack.CreateChunkForMerge(left, right))
                {
                    foreach (var value in _sortJoin.Join(left.GetValue(), right.GetValue()))
                        chunkWriter.Write(value);
                    return chunkWriter.Complete();
                }
            }

            public IChunkReference<T> Merge(
                IChunkReference<T>[] chunks,
                ChunkStack<T> chunkStack)
            {
                using (var chunkWriter = chunkStack.CreateChunkForMerge(chunks))
                {

                    foreach (var value in _sortJoin.Join(chunks.Select(x => x.GetValue()).ToArray()))
                    {
                        chunkWriter.Write(value);
                    }

                    return chunkWriter.Complete();
                }
            }

            public IChunkReference<T> ExecuteFinalMerge()
            {
                var currentChunkStack = _chunkStack;
                
                if (_tempChunkStack.Count > 0 && _chunkStack.TotalSize > _tempChunkStack.TotalSize)
                {
                    Merge(_tempChunkStack.ToArray(), currentChunkStack);                    
                }

                if (_chunkStack.TotalSize < _tempChunkStack.TotalSize && _chunkStack.Count > 0)
                {
                    Merge(_chunkStack.ToArray(), _tempChunkStack);
                    currentChunkStack = _tempChunkStack;
                }

                if (currentChunkStack.Count > 1)
                {
                    return Merge(currentChunkStack.ToArray(), GetOtherChunkStack(currentChunkStack));
                }

                if (currentChunkStack.Count == 1)
                    return currentChunkStack.Pop();

                return ChunkStack<T>.Empty;
            }
        }
    }
}
