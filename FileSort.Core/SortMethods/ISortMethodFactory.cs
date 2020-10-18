using System;

namespace FileSort.Core
{
    public interface ISortMethodFactory
    {
        ISortMethod<T> CreateSortMethod<T>(ChunkStack<T> chunkStack, IChunkStackFactory<T> chunkStackFactory) where T : IComparable;
    }
}