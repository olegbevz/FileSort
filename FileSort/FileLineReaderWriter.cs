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
            return new StreamEnumerable(stream).Select(FileLine.Parse);
        }

        public void WriteToStream(Stream stream, IEnumerable<FileLine> source)
        {
            using (var streamWriter = new StreamWriter(stream, Encoding.UTF8, (int)MemorySize.KB * 4, true))
            {
                foreach (var line in source)
                    streamWriter.WriteLine(line.ToString());
            }
        }
    }
}
