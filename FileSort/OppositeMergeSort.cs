using System;
using System.Collections.Generic;
using System.Linq;

namespace FileSort
{
    public class OppositeMergeSort<T> where T : IComparable
    {
        private const int ChunkPairSize = 2;

        private readonly long _bufferSize;
        private readonly IChunkReaderWriter<T> _readerWriter;
        private readonly ISizeCalculator<T> _sizeCalculator;
        private readonly string _fileName;

        public OppositeMergeSort(long bufferSize, ISizeCalculator<T> sizeCalculator, IChunkReaderWriter<T> readerWriter, string fileName)
        {
            _bufferSize = bufferSize;
            _readerWriter = readerWriter;
            _sizeCalculator = sizeCalculator;
            _fileName = fileName;
        }

        public IEnumerable<T> Sort(IEnumerable<T> source)
        {
            var chunkStack = new ChunkStack<T>(_bufferSize, _sizeCalculator, _readerWriter, _fileName);
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
                    if (chunkStack.Count == 0 || chunkStack.LastChunkLength != chunk.Length)
                    {                        
                        chunkStack.Push(chunk);
                    }
                    else if (chunkStack.LastChunkLength == chunk.Length)
                    {
                        var chunkReference = chunkStack.CreateChunk(chunk);
                        while (chunkStack.Count > 0 && chunkStack.LastChunkLength == chunkReference.Count)
                        {
                            chunkReference = Merge(chunkStack.Pop(), chunkReference, chunkStack);
                            if (chunkStack.Count == 0 || chunkStack.LastChunkLength != chunkReference.Count)
                            {
                                chunkStack.Push(chunkReference);
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
                chunkStack.Push(new T[] { chunk[0] });
            }

            while (chunkStack.Count > 1)
            {
                var leftChunk = chunkStack.Pop();
                var chunkReference = Merge(leftChunk, chunkStack.Pop(), chunkStack);
                chunkStack.Push(chunkReference);
            }

            if (chunkStack.Count == 0)
                return new T[0];

            return chunkStack.Pop().Value;
        }

        public static T[] Merge(T left, T right)
        {
            if (left.CompareTo(right) < 0)
                return new T[2] {left, right };

            return new T[2] { right, left };
        }

        public static IChunkWriter<T> Merge(IChunkReference<T> left, IChunkReference<T> right, ChunkStack<T> chunkStack)
        {
            var chunkWriter = chunkStack.CreateChunkForMerge(left, right);
            Merge(left.Value, right.Value, chunkWriter);
            return chunkWriter;
        }

        public static void Merge(IEnumerable<T> left, IEnumerable<T> right, IChunkWriter<T> chunkWriter)
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
