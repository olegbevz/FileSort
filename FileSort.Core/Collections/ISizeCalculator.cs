namespace FileSort.Core
{
    public interface ISizeCalculator<T>
    {
        long GetBytesCount(T value);
    }
}
