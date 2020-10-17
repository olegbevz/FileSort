using System;

namespace FileSort
{
    public interface IChunkWriter<T> : IDisposable
    {
        void Write(T value);
        IChunkReference<T> Complete();
    }
}
