namespace FileSort
{
    public interface IWritableChunkReference<T> : IChunkReference<T>
    {
        void Write(T value);
    }
}
