namespace FileSort
{
    public interface IChunkStorageWriter<T>
    {
        void Write(T value);
        long Complete();
    }
}
