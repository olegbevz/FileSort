using System.Collections.Generic;

namespace FileSort
{
    public interface IChunkReference<T> : IEnumerable<T>
    {
        long MemorySize { get; }
        long TotalSize { get; }
        int Count { get; }
        IEnumerable<T> GetValue();
        void Flush();
    }
}
