using System;
using System.Collections.Generic;

namespace FileSort.Core
{
    public class OppositeMergeSort<T> : MergeSortBase<T>, ISortMethod<T> where T : IComparable
    {
        private const int ChunkPairSize = 2;
        public OppositeMergeSort(ChunkStack<T> chunkStack, ChunkStack<T> tempChunkStack)
            : base(chunkStack, tempChunkStack)
        {
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

            return ExecuteFinalMerge();
        }
    }
}
