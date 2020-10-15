using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FileSort
{
    public class OppositeMergeQuickSort<T> : ISortMethod<T> where T : IComparable
    {
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly ChunkStack<T> _chunkStack;
        private readonly ChunkStack<T> _tempChunkStack;
        private readonly int _chunkSize;
        private readonly ISortJoin<T> _sortJoin = new MergeSortJoin<T>();

        public OppositeMergeQuickSort(
            ChunkStack<T> chunkStack,
            ChunkStack<T> tempChunkStack,
            int chunkSize = 1000000)
        {
            _chunkStack = chunkStack;
            _tempChunkStack = tempChunkStack;
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
            _logger.Info("Starting final phase...");

            var currentChunkStack = _chunkStack;
            if (_tempChunkStack.Count > 0)
            {
                Merge(currentChunkStack.ToArray(), _tempChunkStack);
                currentChunkStack = _tempChunkStack;
            }

            if (currentChunkStack.Count > 1)
                return Merge(currentChunkStack.ToArray(), _tempChunkStack);

            if (currentChunkStack.Count == 1)
                return _chunkStack.Pop();

            return Array.Empty<T>();
        }

        private ChunkStack<T> GetOtherChunkStack(ChunkStack<T> chunkStack)
        {
            return chunkStack == _chunkStack ? _tempChunkStack : _chunkStack;
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

        private IChunkReference<T> Merge(
            IChunkReference<T> left,
            IChunkReference<T> right,
            ChunkStack<T> chunkStack)
        {
            var chunkWriter = chunkStack.CreateChunkForMerge(left, right);
            foreach (var value in _sortJoin.Join(left.GetValue(), right.GetValue()))
                chunkWriter.Write(value);
            chunkWriter.Complete();
            return chunkWriter;
        }

        private IChunkReference<T> Merge(
            IChunkReference<T>[] chunks,
            ChunkStack<T> chunkStack)
        {
            var chunkWriter = chunkStack.CreateChunkForMerge(chunks);
            foreach (var value in _sortJoin.Join(chunks.Select(x => x.GetValue()).ToArray()))
                chunkWriter.Write(value);
            chunkWriter.Complete();
            return chunkWriter;
        }
    }
}
