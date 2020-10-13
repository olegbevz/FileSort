using FileSort.Core;
using System;

namespace FileSort
{
    public class FileLineSizeCalculator : ISizeCalculator<FileLine>
    {
        public long GetBytesCount(FileLine value)
        {
            if (value.Size == 0)
                throw new ArgumentException("FileLine size was not calculated");

            return value.Size;
        }
    }
}
