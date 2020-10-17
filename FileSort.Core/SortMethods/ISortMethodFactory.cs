using System;

namespace FileSort.Core
{
    public interface ISortMethodFactory
    {
        ISortMethod<T> CreateSortMethod<T>(ChunkStack<T> chunkStack, ChunkStack<T> tempChunkStack) where T : IComparable;
    }
}