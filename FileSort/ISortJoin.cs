using System;
using System.Collections.Generic;

namespace FileSort
{
    public interface ISortJoin<T> where T : IComparable
    {
        void Merge(IEnumerable<T> left, IEnumerable<T> right, IWritableChunkReference<T> chunkWriter);
        void Merge(T[] chunkPair);
    }
}