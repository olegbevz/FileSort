using CommandLine;
using FileSort.Core;

namespace FileSort
{
    public class FileSortOptions
    {
        [Value(0, Required = true, HelpText = "Path to the source file")]
        public string InputFileName { get; set; }
        [Value(1, Required = true, HelpText = "Path to the target file")]
        public string OutputFileName { get; set; }
        [Option("file-buffer", Required = false, Default = "1MB", HelpText = "Size of FileStream internal buffer")]
        public string FileBuffer { get; set; }
        [Option("stream-buffer", Required = false, Default = "4KB", HelpText = "Size of StreamReader internal buffer")]
        public string StreamBuffer { get; set; }
        [Option("memory-buffer", Required = false, Default = "150MB", HelpText = "Size of memory buffer")]
        public string MemoryBuffer { get; set; }
        [Option("quick-sort-size", Required = false, HelpText = "Amount of records which will be quicksorted in memory before mergesort")]
        public int? QuickSortSize { get; set; }
        [Option("channel-capacity", Required = false, Default = 2, HelpText = "Capacity of channel in concurrent sorting method")]
        public int ChannelCapacity { get; set; }
        [Option("sort-concurrency", Required = false, Default = 10, HelpText = "Number of concurrent sorting operations for concurrent sorting method")]
        public int SortConcurrency { get; set; }
        [Option("merge-concurrency", Required = false, Default = 4, HelpText = "Number of concurrent merge operations for concurrent sorting method")]
        public int MergeConcurrency { get; set; }
        [Option("only-memory-merge", Required = false, Default = false, HelpText = "Specify in merge operations should be executed in only memory before the final merge. Option is effective for slow HDD drives")]
        public bool OnlyMemoryMerge { get; set; }
        [Option('s', "sort-method", Required = false, HelpText = "Sorting algorithm")]
        public SortMethod? SortMethod { get; set; }
    }
}
