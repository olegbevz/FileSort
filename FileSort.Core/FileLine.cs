using System;

namespace FileSort.Core
{
    public struct FileLine : IComparable
    {
        public static FileLine None = new FileLine();
        public static FileLine Parse(string data)
        {
            if (TryParse(data, out var entry))
                return entry;

            throw new ArgumentException($"Failed to parse '{data}'");
        }

        public static bool TryParse(string data, out FileLine fileEntry)
        {
            fileEntry = None;
            var parts = data.Split('.');
            if (parts.Length != 2) return false;
            if (!int.TryParse(parts[0], out var number)) return false;
            var name = parts[1].TrimStart();
            if (string.IsNullOrEmpty(name)) return false;
            fileEntry = new FileLine(number, name);
            return true;
        }

        public FileLine(int number, string name)
        {
            Number = number;
            Name = name;
        }

        public int Number;

        public string Name;

        public int CompareTo(object obj)
        {
            if (!(obj is FileLine otherEntry)) return -1;

            if (Name == null) return otherEntry.Name != null ? 1 : 0;
            if (otherEntry.Name == null) return Name != null ? -1 : 0;

            var compareResult = Name.CompareTo(otherEntry.Name);
            if (compareResult != 0) return compareResult;

            return Number.CompareTo(otherEntry.Number);
        }

        public override string ToString()
        {
            return $"{Number}. {Name}";
        }
    }
}
