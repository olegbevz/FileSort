using System.Collections.Generic;

namespace FileSort.Core
{
    /// <summary>
    /// Some abstract external chunks storage
    /// </summary>
    /// <typeparam name="T">Type of data</typeparam>
    public interface IChunkStorage<T>
    {
        IEnumerable<T> OpenForRead(long size);
        IChunkStorageWriter<T> OpenForWrite();
        void CopyTo(IChunkStorage<T> chunkStorage);
    }
}