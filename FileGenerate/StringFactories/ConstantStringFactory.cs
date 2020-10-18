using System;

namespace FileGenerate
{
    public class ConstantStringFactory : IRandomStringFactory
    {
        private readonly int _number;
        private readonly string _name;

        private const int MinStringLength = 4;
        private const int MaxStringLength = 1000000;

        public ConstantStringFactory(int number, string name)
        {
            _number = number;
            _name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public string Create()
        {
            return $"{_number}. {_name}";
        }

        public string Create(int length)
        {
            if (length < MinStringLength || length > MaxStringLength)
                throw new ArgumentOutOfRangeException(nameof(length));

            var @string = Create();
            while (@string.Length < length)
                @string += _name;
            if (@string.Length > length)
                @string = @string.Substring(0, length);
            return @string;
        }
    }
}
