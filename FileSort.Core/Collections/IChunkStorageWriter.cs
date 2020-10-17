namespace FileSort.Core
{
    public interface IChunkStorageWriter<T>
    {
        void Write(T value);
        long Complete();
    }
}
