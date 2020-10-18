using System;

namespace FileSort.Core
{
    /// <summary>
    /// SmartSortMethodFactory represents factory for a sort nethod logic
    /// SortMethod here is determined based on a file size
    /// </summary>
    public class SmartSortMethodFactory : SortMethodFactory
    {
        public SmartSortMethodFactory(long fileSize, int channelCapacity, int concurrency, int? quickSortSize)
            : base(CalculateSortMethod(fileSize), channelCapacity, concurrency, quickSortSize)
        {
        }

        private static SortMethod CalculateSortMethod(long fileSize)
        {
            if (fileSize <= MemorySize.KB)
            {
                return SortMethod.MergeSort;
            }
            else if (fileSize <= 10 * MemorySize.MB)
            {
                return SortMethod.MergeQuickSort;
            }
            else
            {
                return SortMethod.ConcurrentMergeQuickSort;
            }
        }
    }
}
