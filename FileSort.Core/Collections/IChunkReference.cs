using System.Collections.Generic;

namespace FileSort.Core
{
    /// <summary>
    /// Chunk reference represents abstract entry of ChunkStack
    /// It's data can be located in memory or on disk
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IChunkReference<T> : IEnumerable<T>
    {
        long MemorySize { get; }
        long TotalSize { get; }
        int Count { get; }
        IEnumerable<T> GetValue();
        void Flush(IChunkStorage<T> storage);
    }
}
