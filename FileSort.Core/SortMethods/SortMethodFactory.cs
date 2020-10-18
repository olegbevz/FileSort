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

        public ISortMethod<T> CreateSortMethod<T>(ChunkStack<T> chunkStack, IChunkStackFactory<T> chunkStackFactory) where T : IComparable
        {
            switch (_sortMethod)
            {
                case SortMethod.MergeSort:
                    {
                        return new OppositeMergeSort<T>(chunkStack, chunkStackFactory.CreateChunkStack());
                    }
                case SortMethod.MergeQuickSort:
                    {
                        if (_quickSortSize != null)
                            return new OppositeMergeQuickSort<T>(
                                chunkStack,
                                chunkStackFactory.CreateChunkStack(), 
                                _quickSortSize.Value);

                        return new OppositeMergeQuickSort<T>(
                            chunkStack,
                            chunkStackFactory.CreateChunkStack());
                    }
                case SortMethod.ConcurrentMergeQuickSort:
                    {
                        if (_quickSortSize != null)
                            return new ConcurrentOppositeMergeQuickSort<T>(
                                chunkStack,
                                chunkStackFactory,
                                _channelCapacity,
                                _concurrency,
                                _quickSortSize.Value);

                        return new ConcurrentOppositeMergeQuickSort<T>(
                            chunkStack,
                            chunkStackFactory,
                            _channelCapacity,
                            _concurrency);
                    }
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
