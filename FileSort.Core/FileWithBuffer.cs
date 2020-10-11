using System.IO;

namespace FileSort.Core
{
    public static class FileWithBuffer
    {
        public static FileStream OpenRead(string fileName, int bufferSize)
        {
            return new FileStream(
                fileName,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize);
        }

        public static FileStream OpenWrite(string fileName, int bufferSize)
        {
            return new FileStream(
                fileName,
                FileMode.Create,
                FileAccess.Write,
                FileShare.Write,
                bufferSize);
        }
    }

}
