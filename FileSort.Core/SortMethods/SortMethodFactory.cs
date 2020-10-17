using System;

namespace FileSort.Core
{
    public class SortMethodFactory : ISortMethodFactory
    {
        private readonly int _channelCapacity;
        private readonly int _concurrency;
        private readonly int? _quickSortSize;
        private readonly SortMethod _sortMethod;

        public SortMethodFactory(
            SortMethod sortMethod,
            int channelCapacity, 
            int concurrency, 
            int? quickSortSize)
        {
            _sortMethod = sortMethod;
            _channelCapacity = channelCapacity;
            _concurrency = concurrency;
            _quickSortSize = quickSortSize;
        }

        public ISortMethod<T> CreateSortMethod<T>(ChunkStack<T> chunkStack, ChunkStack<T> tempChunkStack) where T : IComparable
        {
            switch (_sortMethod)
            {
                case SortMethod.MergeSort:
                    {
                        return new OppositeMergeSort<T>(chunkStack, tempChunkStack);
                    }
                case SortMethod.MergeQuickSort:
                    {
                        if (_quickSortSize != null)
                            return new OppositeMergeQuickSort<T>(
                                chunkStack,
                                tempChunkStack, 
                                _quickSortSize.Value);

                        return new OppositeMergeQuickSort<T>(
                            chunkStack,
                            tempChunkStack);
                    }
                case SortMethod.ConcurrentMergeQuickSort:
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
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
