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
            var chunk = new T[ChunkPairSize];
            int chunkIndex = 0;
            int targetIndex = 0;

            foreach (var number in source) 
            {
                chunk[chunkIndex] = number;
                chunkIndex++;
                targetIndex++;

                if (chunkIndex == ChunkPairSize)
                {
                    chunk = Merge(chunk[0], chunk[1]);
                    if (_chunkStack.LastChunkLength != chunk.Length)
                    {
                        _chunkStack.Push(chunk);
                    }
                    else
                    {
                        var chunkReference = _chunkStack.CreateChunk(chunk);
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

                    chunk = new T[ChunkPairSize];
                    chunkIndex = 0;
                }
            }

            if (chunkIndex > 0 && chunkIndex < ChunkPairSize)
            {
                _chunkStack.Push(new T[] { chunk[0] });
            }

            while (_chunkStack.Count > 1)
            {
                var leftChunk = _chunkStack.Pop();
                var chunkReference = Merge(leftChunk, _chunkStack.Pop(), _chunkStack);
                _chunkStack.Push(chunkReference);
            }

            if (_chunkStack.Count == 0)
                return new T[0];

            return _chunkStack.Pop().GetValue();
        }

        public static T[] Merge(T left, T right)
        {
            if (left.CompareTo(right) < 0)
                return new T[2] {left, right };

            return new T[2] { right, left };
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
