using System.Collections.Generic;

namespace FileSort.Core
{
    public interface IChunkStorage<T>
    {
        IEnumerable<T> Pop(long size);
        long Push(IEnumerable<T> source);
        IChunkStorageWriter<T> GetWriter();
        void CopyTo(IChunkStorage<T> chunkStorage);
    }
}