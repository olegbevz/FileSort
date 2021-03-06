﻿using System;

namespace FileSort.Core
{
    public interface IChunkWriter<T> : IDisposable
    {
        void Write(T value);
        IChunkReference<T> Complete();
    }
}
