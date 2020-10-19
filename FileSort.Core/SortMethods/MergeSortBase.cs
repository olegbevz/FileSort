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

        protected MergeSortBase(ChunkStack<T> chunkStack, ChunkStack<T> tempChunkStack, bool onlyMemoryMerge)
        {
            _appender = new ChunkStackAppender(chunkStack, tempChunkStack, onlyMemoryMerge);
            _tempChunkStack = tempChunkStack;
            _chunkStack = chunkStack;
        }

        public class ChunkStackAppender
        {
            private readonly ISortJoin<T> _sortJoin = new MergeSortJoin<T>();
            private readonly bool _onlyMemoryMerge;

            protected readonly ChunkStack<T> _chunkStack;
            protected readonly ChunkStack<T> _tempChunkStack;
            public ChunkStackAppender(
                ChunkStack<T> chunkStack,
                ChunkStack<T> tempChunkStack,
                bool onlyMemoryMerge = false)
            {
                _chunkStack = chunkStack;
                _tempChunkStack = tempChunkStack;
                _onlyMemoryMerge = onlyMemoryMerge;
            }

            protected ChunkStack<T> GetOtherChunkStack(ChunkStack<T> chunkStack)
            {
                return chunkStack == _chunkStack ? _tempChunkStack : _chunkStack;
            }

            public void PushToStackRecursively(List<T> chunk)
            {
                if (_chunkStack.LastChunkLength != chunk.Count ||
                    (_onlyMemoryMerge && _chunkStack.LastChunkMemorySize == 0))
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
                if (_chunkStack.LastChunkLength != chunk.Length ||
                    (_onlyMemoryMerge && _chunkStack.LastChunkMemorySize == 0))
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
                while (currentStack.LastChunkLength == chunkReference.Count &&
                    (!_onlyMemoryMerge || (currentStack.LastChunkMemorySize > 0 && chunkReference.MemorySize > 0)))
                {
                    var previousChunkLength = otherStack.LastChunkLength;
                    var previousChunkMemorySize = otherStack.LastChunkMemorySize;
                    chunkReference = Merge(chunkReference, currentStack.Pop(), otherStack);

                    if (previousChunkLength == chunkReference.Count && 
                        (!_onlyMemoryMerge || (previousChunkMemorySize > 0 && chunkReference.MemorySize > 0)))
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
                var targetChunkStack = _chunkStack;
                if (_chunkStack.TotalSize < _tempChunkStack.TotalSize)
                    targetChunkStack = _tempChunkStack;

                var sourceChunkStack = GetOtherChunkStack(targetChunkStack);

                if (sourceChunkStack.Count > 0)
                    Merge(sourceChunkStack.ToArray(), targetChunkStack);

                if (targetChunkStack.Count > 1)
                {
                    return Merge(targetChunkStack.ToArray(), sourceChunkStack);
                }
                else if (targetChunkStack.Count == 1)
                {
                    return targetChunkStack.Pop();
                }

                return ChunkStack<T>.Empty;
            }
        }
    }
}
