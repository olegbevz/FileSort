using System;
using System.Collections.Generic;
using System.Linq;

namespace FileSort
{
    public class OppositeMergeSort<T> : ISortMethod<T> where T : IComparable
    {
        private const int ChunkPairSize = 2;

        private readonly ChunkStack<T> _chunkStack;
        private readonly ChunkStack<T> _tempChunkStack;
        private readonly ISortJoin<T> _sortJoin = new MergeSortJoin<T>();

        public OppositeMergeSort(ChunkStack<T> chunkStack, ChunkStack<T> tempChunkStack)
        {
            _chunkStack = chunkStack;
            _tempChunkStack = tempChunkStack;
        }

        public IEnumerable<T> Sort(IEnumerable<T> source)
        {
            var chunkPair = new T[ChunkPairSize];
            int chunkPairIndex = 0;

            foreach (var value in source)
            {
                chunkPair[chunkPairIndex] = value;
                chunkPairIndex++;

                if (chunkPairIndex == ChunkPairSize)
                {
                    _sortJoin.Join(chunkPair);
                    PushToStackRecursively(chunkPair);

                    chunkPairIndex = 0;
                }
            }

            if (chunkPairIndex > 0 && chunkPairIndex < ChunkPairSize)
            {
                _chunkStack.Push(new T[] { chunkPair[0] });
            }

            var currentChunkStack = _chunkStack;
            if (_tempChunkStack.Count > 0)
            {
                Merge(currentChunkStack.ToArray(), _tempChunkStack);
                currentChunkStack = _tempChunkStack;
            }

            while (currentChunkStack.Count > 1)
            {
                return Merge(currentChunkStack.ToArray(), _tempChunkStack);
                //var leftChunk = _chunkStack.Pop();
                //var chunkReference = Merge(leftChunk, _chunkStack.Pop(), _chunkStack);
                //if (_chunkStack.Count == 0)
                //    return chunkReference;

                //_chunkStack.Push(chunkReference);
            }

            if (currentChunkStack.Count == 1)
                return currentChunkStack.Pop();

            return Array.Empty<T>();
        }

        private ChunkStack<T> GetOtherChunkStack(ChunkStack<T> chunkStack)
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

        private IChunkReference<T> Merge(
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

        private IChunkReference<T> Merge(
            IChunkReference<T>[] chunks,
            ChunkStack<T> chunkStack)
        {
            var chunkWriter = chunkStack.CreateChunkForMerge(chunks);
            foreach (var value in _sortJoin.Join(chunks.Select(x => x.GetValue()).ToArray()))
                chunkWriter.Write(value);
            chunkWriter.Complete();
            return chunkWriter;
        }
    }
}
