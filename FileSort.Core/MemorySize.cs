using System;

namespace FileSort.Core
{
    public static class MemorySize
    {
        private static string[] _units = new string[] { "bytes", "KB", "MB", "GB" };

        public const long KB = 1024;
        public const long MB = KB * 1024;
        public const long GB = MB * 1024;

        public static long Parse(string size)
        {
            for (int i = 0; i < _units.Length; i++)
            {
                string unit = _units[i];
                if (size.EndsWith(unit, StringComparison.InvariantCultureIgnoreCase))
                {
                    var bytes = long.Parse(size.Substring(0, size.Length - unit.Length));
                    bytes = bytes * (long)Math.Pow(1024, i);
                    return bytes;
                }
            }

            return long.Parse(size);
        }
    }
}
