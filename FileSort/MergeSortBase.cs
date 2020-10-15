using System;
using System.Collections.Generic;
using System.Linq;

namespace FileSort
{
    public abstract class MergeSortBase<T> where T : IComparable
    {
        protected readonly ISortJoin<T> _sortJoin = new MergeSortJoin<T>();

        protected readonly ChunkStack<T> _chunkStack;
        protected readonly ChunkStack<T> _tempChunkStack;

        protected MergeSortBase(ChunkStack<T> chunkStack, ChunkStack<T> tempChunkStack)
        {
            _chunkStack = chunkStack;
            _tempChunkStack = tempChunkStack;
        }

        protected ChunkStack<T> GetOtherChunkStack(ChunkStack<T> chunkStack)
        {
            return chunkStack == _chunkStack ? _tempChunkStack : _chunkStack;
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
        }

        protected IChunkReference<T> Merge(
            IChunkReference<T> left,
            IChunkReference<T> right,
            ChunkStack<T> chunkStack)
        {
            var chunkWriter = chunkStack.CreateChunkForMerge(left, right);
            foreach (var value in _sortJoin.Join(left.GetValue(), right.GetValue()))
                chunkWriter.Write(value);
            chunkWriter.Complete();
            return chunkWriter;
        }

        protected IChunkReference<T> Merge(
            IChunkReference<T>[] chunks,
            ChunkStack<T> chunkStack)
        {
            var chunkWriter = chunkStack.CreateChunkForMerge(chunks);
            foreach (var value in _sortJoin.Join(chunks.Select(x => x.GetValue()).ToArray()))
                chunkWriter.Write(value);
            chunkWriter.Complete();
            return chunkWriter;
        }

        protected IEnumerable<T> ExecuteFinalMerge()
        {
            var currentChunkStack = _chunkStack;
            if (_tempChunkStack.Count > 0)
            {
                Merge(currentChunkStack.ToArray(), _tempChunkStack);
                currentChunkStack = _tempChunkStack;
            }

            if (currentChunkStack.Count > 1)
            {
                return Merge(currentChunkStack.ToArray(), GetOtherChunkStack(currentChunkStack));
            }

            if (currentChunkStack.Count == 1)
                return currentChunkStack.Pop();

            return Array.Empty<T>();
        }
    }
}
