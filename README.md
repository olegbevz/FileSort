# FileSort utilities

**FileSort**

FileSort is a simple command line tool for sorting files.
Depending on size of a file different sorting methods could be applied:
- **OppositeMergeSort** is a classic implementation of bottom-up merge sort.
Algorithm is not so fast comparing to quick sort and is effective only for small files less than 1KB.
- **OppositeMergeQuickSort** represents combination of quick and merge sort: chunks of lines are sorted in memory by quicksort and then merged together via mergesort.
Algorithm is faster than the previous one but still not fast for files larger than 10MB.
Size of a chunk is configurable via command line. 
- **ConcurrentOppositeMergeQuickSort** represents a concurrent version of OppositeMergeQuickSort algorithm: chunks of lines are processed in separate threads.
Number of concurrent operations is configurable via command line.

*Command line example:*

FileSort.exe 1GB.txt 1GB_sorted.txt

*Command line description:*

FileSort.exe
  - --file-buffer         (Default: 1MB) Size of FileStream internal buffer
  - --stream-buffer       (Default: 4KB) Size of StreamReader internal buffer
  - --memory-buffer       (Default: 250MB) Size of memory buffer
  - --quick-sort-size     Amount of records which will be quicksorted in memory before mergesort
  - --channel-capacity    (Default: 2) Capacity of channel in concurrent sorting method
  - --concurrency         (Default: 10) Number of concurrent sorting operations in concurrent sorting methods
  - -s, --sort-method     Sorting algorithm
  - --help                Display this help screen.
  - --version             Display version information.
  - value pos. 0          Required. Path to the source file
  - value pos. 1          Required. Path to the target file

*!WARNING! For large files sort a large amount of memory is availble by default. 
If you run out of memory you can descrease memory limit by setting '--memory-buffer', '--channel-capacity'  values.*

**FileGenerate**

FileGenerate is a command line tool for random files generaion.
For generation of random lines following modes are available: 
- constant (constant string), 
- sequence (simple sequental words selection with some fixed step), 
- random (generation using Random class), 
- bogus (generation with Bogus library), 
- autofixture (generation with AutoFixture library)

*Command line example:*

FileGenerate.exe 10MB.txt -s 10MB

**FileCheck**

FileCheck is a simple utility for line order check or file format check

*Command line example:*

FileCheck.exe 10MB.txt

*Command line description:*

FileCheck.exe
  - --file-buffer      (Default: 1MB) Size of FileStream internal buffer
  - --stream-buffer    (Default: 4KB) Size of StreamReader internal buffer
  - --check-format     Check only file format, do not check lines order
  - --help             Display this help screen.
  - --version          Display version information.
  - value pos. 0       Required. Path to the sorted file that need to be checked
