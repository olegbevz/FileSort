namespace FileGenerate
{
    public interface IRandomStringFactory
    {
        string Create();
        string Create(int length);
    }
}
