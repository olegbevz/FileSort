using FileSort.Core.Logging;
using System;
using System.Collections.Generic;

namespace FileSort.Core
{
    /// <summary>
    /// OppositeMergeQuickSort represents a combination of quicksort and merge bottom-up sort
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class OppositeMergeQuickSort<T> : MergeSortBase<T>, ISortMethod<T> where T : IComparable
    {
        private static readonly ILog _logger = LogProvider.GetCurrentClassLogger();

        private readonly int _chunkSize;

        public OppositeMergeQuickSort(
            ChunkStack<T> chunkStack, 
            ChunkStack<T> tempChunkStack, 
            int chunkSize = 1000000,
            bool onlyMemoryMerge = false)
            : base(chunkStack, tempChunkStack, onlyMemoryMerge)
        {
            _chunkSize = chunkSize;
        }

        public IEnumerable<T> Sort(IEnumerable<T> source)
        {
            var currentChunk = new List<T>();
            long currentChunkSize = 0;

            _logger.Info("Starting reading phase...");

            // Here we sequentally collect an array of data
            foreach (var value in source)
            {
                currentChunk.Add(value);
                if (currentChunkSize < _chunkSize)
                {                   
                    currentChunkSize++;
                }
                else
                {
                    // When array is collected we quicksort it and
                    // push it to the stack recursively
                    currentChunk.Sort();
                    _appender.PushToStackRecursively(currentChunk);
                    currentChunk.Clear();
                    currentChunkSize = 0;
                }                
            }

            // When all data is readen we quicksort the last data chunk
            // and push it to the stack
            currentChunk.Sort();
            _appender.PushToStackRecursively(currentChunk);

            _logger.Info("Reading phase completed");
            _logger.Info("Starting final merge phase...");

            return _appender.ExecuteFinalMerge();
        }        
    }
}
