using log4net;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace FileSort
{
    public class OppositeMergeQuickSort<T> : MergeSortBase<T>, ISortMethod<T> where T : IComparable
    {
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

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
            _chunkStack.Push(currentChunk);

            _logger.Info("Reading phase completed");
            _logger.Info("Starting final merge phase...");

            return ExecuteFinalMerge();
        }

        public void PushToStackRecursively(List<T> chunk)
        {
            if (_chunkStack.LastChunkLength != chunk.Count)
            {
                _chunkStack.Push(chunk);
            }
            else
            {
                var chunkReference = _chunkStack.CreateChunk(chunk);
                var currentStack = _chunkStack;
                var otherStack = GetOtherChunkStack(_chunkStack);
                while (currentStack.LastChunkLength == chunkReference.Count)
                {
                    var previousChunkLength = otherStack.LastChunkLength;
                    chunkReference = Merge(chunkReference, currentStack.Pop(), otherStack);

                    if (previousChunkLength == chunkReference.Count)
                    {
                        currentStack = otherStack;
                        otherStack = GetOtherChunkStack(currentStack);
                        chunkReference = currentStack.Pop();
                    }
                }
            }
        }
    }
}
