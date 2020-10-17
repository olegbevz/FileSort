using FileSort.Core;
using System.IO;
using System.Threading;

namespace FileSort
{
    public class FileSort
    {
        private readonly int _fileBufferSize;
        private readonly int _streamBuffer;
        private readonly long _memoryBufferSize;
        public FileSort(int fileBufferSize, int streamBuffer, long memoryBufferSize)
        {
            _fileBufferSize = fileBufferSize;
            _memoryBufferSize = memoryBufferSize;
            _streamBuffer = streamBuffer;
        }

        public void Sort(string inputFileName, string outputFileName)
        {
            using (var fileStream = FileWithBuffer.OpenRead(inputFileName, _fileBufferSize))
            {
                var fileSize = fileStream.Length;
                var readerWriter = new FileLineReaderWriter(_streamBuffer);

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
                    targetChunkStorage);

                var tempChunkStack = new ChunkStack<FileLine>(
                    _memoryBufferSize,
                    new FileLineSizeCalculator(),
                    tempChunkStorage);

                //var sorter = new OppositeMergeQuickSort<FileLine>(chunkStack, tempChunkStack);

                var sorter = new ConcurrentOppositeMergeQuickSort<FileLine>(chunkStack, tempChunkStack);

                var inputFileLines = new FileLineReader(fileStream, _streamBuffer);
                var sortedCollection = sorter.Sort(inputFileLines);
                if (sortedCollection is IChunkReference<FileLine> chunkReference)
                    chunkReference.Flush(targetChunkStorage);
            }
        }

        private int tempChunkCounter = 0;

        public ChunkStack<FileLine> CreateTempChunckStack(string outputFileName)
        {
            int index = Interlocked.Increment(ref tempChunkCounter);

            var tempFileName = Path.Combine(
                    Path.GetDirectoryName(outputFileName),
                    $"{Path.GetFileNameWithoutExtension(outputFileName)}_part{index}{Path.GetExtension(outputFileName)}");

            var tempChunkStorage = new ChunkFileStorage<FileLine>(
                tempFileName,
                _fileBufferSize,
                new FileLineReaderWriter(_streamBuffer));

            return new ChunkStack<FileLine>(
                _memoryBufferSize,
                new FileLineSizeCalculator(),
                tempChunkStorage);
        }
    }
}
