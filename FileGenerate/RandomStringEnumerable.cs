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
        private readonly IRandomStringFactory _stringFactory;

        public RandomStringEnumerable(
            long maxSize, 
            Encoding targetEncoding, 
            string lineSeparator, 
            IRandomStringFactory stringFactory)
        {
            _maxSize = maxSize;
            _targetEncoding = targetEncoding;
            _lineSeparator = lineSeparator;
            _stringFactory = stringFactory;
        }

        public IEnumerator<string> GetEnumerator()
        {
            return new RandomStringEnumerator(
                _maxSize, 
                _targetEncoding, 
                _lineSeparator, 
                _stringFactory);
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
            private readonly IRandomStringFactory _stringFactory;

            private long _currentSize;

            public RandomStringEnumerator(
                long maxSize, 
                Encoding targetEncoding, 
                string lineSeparator, 
                IRandomStringFactory stringFactory)
            {
                _maxSize = maxSize;
                _targetEncoding = targetEncoding;
                _lineSeparatorSize = targetEncoding.GetByteCount(lineSeparator);
                _stringFactory = stringFactory;
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

                Current = _stringFactory.Create();
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
