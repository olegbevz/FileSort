using FileSort.Core;
using System;

namespace FileSort.Core
{
    public class SortMethodFactory : ISortMethodFactory
    {
        private readonly int _channelCapacity;
        private readonly int _concurrency;
        private readonly int? _quickSortSize;

        public SortMethodFactory(int channelCapacity, int concurrency, int? quickSortSize)
        {
            _channelCapacity = channelCapacity;
            _concurrency = concurrency;
            _quickSortSize = quickSortSize;
        }

        public ISortMethod<T> CreateSortMethod<T>(ChunkStack<T> chunkStack, ChunkStack<T> tempChunkStack) where T : IComparable
        {
            if (_quickSortSize != null)
                return new ConcurrentOppositeMergeQuickSort<T>(
                    chunkStack,
                    tempChunkStack,
                    _channelCapacity,
                    _concurrency,
                    _quickSortSize.Value);

            return new ConcurrentOppositeMergeQuickSort<T>(
                chunkStack,
                tempChunkStack,
                _channelCapacity,
                _concurrency);
        }
    }
}
