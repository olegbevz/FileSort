namespace FileSort
{
    public class ConstantSizeCalculator<T> : ISizeCalculator<T>
    {
        private readonly long _size;

        public ConstantSizeCalculator(long size)
        {
            _size = size;
        }

        public long GetBytesCount(T value)
        {
            return _size;
        }
    }
}
