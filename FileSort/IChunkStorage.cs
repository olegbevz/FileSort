using System.Collections.Generic;

namespace FileSort
{
    public interface IChunkStorage<T>
    {
        IEnumerable<T> Pop(long size);
        long Push(IEnumerable<T> source);
    }
}