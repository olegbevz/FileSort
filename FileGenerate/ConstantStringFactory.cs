using System;

namespace FileGenerate
{
    public class ConstantStringFactory : IRandomStringFactory
    {
        private readonly string _value;

        public ConstantStringFactory(string value)
        {
            _value = value ?? throw new ArgumentNullException(nameof(value));
        }

        public string Create()
        {
            return _value;
        }
    }
}
