using log4net;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace FileSort
{
    public class ConcurrentOppositeMergeQuickSort<T> : ISortMethod<T> where T : IComparable
    {
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ChunkStack<T> _chunkStack;
        private readonly int _chunkSize;
        private readonly ISortJoin<T> _sortJoin = new MergeSortJoin<T>();

        public ConcurrentOppositeMergeQuickSort(
            ChunkStack<T> chunkStack,
            int chunkSize = 1000000)
        {
            _chunkStack = chunkStack;
            _chunkSize = chunkSize;
        }
        public IEnumerable<T> Sort(IEnumerable<T> source)
        {
            _logger.Info("Starting reading phase...");

            var counter = 0;

            foreach (var value in source)
            {
                counter++;
            }

            _logger.Info("Reading phase completed");

            return Array.Empty<T>();
        }
    }
}
