using FileSort.Core;
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
        private readonly Func<ChunkStack<T>> _chunkStackFactory;
        private readonly int _chunkSize;
        private readonly int _concurrency = 4;
        private readonly int _channelCapacity = 10; 

        private int _concurrencyCounter;
        private int _mergeConcurrecyCounter;

        public ConcurrentOppositeMergeQuickSort(
            ChunkStack<T> chunkStack,
            Func<ChunkStack<T>> chunkStackFactory,
            int chunkSize = 100000)
            : base(chunkStack, null)
        {
            _chunkStackFactory = chunkStackFactory;
            _chunkSize = chunkSize;
        }
        public IEnumerable<T> Sort(IEnumerable<T> source)
        {
            var readChannel = CreateReadChannel();
            var sortChannel = CreateSortChannel();

            var readTask = Task.Run(() => ReadChunks(source, readChannel.Writer));
            var sortTasks = new Task[_concurrency];
            for (int i = 0; i < _concurrency; i++)
            {
                sortTasks[i] = Task.Run(async () => await SortChunks(readChannel.Reader, sortChannel.Writer));
            }
            var mergeTasks = new Task<IChunkReference<T>>[_concurrency];
            for (int i = 0; i < _concurrency; i++)
            {
                mergeTasks[i] = Task.Run(async () => await MergeChunks(sortChannel.Reader));
            }

            readTask.Wait();
            Task.WaitAll(sortTasks);
            sortChannel.Writer.Complete();
            Task.WaitAll(mergeTasks);

            var chunks = mergeTasks.Select(x => x.Result).ToArray();
            return _appender.Merge(chunks, _chunkStack);

            return Array.Empty<T>();
        }

        private Channel<List<T>> CreateSortChannel()
        {
            return Channel.CreateBounded<List<T>>(new BoundedChannelOptions(_channelCapacity)
            {
                SingleWriter = false,
                SingleReader = false,
                AllowSynchronousContinuations = false
            });
        }

        private Channel<List<T>> CreateReadChannel()
        {
            return Channel.CreateBounded<List<T>>(new BoundedChannelOptions(_channelCapacity)
            {
                SingleWriter = true,
                SingleReader = false,
                AllowSynchronousContinuations = false
            });
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

        private async Task<IChunkReference<T>> MergeChunks(ChannelReader<List<T>> channelReader)
        {
            int index = Interlocked.Increment(ref _mergeConcurrecyCounter);

            _logger.Info($"{index} Started merging phase...");

            var chunkStack = _chunkStackFactory();
            var secondaryChunkStack = _chunkStackFactory();
            var appender = new ChunkStackAppender(chunkStack, secondaryChunkStack);

            while (await channelReader.WaitToReadAsync())
            {
                if (channelReader.TryRead(out var chunk))
                {
                    _logger.Debug($"{index} Received chunk for merge");
                    appender.PushToStackRecursively(chunk);
                    _logger.Debug($"{index} Chunk push to the stack");
                }
            }

            _logger.Info($"{index} Merging phase is completed.");

            return appender.ExecuteFinalMerge();            
        }
    }
}
