using System;
using System.Collections.Generic;
using System.Linq;

namespace FileSort
{
    public class OppositeMergeSort<T> : ISortMethod<T> where T : IComparable
    {
        private const int ChunkPairSize = 2;

        private readonly ChunkStack<T> _chunkStack;
        private readonly ChunkStack<T> _tempChunkStack;
        private readonly ISortJoin<T> _sortJoin = new MergeSortJoin<T>();

        public OppositeMergeSort(ChunkStack<T> chunkStack, ChunkStack<T> tempChunkStack)
        {
            _chunkStack = chunkStack;
            _tempChunkStack = tempChunkStack;
        }

        public IEnumerable<T> Sort(IEnumerable<T> source)
        {
            var chunkPair = new T[ChunkPairSize];
            int chunkPairIndex = 0;

            foreach (var value in source)
            {
                chunkPair[chunkPairIndex] = value;
                chunkPairIndex++;

                if (chunkPairIndex == ChunkPairSize)
                {
                    _sortJoin.Join(chunkPair);
                    if (_chunkStack.LastChunkLength != chunkPair.Length)
                    {
                        _chunkStack.Push(chunkPair);
                    }
                    else
                    {
                        var chunkReference = _chunkStack.CreateChunk(chunkPair);
                        while (_chunkStack.LastChunkLength == chunkReference.Count)
                        {
                            chunkReference = Merge(chunkReference, _chunkStack.Pop(), _tempChunkStack);
                            var previousChunkLength = _chunkStack.LastChunkLength;
                            _chunkStack.Push(chunkReference);
                            if (previousChunkLength == chunkReference.Count)
                            {
                                chunkReference = _chunkStack.Pop();
                            }
                            else
                            {
                                break;
                            }
                        }
                    }

                    chunkPairIndex = 0;
                }
            }

            if (chunkPairIndex > 0 && chunkPairIndex < ChunkPairSize)
            {
                _chunkStack.Push(new T[] { chunkPair[0] });
            }

            while (_chunkStack.Count > 1)
            {
                return Merge(_chunkStack.ToArray(), _tempChunkStack);
                //var leftChunk = _chunkStack.Pop();
                //var chunkReference = Merge(leftChunk, _chunkStack.Pop(), _chunkStack);
                //if (_chunkStack.Count == 0)
                //    return chunkReference;

                //_chunkStack.Push(chunkReference);
            }

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
