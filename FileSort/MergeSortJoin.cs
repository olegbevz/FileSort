using System;
using System.Collections.Generic;

namespace FileSort
{
    public class MergeSortJoin<T> : ISortJoin<T> where T : IComparable
    {
        public void Join(T[] chunkPair)
        {
            if (chunkPair[0].CompareTo(chunkPair[1]) > 0)
            {
                T temp = chunkPair[0];
                chunkPair[0] = chunkPair[1];
                chunkPair[1] = temp;
            }
        }

        public IEnumerable<T> Join(IEnumerable<T> left, IEnumerable<T> right)
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
                        yield return leftEnumerator.Current;
                        leftNotCompleted = leftEnumerator.MoveNext();
                    }
                    else
                    {
                        yield return rightEnumerator.Current;
                        rightNotCompleted = rightEnumerator.MoveNext();
                    }
                }

                if (leftNotCompleted)
                {
                    do
                    {
                        yield return leftEnumerator.Current;
                    }
                    while (leftEnumerator.MoveNext());
                }

                if (rightNotCompleted)
                {
                    do
                    {
                        yield return rightEnumerator.Current;
                    }
                    while (rightEnumerator.MoveNext());
                }
            }
        }
    }
}
