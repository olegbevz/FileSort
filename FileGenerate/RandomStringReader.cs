using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace FileGenerate
{
    public class RandomStringReader : IEnumerable<string>
    {
        private readonly long _targetSize;
        private readonly Encoding _targetEncoding;
        private readonly string _separator;
        private readonly IRandomStringFactory _stringFactory;

        public RandomStringReader(
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
            private readonly int _minStringSize;
            private readonly long _targetSize;
            private readonly Encoding _targetEncoding;
            private readonly int _separatorSize;
            private readonly IRandomStringFactory _stringFactory;

            private long _leftSize;

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
                _minStringSize = _targetEncoding.GetByteCount($"{int.MaxValue}. X");
                _leftSize = _targetSize;

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
                if (_leftSize <= 0)
                    return false;

                var current = _stringFactory.Create();
                var stringSize = _targetEncoding.GetByteCount(current);
                var nextLeftSize = _leftSize - stringSize - _separatorSize;

                if (nextLeftSize <= _minStringSize)
                {
                    current = _stringFactory.Create((int)(_leftSize - _separatorSize));
                    stringSize = _targetEncoding.GetByteCount(current);
                }

                _leftSize -= stringSize;
                _leftSize -= _separatorSize;
                Current = current;
                return true;
            }

            public void Reset()
            {
            }
        }
    }
}
