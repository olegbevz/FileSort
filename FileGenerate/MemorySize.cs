using System;

namespace FileGenerate
{
    public class MemorySize
    {
        private static string[] _units = new string[] { "bytes", "KB", "MB", "GB" };

        private readonly long _size;

        public static MemorySize Parse(string size)
        {
            for (int i = 0; i < _units.Length; i++)
            {
                string unit = _units[i];
                if (size.EndsWith(unit, StringComparison.InvariantCultureIgnoreCase))
                {
                    var bytes = long.Parse(size.Substring(0, size.Length - unit.Length));
                    bytes = bytes * (long)Math.Pow(1024, i);
                    return new MemorySize(bytes);
                }
            }

            return new MemorySize(long.Parse(size));
        }

        public MemorySize(long size)
        {
            _size = size;
        }

        public long GetTotalBytes()
        {
            return _size;
        }
    }
}
