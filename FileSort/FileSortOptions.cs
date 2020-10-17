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
        [Option("memory-buffer", Required = false, Default = "500MB", HelpText = "Size of memory buffer")]
        public string MemoryBuffer { get; set; }
        [Option("quick-sort-size", Required = false, HelpText = "Amount of records which will be quicksorted in memory before mergesort")]
        public int? QuickSortSize { get; set; }
        [Option("channel-capacity", Required = false, Default = 10, HelpText = "Capacity of channel in concurrent sorting method")]
        public int ChannelCapacity { get; set; }
        [Option("concurrency", Required = false, Default = 10, HelpText = "Number of concurrent sorting operations in concurrent sorting methods")]
        public int Concurrency { get; set; }
        [Option('s', "sort-method", Required = false, Default = SortMethod.ConcurrentMergeQuickSort, HelpText = "Sorting algorithm")]
        public SortMethod SortMethod { get; set; }
    }
}
