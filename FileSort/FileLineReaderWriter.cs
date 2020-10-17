using FileSort.Core;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FileSort
{
    public class FileLineReaderWriter : IChunkReaderWriter<FileLine>
    {
        public IEnumerable<FileLine> ReadFromStream(Stream stream)
        {
            return new FileLineReader(stream);
        }

        public void WriteToStream(StreamWriter streamWriter, IEnumerable<FileLine> source)
        {
            foreach (var line in source)
                line.WriteTo(streamWriter);
        }
    }
}
