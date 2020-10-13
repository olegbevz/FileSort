using System;
using System.Collections.Generic;

namespace FileSort
{
    public class MergeSortJoin<T> : ISortJoin<T> where T : IComparable
    {
        public void Merge(T[] chunkPair)
        {
            if (chunkPair[0].CompareTo(chunkPair[1]) > 0)
            {
                T temp = chunkPair[0];
                chunkPair[0] = chunkPair[1];
                chunkPair[1] = temp;
            }
        }

        public void Merge(IEnumerable<T> left, IEnumerable<T> right, IWritableChunkReference<T> chunkWriter)
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
