﻿using FileSort.Core.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace FileSort.Core
{
    public class ConcurrentOppositeMergeQuickSort<T> : MergeSortBase<T>, ISortMethod<T> where T : IComparable
    {
        private static readonly ILog _logger = LogProvider.GetCurrentClassLogger();

        private readonly int _chunkSize;
        private readonly int _concurrency;
        private readonly int _channelCapacity; 

        private int _concurrencyCounter;

        public ConcurrentOppositeMergeQuickSort(
            ChunkStack<T> chunkStack,
            ChunkStack<T> tempChunkStack,
            int channelCapacity = 10,
            int concurrency = 10,
            int chunkSize = 1000000)
            : base(chunkStack, tempChunkStack)
        {
            _chunkSize = chunkSize;
            _channelCapacity = channelCapacity;
            _concurrency = concurrency;
        }
        public IEnumerable<T> Sort(IEnumerable<T> source)
        {
            var readChannel = CreateReadChannel();
            var sortChannel = CreateSortChannel();


            var readTask = Task.Run(async () => await ReadChunks(source, readChannel.Writer));
            var sortTasks = new Task[_concurrency];
            for (int i = 0; i < _concurrency; i++)
            {
                sortTasks[i] = Task.Run(async () => await SortChunks(readChannel.Reader, sortChannel.Writer));
            }
            var mergeTask = Task.Run(async () => await PushChunksToStackAndMerge(sortChannel.Reader).ConfigureAwait(false));

            readTask.Wait();
            Task.WaitAll(sortTasks);
            sortChannel.Writer.Complete();
            mergeTask.Wait();
            return mergeTask.Result;
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

                _logger.Debug($"Chunk readen from input source");
                await channelWriter.WriteAsync(currentChunk).ConfigureAwait(false);
                _logger.Debug($"Chunk pushed to channel");

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

        private async Task<IChunkReference<T>> PushChunksToStackAndMerge(ChannelReader<List<T>> channelReader)
        {
            try
            {
                var stopwatch = Stopwatch.StartNew();
                while (await channelReader.WaitToReadAsync().ConfigureAwait(false))
                {
                    if (channelReader.TryRead(out var chunk))
                    {
                        _logger.Debug($"Starting to merge chunk with {chunk.Count}");
                        PushToStackRecursively(chunk);
                        _logger.Debug($"Chunk was merged in {stopwatch.Elapsed}");
                        stopwatch.Restart();
                    }
                }

                if (_chunkStack.Count == 0)
                    return null;

                _logger.Info("Starting final merge");

                return _appender.ExecuteFinalMerge();
            }
            catch (Exception ex)
            {
                _logger.ErrorException("Error occured while merging: ", ex);
                throw;
            }
        }
    }
}
