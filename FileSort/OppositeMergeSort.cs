using System;
using System.Collections.Generic;
using System.Linq;

namespace FileSort
{
    public class OppositeMergeSort<T> where T : IComparable
    {
        private readonly long _bufferSize;
        private readonly ISizeCalculator<T> _sizeCalculator;

        public OppositeMergeSort(long bufferSize, ISizeCalculator<T> sizeCalculator)
        {
            _bufferSize = bufferSize;
            _sizeCalculator = sizeCalculator;
        }

        public T[] Sort(IEnumerable<T> source)
        {
            var chunkStack = new ChunkStack<T>(_bufferSize, _sizeCalculator);
            int chunkSize = 2;
            var chunk = new T[chunkSize];
            int chunkIndex = 0;
            int targetIndex = 0;

            foreach (var number in source) 
            {
                chunk[chunkIndex] = number;
                chunkIndex++;
                targetIndex++;

                if (chunkIndex == chunkSize)
                {
                    if (chunkStack.Count == 0 || chunkStack.LastChunkLength != chunk.Length)
                    {
                        Merger(chunk, 0, (chunk.Length / 2) - 1, chunk.Length - 1);
                        chunkStack.Push(chunk);
                    }
                    else if (chunkStack.LastChunkLength == chunk.Length)
                    {
                        Merger(chunk, 0, (chunk.Length / 2) - 1, chunk.Length - 1);

                        while (chunkStack.Count > 0 && chunkStack.LastChunkLength == chunk.Length)
                        {
                            chunk = chunkStack.Pop().Concat(chunk).ToArray();
                            Merger(chunk, 0, (chunk.Length / 2) - 1, chunk.Length - 1);
                            if (chunkStack.Count == 0 || chunkStack.LastChunkLength != chunk.Length)
                            {
                                chunkStack.Push(chunk);
                                break;
                            }
                        }
                    }

                    chunk = new T[chunkSize];
                    chunkIndex = 0;
                }
            }

            if (chunkIndex > 0 && chunkIndex < chunkSize)
            {
                chunkStack.Push(new T[] { chunk[0] });
            }

            while (chunkStack.Count > 1)
            {
                var leftChunk = chunkStack.Pop();
                chunk = leftChunk.Concat(chunkStack.Pop()).ToArray();
                Merger(chunk, 0, leftChunk.Length - 1, chunk.Length - 1);
                chunkStack.Push(chunk);
            }

            if (chunkStack.Count == 0)
                return new T[0];

            return chunkStack.Pop();
        }

        private static void Merger<T>(T[] arr, int start, int mid, int end) where T : IComparable
        {
            T[] temp = new T[end - start + 1];

            int i = start;
            int j = mid + 1;
            int k = 0;

            while (i < mid + 1 && j < end + 1)
            {
                if (arr[i].CompareTo(arr[j]) < 0)
                {
                    temp[k] = arr[i];
                    i++;
                    k++;
                }
                else
                {
                    temp[k] = arr[j];
                    j++;
                    k++;
                }
            }
            //fill in the rest
            while (i <= mid)
            {
                temp[k] = arr[i];
                i++;
                k++;

            }
            while (j <= end)
            {
                temp[k] = arr[j];
                j++;
                k++;
            }
            //now make array the sorted version
            i = start;
            k = 0;
            while (k < temp.Length && i <= end)
            {
                arr[i] = temp[k];
                k++;
                i++;
            }
        }
    }
}
