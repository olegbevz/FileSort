using System;
using System.Collections.Generic;

namespace FileSort.Core
{
    public interface ISortJoin<T> where T : IComparable
    {
        void Join(T[] chunkPair);
        IEnumerable<T> Join(IEnumerable<T> left, IEnumerable<T> right);

        IEnumerable<T> Join(params IEnumerable<T>[] enumerables);
    }
}