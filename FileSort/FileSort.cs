using FileSort.Core;
using System.IO;

namespace FileSort
{
    public class FileSort
    {
        private readonly int _fileBufferSize;
        private readonly long _memoryBufferSize;

        public FileSort(int fileBufferSize, long memoryBufferSize)
        {
            _fileBufferSize = fileBufferSize;
            _memoryBufferSize = memoryBufferSize;
            
        }

        public void Sort(string inputFileName, string outputFileName)
        {
            using (var fileStream = FileWithBuffer.OpenRead(inputFileName, _fileBufferSize))
            {
                var fileSize = fileStream.Length;
                var readerWriter = new FileLineReaderWriter();

                var targetChunkStorage = new ChunkFileStorage<FileLine>(
                    outputFileName, 
                    _fileBufferSize,
                    readerWriter);

                var tempFileName = Path.Combine(
                    Path.GetDirectoryName(outputFileName),
                    $"{Path.GetFileNameWithoutExtension(outputFileName)}_temp{Path.GetExtension(outputFileName)}");

                var tempChunkStorage = new ChunkFileStorage<FileLine>(
                    tempFileName,
                    _fileBufferSize,
                    readerWriter);

                var chunkStack = new ChunkStack<FileLine>(
                    _memoryBufferSize,
                    new FileLineSizeCalculator(),
                    targetChunkStorage,
                    tempChunkStorage);

                var sorter = new OppositeMergeSort<FileLine>(chunkStack);

                var inputFileLines = new FileLineReader(fileStream);
                var sortedCollection = sorter.Sort(inputFileLines);
                if (sortedCollection is IChunkReference<FileLine> chunkReference)
                    chunkReference.Flush();
            }
        }
    }
}
