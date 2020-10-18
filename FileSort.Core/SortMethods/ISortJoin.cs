using System;
using System.Collections.Generic;

namespace FileSort.Core
{
    /// <summary>
    /// ISortJoin reperesents logic of merging sorted sources of data.
    /// Iput could be an array with pair of elements, two or many enumerations.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ISortJoin<T> where T : IComparable
    {
        void Join(T[] chunkPair);
        IEnumerable<T> Join(IEnumerable<T> left, IEnumerable<T> right);

        IEnumerable<T> Join(params IEnumerable<T>[] enumerables);
    }
}