using System;
using System.Collections.Generic;

namespace FileSort
{
    public interface ISortJoin<T> where T : IComparable
    {
        IEnumerable<T> Join(IEnumerable<T> left, IEnumerable<T> right);
        void Join(T[] chunkPair);
    }
}