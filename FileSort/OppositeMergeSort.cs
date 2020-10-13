using System;
using System.Collections.Generic;

namespace FileSort
{
    public class OppositeMergeSort<T> where T : IComparable
    {
        private const int ChunkPairSize = 2;

        private readonly ChunkStack<T> _chunkStack;
        public OppositeMergeSort(ChunkStack<T> chunkStack)
        {
            _chunkStack = chunkStack;
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
                    Merge(chunkPair);
                    if (_chunkStack.LastChunkLength != chunkPair.Length)
                    {
                        _chunkStack.Push(chunkPair);
                    }
                    else
                    {
                        var chunkReference = _chunkStack.CreateChunk(chunkPair);
                        while (_chunkStack.LastChunkLength == chunkReference.Count)
                        {
                            chunkReference = Merge(chunkReference, _chunkStack.Pop(), _chunkStack);
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
                var leftChunk = _chunkStack.Pop();
                var chunkReference = Merge(leftChunk, _chunkStack.Pop(), _chunkStack);
                _chunkStack.Push(chunkReference);
            }

            if (_chunkStack.Count == 0)
                return _chunkStack.CreateChunk(new T[0]);

            return _chunkStack.Pop();
        }

        public static void Merge(T[] chunkPair)
        {
            if (chunkPair[0].CompareTo(chunkPair[1]) > 0)
            {
                T temp = chunkPair[0];
                chunkPair[0] = chunkPair[1];
                chunkPair[1] = temp;
            }
        }

        public static IWritableChunkReference<T> Merge(IChunkReference<T> left, IChunkReference<T> right, ChunkStack<T> chunkStack)
        {
            var chunkWriter = chunkStack.CreateChunkForMerge(left, right);
            Merge(left.GetValue(), right.GetValue(), chunkWriter);
            chunkWriter.Complete();
            return chunkWriter;
        }

        public static void Merge(IEnumerable<T> left, IEnumerable<T> right, IWritableChunkReference<T> chunkWriter)
        {
            using (var leftEnumerator = left.GetEnumerator())
            using (var rightEnumerator = right.GetEnumerator())
            {
                bool leftNotCompleted = leftEnumerator.MoveNext();
                bool rightNotCompleted = rightEnumerator.MoveNext();

                while (leftNotCompleted && rightNotCompleted)
                {
                    if (leftEnumerator.Current.CompareTo(rightEnumerator.Current) < 0)
                    {
                        chunkWriter.Write(leftEnumerator.Current);
                        leftNotCompleted = leftEnumerator.MoveNext();
                    }
                    else
                    {
                        chunkWriter.Write(rightEnumerator.Current);
                        rightNotCompleted = rightEnumerator.MoveNext();
                    }
                }

                if (leftNotCompleted)
                {
                    do
                    {
                        chunkWriter.Write(leftEnumerator.Current);
                    }
                    while (leftEnumerator.MoveNext());
                }

                if (rightNotCompleted)
                {
                    do
                    {
                        chunkWriter.Write(rightEnumerator.Current);
                    }
                    while (rightEnumerator.MoveNext());
                }
            }
        }
    }
}
