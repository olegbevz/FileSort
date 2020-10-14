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
        private readonly int _chunkSize;
        private readonly ISortJoin<T> _sortJoin = new MergeSortJoin<T>();

        public OppositeMergeQuickSort(
            ChunkStack<T> chunkStack,
            int chunkSize = 1000000)
        {
            _chunkStack = chunkStack;
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
                    _chunkStack.Push(currentChunk);
                    currentChunk.Clear();
                    currentChunkSize = 0;
                }                
            }

            currentChunk.Sort();
            _chunkStack.Push(currentChunk);

            _logger.Info("Reading phase completed");
            _logger.Info("Starting final phase...");

            //var joinSize = 4;
            //IChunkReference<T>[] chunkBuffer = new IChunkReference<T>[joinSize];
            //while (_chunkStack.Count > 1)
            //{
            //    var size = Math.Min(joinSize, _chunkStack.Count);
            //    for (int i = 0; i < size; i++)
            //        chunkBuffer[size - 1 - i] = _chunkStack.Pop();

            //    var chunk = Merge(chunkBuffer, _chunkStack);
            //    _tempChunkStack.Push(chunk);
            //}

            //if (_tempChunkStack.Count > 1)
            //    return Merge(_tempChunkStack.ToArray(), _chunkStack);

            if (_chunkStack.Count > 1)
                return Merge(_chunkStack.ToArray(), _chunkStack);

            if (_chunkStack.Count == 1)
                return _chunkStack.Pop();

            return Array.Empty<T>();
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
