using FileSort.Core;
using System.Text;

namespace FileSort
{
    public class FileLineSizeCalculator : ISizeCalculator<FileLine>
    {
        public long GetBytesCount(FileLine value)
        {
            return sizeof(int) + Encoding.Unicode.GetByteCount(value.Name);
        }
    }
}
