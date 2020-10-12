using FileSort.Core;
using NUnit.Framework;
using System;
using System.Linq;

namespace FileSort.UnitTests
{
    [TestFixture]
    public class OppositeMergeSortTests
    {
        private readonly OppositeMergeSort<int> _sorter = new OppositeMergeSort<int>(new ChunkStack<int>(
            100 * MemorySize.MB,
            new ConstantSizeCalculator<int>(sizeof(int)),
            null,
            null));

        [TestCase]
        public void ShouldSortSimpleNumberArray()
        {
            var sourceArray = new int[] { 6, 4, 5, 8, 7, 9, 2, 3, 1 };
            var expectedArray = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

            var sortedArray = _sorter.Sort(sourceArray);

            CollectionAssert.AreEqual(expectedArray, sortedArray);
        }

        [TestCase]
        public void ShouldSortEmptyNumberArray()
        {
            CollectionAssert.IsEmpty(_sorter.Sort(new int[0]));
        }

        [TestCase]
        public void ShouldSortSingleNumberArray()
        {
            var sourceArray = new int[] { 6 };
            var expectedArray = new int[] { 6 };

            var sortedArray = _sorter.Sort(sourceArray);

            CollectionAssert.AreEqual(expectedArray, sortedArray);
        }

        [TestCase(5)]
        [TestCase(6)]
        [TestCase(7)]
        [TestCase(8)]
        [TestCase(9)]
        [TestCase(10)]
        public void ShouldSortDescendingNumberArray(int arraySize)
        {
            var sourceArray = Enumerable.Range(0, arraySize).Select(index => arraySize - index).ToArray();
            var sortedArray = _sorter.Sort(sourceArray);

            CollectionAssert.IsOrdered(sortedArray);
        }

        [TestCase(100)]
        [TestCase(1000)]
        [TestCase(1000000)]
        public void ShouldSortLargeRandomNumberArray(int arraySize)
        {
            var random = new Random();
            var sourceArray =  Enumerable.Range(0, arraySize).Select(index => random.Next()).ToArray();
            var sortedArray = _sorter.Sort(sourceArray);

            CollectionAssert.IsOrdered(sortedArray);
        }

        [TestCase]
        public void ShouldSortNumberStringArray()
        {
            var sorter = new OppositeMergeSort<FileLine>(new ChunkStack<FileLine>(
                100 * MemorySize.MB,
                new FileLineSizeCalculator(),
                null,
                null));

            var sourceArray = new FileLine[] 
            {
                FileLine.Parse("415. Apple"),
                FileLine.Parse("30432. Something something something"),
                FileLine.Parse("1. Apple"),
                FileLine.Parse("32. Cherry is the best"),
                FileLine.Parse("2. Banana is yellow")
            };
            var expectedArray = new FileLine[] 
            {
                FileLine.Parse("1. Apple"),
                FileLine.Parse("415. Apple"),
                FileLine.Parse("2. Banana is yellow"),
                FileLine.Parse("32. Cherry is the best"),
                FileLine.Parse("30432. Something something something")
            };

            var sortedArray = sorter.Sort(sourceArray);

            CollectionAssert.AreEqual(expectedArray, sortedArray);
        }

        [TestCase]
        public void ShouldMergeSimpleArrays()
        {
            var leftArray = new[] { 1, 3, 5, 7, 9 };
            var rightArray = new[] { 0, 2, 4, 6, 8, 10 };
            var expectedArray = new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10};
            var arrayMerge = new ChunkStack<int>.MemoryChunkReference(leftArray.Length + rightArray.Length, 0);
            OppositeMergeSort<int>.Merge(leftArray, rightArray, arrayMerge);

            CollectionAssert.AreEqual(expectedArray, arrayMerge.ToArray());
        }

        [TestCase]
        public void ShouldMergeEmptyArrayWithNotEmptyArray()
        {
            var leftArray = Array.Empty<int>();
            var rightArray = new[] { 0, 2, 4, 6, 8, 10 };
            var arrayMerge = new ChunkStack<int>.MemoryChunkReference(leftArray.Length + rightArray.Length, 0);
            OppositeMergeSort<int>.Merge(leftArray, rightArray, arrayMerge);

            CollectionAssert.AreEqual(rightArray, arrayMerge.ToArray());
        }

        [TestCase]
        public void ShouldMergeNotEmptyArrayWithEmptyArray()
        {
            var leftArray = new[] { 1, 3, 5, 7, 9 };
            var rightArray = Array.Empty<int>();
            var arrayMerge = new ChunkStack<int>.MemoryChunkReference(leftArray.Length + rightArray.Length, 0);
            OppositeMergeSort<int>.Merge(leftArray, rightArray, arrayMerge);

            CollectionAssert.AreEqual(leftArray, arrayMerge.ToArray());
        }

        [TestCase]
        public void ShouldMergeEmptyArrays()
        {
            var arrayMerge = new ChunkStack<int>.MemoryChunkReference(0, 0);
            OppositeMergeSort<int>.Merge(Array.Empty<int>(), Array.Empty<int>(), arrayMerge);

            CollectionAssert.IsEmpty(arrayMerge.ToArray());
        }
    }
}
