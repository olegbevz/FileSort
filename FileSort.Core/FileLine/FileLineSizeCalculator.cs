using System;

namespace FileSort.Core
{
    /// <summary>
    /// FileLineSizeCalculator represents memory size calculation logic for FileLine struct.
    /// For performance persposes FileLine struct already contains its size
    /// </summary>
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
