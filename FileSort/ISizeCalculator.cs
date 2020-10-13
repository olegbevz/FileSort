namespace FileSort
{
    public interface ISizeCalculator<T>
    {
        long GetBytesCount(T value);
    }
}
