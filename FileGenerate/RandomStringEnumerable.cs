using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace FileGenerate
{
    public class RandomStringEnumerable : IEnumerable<string>
    {
        private readonly long _targetSize;
        private readonly Encoding _targetEncoding;
        private readonly string _separator;
        private readonly IRandomStringFactory _stringFactory;

        public RandomStringEnumerable(
            long targetSize, 
            Encoding targetEncoding, 
            string separator, 
            IRandomStringFactory stringFactory)
        {
            _targetSize = targetSize;
            _targetEncoding = targetEncoding;
            _separator = separator;
            _stringFactory = stringFactory;
        }

        public IEnumerator<string> GetEnumerator()
        {
            return new RandomStringEnumerator(
                _targetSize, 
                _targetEncoding, 
                _separator, 
                _stringFactory);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private class RandomStringEnumerator : IEnumerator<string>
        {
            private const char Salt = 'x';

            private readonly int _minStringSize;
            private readonly long _targetSize;
            private readonly Encoding _targetEncoding;
            private readonly int _separatorSize;
            private readonly IRandomStringFactory _stringFactory;
            private readonly int _saltSize;

            private long _currentSize;

            public RandomStringEnumerator(
                long targetSize, 
                Encoding targetEncoding, 
                string separator, 
                IRandomStringFactory stringFactory)
            {
                _targetSize = targetSize;
                _targetEncoding = targetEncoding;
                _separatorSize = targetEncoding.GetByteCount(separator);
                _stringFactory = stringFactory;
                _minStringSize = _targetEncoding.GetByteCount($"{int.MaxValue}. {Salt}");
                _saltSize = targetEncoding.GetByteCount(Salt.ToString());

                if (_targetSize > 0 && _targetSize < _minStringSize)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(targetSize),
                        targetSize,
                        $"Target size should not be less than {_minStringSize}");
                }
            }

            public string Current { get; private set; }
            object IEnumerator.Current => this.Current;

            public void Dispose()
            {
                Reset();
            }

            public bool MoveNext()
            {
                if (_currentSize >= _targetSize)
                    return false;

                var current = _stringFactory.Create();
                var stringSize = _targetEncoding.GetByteCount(current);
                var nextSize = _currentSize + stringSize;

                var spaceLeft = _targetSize - nextSize - _separatorSize;
                if (spaceLeft > 0 && spaceLeft <= _minStringSize)
                {
                    var charsLeft = (int)(spaceLeft / _saltSize);
                    var stringBuilder = new StringBuilder(current);
                    for (int i = 0; i < charsLeft; i++)
                        stringBuilder.Append(Salt);
                    current = stringBuilder.ToString();
                    stringSize = _targetEncoding.GetByteCount(current);
                }
                else if (nextSize >= _targetSize)
                {
                    current = current.Substring(0, current.Length - (int)(nextSize - _targetSize + _separatorSize));
                    stringSize = _targetEncoding.GetByteCount(current);
                }

                _currentSize += stringSize;
                _currentSize += _separatorSize;
                Current = current;
                return true;
            }

            public void Reset()
            {
                _currentSize = 0;
            }
        }
    }
}
