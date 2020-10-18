using FileSort.Core.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace FileSort.Core
{
    /// <summary>
    /// ConcurrentOppositeMergeQuickSort represents a concurrent version of
    /// OppositeMergeQuickSort algorithm
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ConcurrentOppositeMergeQuickSort<T> : MergeSortBase<T>, ISortMethod<T> where T : IComparable
    {
        private static readonly ILog _logger = LogProvider.GetCurrentClassLogger();

        private readonly int _chunkSize;
        private readonly int _concurrency;
        private readonly int _stackConcurrency;
        private readonly IChunkStackFactory<T> _chunkStackFactory;
        private readonly int _channelCapacity; 

        private int _concurrencyCounter;

        public ConcurrentOppositeMergeQuickSort(
            ChunkStack<T> chunkStack,
            IChunkStackFactory<T> chunkStackFactory,
            int channelCapacity = 2,
            int concurrency = 10,
            int chunkSize = 1000000,
            int stackConcurrency = 2)
            : base(chunkStack, null)
        {
            _chunkSize = chunkSize;
            _chunkStackFactory = chunkStackFactory;
            _channelCapacity = channelCapacity;
            _concurrency = concurrency;
            _stackConcurrency = stackConcurrency;
        }
        public IEnumerable<T> Sort(IEnumerable<T> source)
        {
            // Here we create channel for readen data chunks
            var readChannel = CreateReadChannel();

            // Here we create channel for sorted data chunks
            var sortChannel = CreateSortChannel();

            try
            {
                // Start the reading task
                var readTask = Task.Run(async () => await ReadChunks(source, readChannel.Writer));

                // Start several sorting tasks
                var sortTasks = new Task[_concurrency];
                for (int i = 0; i < _concurrency; i++)
                {
                    sortTasks[i] = Task.Run(async () => await SortChunks(readChannel.Reader, sortChannel.Writer));
                }

                // Start several merge taks
                var mergeTasks = new Task<IEnumerable<IChunkReference<T>>>[_stackConcurrency];
                for (int i = 0; i < _stackConcurrency; i++)
                {
                    mergeTasks[i] = Task.Run(async () => await PushChunksToStackAndMerge(sortChannel.Reader));
                }

                // Waiting for reading phase to finish
                readTask.Wait();

                // Waiting for all soring tasks to finish
                Task.WaitAll(sortTasks);

                sortChannel.Writer.Complete();

                // Waiting for merge tasks to finish
                Task.WaitAll(mergeTasks);

                // When all file patrs will be ready execute the final merge
                return ExecuteFinalMerge(mergeTasks);
            }
            catch (Exception ex)
            {
                readChannel.Writer.Complete(ex);
                sortChannel.Writer.Complete(ex);
                throw;
            }
        }

        private IChunkReference<T> ExecuteFinalMerge(Task<IEnumerable<IChunkReference<T>>>[] mergeTasks)
        {
            _logger.Info("Starting final merge");

            var stopwatch = Stopwatch.StartNew();

            var chunks = mergeTasks
                .Where(x => x.Result != null)
                .SelectMany(x => x.Result).ToArray();
            var chunk = _appender.Merge(chunks, _chunkStack);

            _logger.Info($"Final merge completed in {stopwatch.Elapsed}");
            stopwatch.Stop();
            return chunk;
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
            try
            {
                _logger.Info("Starting reading phase...");

                var stopwatch = Stopwatch.StartNew();
                var chunkStopwatch = Stopwatch.StartNew();

                var currentChunk = new List<T>();
                long currentChunkSize = 0;
                long currentChunkNumber = 0;

                foreach (var value in source)
                {
                    currentChunk.Add(value);
                    if (currentChunkSize < _chunkSize)
                    {
                        currentChunkSize++;
                    }
                    else
                    {
                        _logger.Debug($"Chunk {currentChunkNumber} with {currentChunk.Count} lines was readen in {chunkStopwatch.Elapsed}.");
                        chunkStopwatch.Restart();
                        await channelWriter.WriteAsync(currentChunk).ConfigureAwait(false);
                        currentChunk = new List<T>();
                        currentChunkSize = 0;
                        currentChunkNumber++;
                    }
                }

                _logger.Debug($"Chunk {currentChunkNumber} with {currentChunk.Count} lines was readen in {chunkStopwatch.Elapsed}.");
                await channelWriter.WriteAsync(currentChunk).ConfigureAwait(false);

                channelWriter.Complete();
                stopwatch.Stop();
                _logger.Info($"Reading phase is completed in {stopwatch.Elapsed}.");
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Error occured while reading: ", ex);
                channelWriter.Complete(ex);
                throw;
            }
        }        

        private async Task SortChunks(ChannelReader<List<T>> channelReader, ChannelWriter<List<T>> channelWriter)
        {
            try
            {
                int index = Interlocked.Increment(ref _concurrencyCounter);

                _logger.Info($"{index}. Started sorting phase...");

                var stopwatch = Stopwatch.StartNew();

                while (await channelReader.WaitToReadAsync().ConfigureAwait(false))
                {
                    if (channelReader.TryRead(out var chunk))
                    {
                        
                        _logger.Debug($"{index}. Started sorting chunk ...");
                        chunk.Sort();
                        _logger.Debug($"{index}. Chunk was sorted in {stopwatch.Elapsed}");
                        stopwatch.Restart();
                        await channelWriter.WriteAsync(chunk).ConfigureAwait(false);
                    }
                }

                _logger.Info($"{index}. Sorting phase is completed.");
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Error occured while sorting: ", ex);
                channelWriter.Complete(ex);
                throw;
            }
        }

        private async Task<IEnumerable<IChunkReference<T>>> PushChunksToStackAndMerge(ChannelReader<List<T>> channelReader)
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();
                var chunkStack = _chunkStackFactory.CreateChunkStack();
                var tempChunStack = _chunkStackFactory.CreateChunkStack();
                var appender = new ChunkStackAppender(chunkStack, tempChunStack);

                while (await channelReader.WaitToReadAsync().ConfigureAwait(false))
                {
                    if (channelReader.TryRead(out var chunk))
                    {
                        _logger.Debug($"Starting to merge chunk with {chunk.Count} lines");
                        appender.PushToStackRecursively(chunk);
                        _logger.Debug($"Chunk was merged in {stopwatch.Elapsed}");
                        stopwatch.Restart();
                    }
                }

                return chunkStack.ToArray().Concat(tempChunStack.ToArray());
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Error occured while merging: ", ex);
                throw;
            }
        }
    }
}
