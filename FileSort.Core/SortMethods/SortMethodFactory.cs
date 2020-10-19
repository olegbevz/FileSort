using System;

namespace FileSort.Core
{
    /// <summary>
    /// SortMethodFactory represents factory for a sort nethod logic
    /// </summary>
    public class SortMethodFactory : ISortMethodFactory
    {
        private readonly int _channelCapacity;
        private readonly int _sortConcurrency;
        private readonly int _mergeConcurrency;
        private readonly int? _quickSortSize;
        private readonly bool _onlyMemoryMerge;
        private readonly SortMethod _sortMethod;

        public SortMethodFactory(
            SortMethod sortMethod,
            int channelCapacity, 
            int sortConcurrency, 
            int mergeConcurrency,
            bool onlyMemoryMerge,
            int? quickSortSize)
        {
            _sortMethod = sortMethod;
            _channelCapacity = channelCapacity;
            _sortConcurrency = sortConcurrency;
            _mergeConcurrency = mergeConcurrency;
            _onlyMemoryMerge = onlyMemoryMerge;
            _quickSortSize = quickSortSize;
        }

        public ISortMethod<T> CreateSortMethod<T>(ChunkStack<T> chunkStack, IChunkStackFactory<T> chunkStackFactory) where T : IComparable
        {
            switch (_sortMethod)
            {
                case SortMethod.MergeSort:
                    {
                        return new OppositeMergeSort<T>(
                            chunkStack, 
                            chunkStackFactory.CreateChunkStack(), 
                            _onlyMemoryMerge);
                    }
                case SortMethod.MergeQuickSort:
                    {
                        if (_quickSortSize != null)
                            return new OppositeMergeQuickSort<T>(
                                chunkStack,
                                chunkStackFactory.CreateChunkStack(), 
                                _quickSortSize.Value,
                                _onlyMemoryMerge);

                        return new OppositeMergeQuickSort<T>(
                            chunkStack,
                            chunkStackFactory.CreateChunkStack(),
                            onlyMemoryMerge: _onlyMemoryMerge);
                    }
                case SortMethod.ConcurrentMergeQuickSort:
                    {
                        if (_quickSortSize != null)
                            return new ConcurrentOppositeMergeQuickSort<T>(
                                chunkStack,
                                chunkStackFactory,
                                _channelCapacity,
                                _sortConcurrency,
                                _mergeConcurrency,
                                _quickSortSize.Value,
                                _onlyMemoryMerge);

                        return new ConcurrentOppositeMergeQuickSort<T>(
                            chunkStack,
                            chunkStackFactory,
                            _channelCapacity,
                            _sortConcurrency,
                            _mergeConcurrency,
                            onlyMemoryMerge: _onlyMemoryMerge);
                    }
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
