using System;
using System.Collections.Generic;

namespace FileSort.Core
{
    /// <summary>
    /// OppositeMergeSort represents classic merge sort bottom up algorithm
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class OppositeMergeSort<T> : MergeSortBase<T>, ISortMethod<T> where T : IComparable
    {
        private const int ChunkPairSize = 2;
        public OppositeMergeSort(ChunkStack<T> chunkStack, ChunkStack<T> tempChunkStack)
            : base(chunkStack, tempChunkStack)
        {
        }

        public IEnumerable<T> Sort(IEnumerable<T> source)
        {
            // Here we sequentally collect pairs of data 
            // while reading input source
            var chunkPair = new T[ChunkPairSize];
            int chunkPairIndex = 0;

            foreach (var value in source)
            {
                chunkPair[chunkPairIndex] = value;
                chunkPairIndex++;

                if (chunkPairIndex == ChunkPairSize)
                {
                    // When the pair is ready we sort it and push 
                    // to the stack
                    _sortJoin.Join(chunkPair);
                    _appender.PushToStackRecursively(chunkPair);

                    chunkPairIndex = 0;
                }
            }

            // If after all data is readen single item is left 
            // we push it to the stack
            if (chunkPairIndex > 0 && chunkPairIndex < ChunkPairSize)
            {
                _chunkStack.Push(new T[] { chunkPair[0] });
            }

            return _appender.ExecuteFinalMerge();
        }
    }
}
