using System.Collections.Generic;

namespace FileSort.Core
{
    public interface IChunkStorage<T>
    {
        IEnumerable<T> OpenForRead(long size);
        IChunkStorageWriter<T> OpenForWrite();
        void CopyTo(IChunkStorage<T> chunkStorage);
    }
}