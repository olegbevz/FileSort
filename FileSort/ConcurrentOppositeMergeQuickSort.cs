using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace FileSort
{
    public class ConcurrentOppositeMergeQuickSort<T> : MergeSortBase<T>, ISortMethod<T> where T : IComparable
    {
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly int _chunkSize;
        private readonly int _concurrency = 4;

        private int _concurrencyCounter;

        public ConcurrentOppositeMergeQuickSort(
            ChunkStack<T> chunkStack,
            ChunkStack<T> tempChunkStack,
            int chunkSize = 100000)
            : base(chunkStack, tempChunkStack)
        {
            _chunkSize = chunkSize;
        }
        public IEnumerable<T> Sort(IEnumerable<T> source)
        {
            var readChannel = Channel.CreateUnbounded<List<T>>(new UnboundedChannelOptions { SingleWriter = true, SingleReader = false, AllowSynchronousContinuations = false });
            var sortChannel = Channel.CreateUnbounded<List<T>>(new UnboundedChannelOptions { SingleWriter = false, SingleReader = true, AllowSynchronousContinuations = false });

            var readTask = Task.Run(() => ReadChunks(source, readChannel.Writer));
            var sortTasks = new Task[_concurrency];
            for (int i = 0; i < _concurrency; i++)
            {
                sortTasks[i] = Task.Run(async () => await SortChunks(readChannel.Reader, sortChannel.Writer));
            }
            var mergeTask = Task.Run(async () => await MergeChunks(sortChannel.Reader));

            readTask.Wait();
            Task.WaitAll(sortTasks);
            sortChannel.Writer.Complete();
            mergeTask.Wait();

            return ExecuteFinalMerge();
        }

        private async Task ReadChunks(IEnumerable<T> source, ChannelWriter<List<T>> channelWriter)
        {
            _logger.Info("Starting reading phase...");

            var currentChunk = new List<T>();
            long currentChunkSize = 0;

            foreach (var value in source)
            {
                currentChunk.Add(value);
                if (currentChunkSize < _chunkSize)
                {
                    currentChunkSize++;
                }
                else
                {
                    _logger.Debug($"Chunk readen from input source");
                    await channelWriter.WriteAsync(currentChunk);
                    _logger.Debug($"Chunk pushed to channel");
                    currentChunk = new List<T>();
                    currentChunkSize = 0;
                }
            }

            _logger.Debug($"Chunk readen from input source");
            await channelWriter.WriteAsync(currentChunk);
            _logger.Debug($"Chunk pushed to channel");

            channelWriter.Complete();

            _logger.Info("Reading phase is completed.");
        }        

        private async Task SortChunks(ChannelReader<List<T>> channelReader, ChannelWriter<List<T>> channelWriter)
        {
            int index = Interlocked.Increment(ref _concurrencyCounter);

            _logger.Info($"{index}. Started sorting phase...");

            while (await channelReader.WaitToReadAsync())
            {
                if (channelReader.TryRead(out var chunk))
                {
                    _logger.Debug($"{index}. Chunk sorting");
                    chunk.Sort();
                    _logger.Debug($"{index}. Chunk sorted");
                    await channelWriter.WriteAsync(chunk);
                }
            }

            _logger.Info($"{index}. Sorting phase is completed.");
        }

        private async Task MergeChunks(ChannelReader<List<T>> channelReader)
        {
            _logger.Info("Started merging phase...");

            while (await channelReader.WaitToReadAsync())
            {
                if (channelReader.TryRead(out var chunk))
                {
                    _logger.Info("Chunk push to the stack");
                    PushToStackRecursively(chunk);
                }
            }

            _logger.Info("Merging phase is completed.");
        }
    }
}
