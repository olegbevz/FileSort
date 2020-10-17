using System.Collections.Generic;
using System.IO;

namespace FileSort.Core
{
    public interface IChunkReaderWriter<T>
    {
        void WriteToStream(StreamWriter streamWriter, IEnumerable<T> source);
        IEnumerable<T> ReadFromStream(Stream stream);
    }
}
