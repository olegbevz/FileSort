using FileSort.Core.Logging;
using System;
using System.Collections.Generic;

namespace FileSort.Core
{
    public class OppositeMergeQuickSort<T> : MergeSortBase<T>, ISortMethod<T> where T : IComparable
    {
        private static readonly ILog _logger = LogProvider.GetCurrentClassLogger();

        private readonly int _chunkSize;

        public OppositeMergeQuickSort(ChunkStack<T> chunkStack, ChunkStack<T> tempChunkStack, int chunkSize = 1000000)
            : base(chunkStack, tempChunkStack)
        {
            _chunkSize = chunkSize;
        }

        public IEnumerable<T> Sort(IEnumerable<T> source)
        {
            var currentChunk = new List<T>();
            long currentChunkSize = 0;

            _logger.Info("Starting reading phase...");

            foreach (var value in source)
            {
                currentChunk.Add(value);
                if (currentChunkSize < _chunkSize)
                {                   
                    currentChunkSize++;
                }
                else
                {
                    currentChunk.Sort();
                    PushToStackRecursively(currentChunk);
                    currentChunk.Clear();
                    currentChunkSize = 0;
                }                
            }

            currentChunk.Sort();
            PushToStackRecursively(currentChunk);

            _logger.Info("Reading phase completed");
            _logger.Info("Starting final merge phase...");

            return ExecuteFinalMerge();
        }        
    }
}
