using FileSort.Core.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace FileSort.Core
{
    public interface IChunkStackFactory<T>
    {
        ChunkStack<T> CreateChunkStack();
    }
}
