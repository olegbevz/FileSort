using FileSort.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace FileSort
{

    public class FileSort
    {
        private readonly int _fileBuffer;
        private readonly int _streamBuffer;
        private readonly long _memoryBuffer;
        private readonly ISortMethodFactory _sortMethodFactory;
        public FileSort(
            int fileBufferSize, 
            int streamBuffer, 
            long memoryBufferSize,
            ISortMethodFactory sortMethodFactory)
        {
            _fileBuffer = fileBufferSize;
            _memoryBuffer = memoryBufferSize;
            _sortMethodFactory = sortMethodFactory;
            _streamBuffer = streamBuffer;
        }

        public void Sort(string inputFileName, string outputFileName)
        {
            using (var fileStream = FileWithBuffer.OpenRead(inputFileName, _fileBuffer))
            using (var targetChunkStorage = CreateTargetFileStorage(outputFileName))
            using (var tempChunkStackFactory = new TempChunkStackFactory(outputFileName, _fileBuffer, _streamBuffer, _memoryBuffer))
            {
                var fileSize = fileStream.Length;

                var chunkStack = new ChunkStack<FileLine>(
                    _memoryBuffer,
                    new FileLineSizeCalculator(),
                    targetChunkStorage);

                var sortMethod = _sortMethodFactory.CreateSortMethod(chunkStack, tempChunkStackFactory);

                var inputFileLines = new FileLineReader(fileStream, _streamBuffer);
                var sortedCollection = sortMethod.Sort(inputFileLines);
                if (sortedCollection is IChunkReference<FileLine> chunkReference)
                    chunkReference.Flush(targetChunkStorage);
            }
        }

        private ChunkFileStorage<FileLine> CreateTargetFileStorage(            
            string outputFileName)
        {
            return new ChunkFileStorage<FileLine>(
                outputFileName,
                _fileBuffer,
                new FileLineReaderWriter(_streamBuffer));
        }

        private class TempChunkStackFactory : IChunkStackFactory<FileLine>, IDisposable
        {
            private readonly string _fileName;
            private readonly int _fileBuffer;
            private readonly int _streamBuffer;
            private readonly long _memoryBuffer;

            private readonly HashSet<string> _tempFileNames = new HashSet<string>();
            private int _partCounter = 0;

            public TempChunkStackFactory(string fileName, int fileBuffer, int streamBuffer, long memoryBuffer)
            {
                _fileName = fileName;
                _fileBuffer = fileBuffer;
                _streamBuffer = streamBuffer;
                _memoryBuffer = memoryBuffer;
            }

            public ChunkStack<FileLine> CreateChunkStack()
            {
                Interlocked.Increment(ref _partCounter);

                var tempFileName = Path.Combine(
                    Path.GetDirectoryName(_fileName),
                    $"{Path.GetFileNameWithoutExtension(_fileName)}_temp{_partCounter}{Path.GetExtension(_fileName)}");

                _tempFileNames.Add(tempFileName);

                var storage = new ChunkFileStorage<FileLine>(
                    tempFileName,
                    _fileBuffer,
                    new FileLineReaderWriter(_streamBuffer),
                    true);

                return new ChunkStack<FileLine>(
                    _memoryBuffer,
                    new FileLineSizeCalculator(),
                    storage);
            }

            public void Dispose()
            {
                foreach (var tempFileName in _tempFileNames)
                {
                    if (File.Exists(tempFileName))
                        File.Delete(tempFileName);
                }
            }
        }
    }
}
