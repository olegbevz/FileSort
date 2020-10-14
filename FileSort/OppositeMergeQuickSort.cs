using FileSort.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileSort
{
    public class OppositeMergeQuickSort<T> : ISortMethod<T> where T : IComparable
    {
        private readonly ChunkStack<T> _chunkStack;
        private readonly long _chunkMaxSize = 1000000;
        private readonly ISortJoin<T> _sortJoin = new MergeSortJoin<T>();
        private readonly ISizeCalculator<T> _sizeCalculator;

        public OppositeMergeQuickSort(ChunkStack<T> chunkStack, ISizeCalculator<T> sizeCalculator)
        {
            _chunkStack = chunkStack;
            _sizeCalculator = sizeCalculator;
        }
        public IEnumerable<T> Sort(IEnumerable<T> source)
        {
            var currentChunk = new List<T>();
            long currentChunkSize = 0;

            foreach (var value in source)
            {
                currentChunk.Add(value);
                if (currentChunkSize < _chunkMaxSize)
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

            if (_chunkStack.Count > 1)
            {
                return Merge(_chunkStack.ToArray(), _chunkStack);
            }

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
