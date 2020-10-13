using System;
using System.IO;

namespace FileSort.Core
{
    public struct FileLine : IComparable
    {
        private static int _maxNumberLength = int.MaxValue.ToString().Length;
        private const int _maxNumberDivTen = int.MaxValue / 10;
        private const int _maxNumberModTen = int.MaxValue % 10;

        public static FileLine None = new FileLine();
        public static FileLine Parse(string data)
        {
            if (TryParse(data, out var entry))
                return entry;

            throw new ArgumentException($"Failed to parse line '{data}'.");
        }

        public static FileLine Parse(StreamReader streamReader)
        {
            if (TryParse(streamReader, out var entry))
                return entry;

            throw new ArgumentException($"Failed to parse stream line.");
        }

        public static bool TryParse(StreamReader streamReader, out FileLine fileLine)
        {
            fileLine = None;
            char current;
            int size = 0;
            bool readNumber = false;
            int number = 0;

            if (streamReader.EndOfStream)
                return false;

            while (!streamReader.EndOfStream && ((current = (char)streamReader.Read()) != '.') && size <= _maxNumberLength)
            {
                if (char.IsWhiteSpace(current) && !readNumber) continue;
                if (!char.IsNumber(current)) return false;

                readNumber = true;
                int numberPart = (current - '0');
                if (number == _maxNumberDivTen && numberPart > _maxNumberModTen)
                    return false;

                number = number * 10 + numberPart;
                size++;
            }

            if (streamReader.EndOfStream)
                return false;

            var name = streamReader.ReadLine().Trim();
            if (string.IsNullOrEmpty(name))
                return false;

            fileLine = new FileLine(number, name, sizeof(int) + name.Length);

            return true;
        }

        public static bool TryParse(string data, out FileLine fileLine)
        {
            fileLine = None;
            if (string.IsNullOrEmpty(data)) return false;
            var parts = data.Split('.');
            if (parts.Length != 2) return false;
            if (!int.TryParse(parts[0], out var number)) return false;
            var name = parts[1].Trim();
            if (string.IsNullOrEmpty(name)) return false;
            fileLine = new FileLine(number, name, sizeof(int) + name.Length);
            return true;
        }

        public FileLine(int number, string name, long size)
        {
            Number = number;
            Name = name;
            Size = size;
        }

        public int Number;
        public string Name;
        public long Size;

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
