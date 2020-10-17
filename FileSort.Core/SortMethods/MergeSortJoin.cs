using System;
using System.Collections.Generic;

namespace FileSort.Core
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

        public IEnumerable<T> Join(IEnumerable<T> first, IEnumerable<T> second)
        {
            using (var firstEnumerator = first.GetEnumerator())
            using (var secondEnumerator = second.GetEnumerator())
            {
                bool firstActive = firstEnumerator.MoveNext();
                bool secondActive = secondEnumerator.MoveNext();

                while (firstActive && secondActive)
                {
                    if (firstEnumerator.Current.CompareTo(secondEnumerator.Current) < 0)
                    {
                        yield return firstEnumerator.Current;
                        firstActive = firstEnumerator.MoveNext();
                    }
                    else
                    {
                        yield return secondEnumerator.Current;
                        secondActive = secondEnumerator.MoveNext();
                    }
                }

                if (firstActive)
                {
                    do
                    {
                        yield return firstEnumerator.Current;
                    }
                    while (firstEnumerator.MoveNext());
                }

                if (secondActive)
                {
                    do
                    {
                        yield return secondEnumerator.Current;
                    }
                    while (secondEnumerator.MoveNext());
                }
            }
        }

        public IEnumerable<T> Join(IEnumerable<T>[] enumerables)
        {
            int count = enumerables.Length;
            var enumerators = new IEnumerator<T>[count];
            var completed = new bool[count];
            int activeCount = count;
            int startIndex = 0;
            int endIndex = count;

            try
            {

                for (int i = startIndex; i < endIndex; i++)
                {
                    var enumerator = enumerables[i].GetEnumerator();
                    enumerators[i] = enumerator;
                    if (!enumerator.MoveNext())
                    {
                        activeCount--;
                        completed[i] = true;
                        enumerator.Dispose();
                        if (startIndex == i) startIndex++;
                        if (endIndex == i + 1) endIndex--;
                    }
                }

                while (activeCount > 0)
                {
                    T minValue = default;
                    int minIndex = 0;
                    bool firstIteration = true;
                    for (int i = startIndex; i < endIndex; i++)
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
                        if (startIndex == minIndex) startIndex++;
                        if (endIndex == minIndex + 1) endIndex--;
                    }
                }
            }
            finally
            {
                if (activeCount > 0)
                {
                    for (int i = startIndex; i < endIndex; i++)
                    {
                        if (!completed[i]) enumerators[i].Dispose();
                    }
                }
            }
        }
    }
}
