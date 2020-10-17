using System;
using System.Collections.Generic;

namespace FileSort.Core
{
    public interface IChunkStorageWriter<T> : IDisposable
    {
        void Write(IEnumerable<T> values);
        void Write(T value);
        long Complete();
    }
}
