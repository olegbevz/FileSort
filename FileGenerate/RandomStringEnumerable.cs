using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace FileGenerate
{
    public class RandomStringEnumerable : IEnumerable<string>
    {
        private readonly long _maxSize;
        private readonly Encoding _targetEncoding;
        private readonly string _lineSeparator;

        public RandomStringEnumerable(long maxSize, Encoding targetEncoding, string lineSeparator)
        {
            _maxSize = maxSize;
            _targetEncoding = targetEncoding;
            _lineSeparator = lineSeparator;
        }

        public IEnumerator<string> GetEnumerator()
        {
            return new RandomStringEnumerator(_maxSize, _targetEncoding, _lineSeparator);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private class RandomStringEnumerator : IEnumerator<string>
        {
            private readonly long _maxSize;
            private readonly Encoding _targetEncoding;
            private readonly int _lineSeparatorSize;
            private long _currentSize;

            public RandomStringEnumerator(long maxSize, Encoding targetEncoding, string lineSeparator)
            {
                _maxSize = maxSize;
                _targetEncoding = targetEncoding;
                _lineSeparatorSize = targetEncoding.GetByteCount(lineSeparator);
            }

            public string Current { get; private set; }
            object IEnumerator.Current => this.Current;

            public void Dispose()
            {
                Reset();
            }

            public bool MoveNext()
            {
                if (_currentSize >= _maxSize)
                    return false;

                Current = "2. Banana is yellow";
                var stringSize = _targetEncoding.GetByteCount(Current);
                _currentSize += stringSize;
                _currentSize += _lineSeparatorSize;
                return true;
            }

            public void Reset()
            {
                _currentSize = 0;
            }
        }
    }
}
