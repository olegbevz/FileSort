using FileSort.Core;
using System.IO;

namespace FileSort
{

    public class FileSort
    {
        private readonly int _fileBufferSize;
        private readonly int _streamBuffer;
        private readonly long _memoryBufferSize;
        private readonly ISortMethodFactory _sortMethodFactory;
        public FileSort(
            int fileBufferSize, 
            int streamBuffer, 
            long memoryBufferSize,
            ISortMethodFactory sortMethodFactory)
        {
            _fileBufferSize = fileBufferSize;
            _memoryBufferSize = memoryBufferSize;
            _sortMethodFactory = sortMethodFactory;
            _streamBuffer = streamBuffer;
        }

        public void Sort(string inputFileName, string outputFileName)
        {
            using (var fileStream = FileWithBuffer.OpenRead(inputFileName, _fileBufferSize))
            using (var targetChunkStorage = CreateTargetFileStorage(outputFileName))
            using (var tempChunkStorage = CreateTempFileStorage(outputFileName))
            {
                var fileSize = fileStream.Length;

                var chunkStack = new ChunkStack<FileLine>(
                    _memoryBufferSize,
                    new FileLineSizeCalculator(),
                    targetChunkStorage);

                var tempChunkStack = new ChunkStack<FileLine>(
                    _memoryBufferSize,
                    new FileLineSizeCalculator(),
                    tempChunkStorage);

                var sortMethod = _sortMethodFactory.CreateSortMethod(chunkStack, tempChunkStack);

                var inputFileLines = new FileLineReader(fileStream, _streamBuffer);
                var sortedCollection = sortMethod.Sort(inputFileLines);
                if (sortedCollection is IChunkReference<FileLine> chunkReference)
                    chunkReference.Flush(targetChunkStorage);
            }
        }

        private ChunkFileStorage<FileLine> CreateTempFileStorage(
            string outputFileName)
        {
            var tempFileName = Path.Combine(
                Path.GetDirectoryName(outputFileName),
                $"{Path.GetFileNameWithoutExtension(outputFileName)}_temp{Path.GetExtension(outputFileName)}");

            return new ChunkFileStorage<FileLine>(
                tempFileName,
                _fileBufferSize,
                new FileLineReaderWriter(_streamBuffer),
                true);
        }

        private ChunkFileStorage<FileLine> CreateTargetFileStorage(            
            string outputFileName)
        {
            return new ChunkFileStorage<FileLine>(
                outputFileName,
                _fileBufferSize,
                new FileLineReaderWriter(_streamBuffer));
        }
    }
}
