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
                bool leftActive = leftEnumerator.MoveNext();
                bool rightActive = rightEnumerator.MoveNext();

                while (leftActive && rightActive)
                {
                    if (leftEnumerator.Current.CompareTo(rightEnumerator.Current) < 0)
                    {
                        yield return leftEnumerator.Current;
                        leftActive = leftEnumerator.MoveNext();
                    }
                    else
                    {
                        yield return rightEnumerator.Current;
                        rightActive = rightEnumerator.MoveNext();
                    }
                }

                if (leftActive)
                {
                    do
                    {
                        yield return leftEnumerator.Current;
                    }
                    while (leftEnumerator.MoveNext());
                }

                if (rightActive)
                {
                    do
                    {
                        yield return rightEnumerator.Current;
                    }
                    while (rightEnumerator.MoveNext());
                }
            }
        }

        public IEnumerable<T> Join(IEnumerable<T>[] enumerables)
        {
            int count = enumerables.Length;
            var enumerators = new IEnumerator<T>[count];
            var completed = new bool[count];
            int activeCount = count;

            for (int i = 0; i < count; i++)
            {
                var enumerator = enumerables[i].GetEnumerator();
                enumerators[i] = enumerator;
                if (!enumerator.MoveNext())
                {
                    activeCount--;
                    completed[i] = true;
                    enumerator.Dispose();
                }
            }

            while (activeCount > 0)
            {
                T minValue = default;
                int minIndex = 0;
                bool firstIteration = true;
                for (int i = 0; i < count; i++)
                {
                    if (completed[i]) continue;

                    var current = enumerators[i].Current;
                    if (firstIteration)
                    {
                        minValue = current;
                        minIndex = i;
                        firstIteration = false;
                    }
                    else if (current.CompareTo(minValue) < 0)
                    {
                        minValue = current;
                        minIndex = i;
                    }
                }

                yield return minValue;

                var enumerator = enumerators[minIndex];
                if (!enumerator.MoveNext())
                {
                    activeCount--;
                    completed[minIndex] = true;
                    enumerator.Dispose();
                }
            }
        }
    }
}
