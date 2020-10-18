using FileSort.Core;
using NUnit.Framework;
using System;
using System.Linq;

namespace FileSort.UnitTests
{
    [TestFixture]
    public class OppositeMergeSortTests
    {
        [TestCase]
        public void ShouldSortSimpleNumberArray()
        {
            var sourceArray = new int[] { 6, 4, 5, 8, 7, 9, 2, 3, 1 };
            var expectedArray = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

            var sorter = CreateSorter<int>(new ConstantSizeCalculator<int>(sizeof(int)));
            var sortedArray = sorter.Sort(sourceArray);

            CollectionAssert.AreEqual(expectedArray, sortedArray);
        }

        [TestCase]
        public void ShouldSortEmptyNumberArray()
        {
            var sorter = CreateSorter<int>(new ConstantSizeCalculator<int>(sizeof(int)));
            CollectionAssert.IsEmpty(sorter.Sort(new int[0]));
        }

        [TestCase]
        public void ShouldSortSingleNumberArray()
        {
            var sourceArray = new int[] { 6 };
            var expectedArray = new int[] { 6 };

            var sorter = CreateSorter<int>(new ConstantSizeCalculator<int>(sizeof(int)));
            var sortedArray = sorter.Sort(sourceArray);

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

            var sorter = CreateSorter<int>(new ConstantSizeCalculator<int>(sizeof(int)));
            var sortedArray = sorter.Sort(sourceArray);

            CollectionAssert.IsOrdered(sortedArray);
        }

        [TestCase(100)]
        [TestCase(1000)]
        [TestCase(1000000)]
        public void ShouldSortLargeRandomNumberArray(int arraySize)
        {
            var random = new Random();
            var sourceArray =  Enumerable.Range(0, arraySize).Select(index => random.Next()).ToArray();

            var sorter = CreateSorter<int>(new ConstantSizeCalculator<int>(sizeof(int)));
            var sortedArray = sorter.Sort(sourceArray);

            CollectionAssert.IsOrdered(sortedArray);
        }

        [TestCase]
        public void ShouldSortNumberStringArray()
        {
            var sorter = CreateSorter<FileLine>(new FileLineSizeCalculator());

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
        public void ShouldPushChunkPairsRecursively()
        {
            var chunkStack1 = new ChunkStack<int>(
                100 * MemorySize.MB,
                new ConstantSizeCalculator<int>(sizeof(int)),
                null);

            var chunkStack2 = new ChunkStack<int>(
                100 * MemorySize.MB,
                new ConstantSizeCalculator<int>(sizeof(int)),
                null);

            var appender = new MergeSortBase<int>.ChunkStackAppender(chunkStack1, chunkStack2);

            appender.PushToStackRecursively(new int[] { 1, 2 });
            CollectionAssert.AreEqual(new int[] { 2 }, chunkStack1.GetChunkSizes());
            CollectionAssert.AreEqual(new int[] { }, chunkStack2.GetChunkSizes());

            appender.PushToStackRecursively(new int[] { 3, 4 });
            CollectionAssert.AreEqual(new int[] { }, chunkStack1.GetChunkSizes());
            CollectionAssert.AreEqual(new int[] { 4 }, chunkStack2.GetChunkSizes());

            appender.PushToStackRecursively(new int[] { 5, 6 });
            CollectionAssert.AreEqual(new int[] { 2 }, chunkStack1.GetChunkSizes());
            CollectionAssert.AreEqual(new int[] { 4 }, chunkStack2.GetChunkSizes());

            appender.PushToStackRecursively(new int[] { 7, 8 });
            CollectionAssert.AreEqual(new int[] { 8 }, chunkStack1.GetChunkSizes());
            CollectionAssert.AreEqual(new int[] { }, chunkStack2.GetChunkSizes());

            appender.PushToStackRecursively(new int[] { 9, 10 });
            CollectionAssert.AreEqual(new int[] { 2,  8 }, chunkStack1.GetChunkSizes());
            CollectionAssert.AreEqual(new int[] { }, chunkStack2.GetChunkSizes());

            appender.PushToStackRecursively(new int[] { 11, 12 });
            CollectionAssert.AreEqual(new int[] { 8 }, chunkStack1.GetChunkSizes());
            CollectionAssert.AreEqual(new int[] { 4 }, chunkStack2.GetChunkSizes());

            appender.PushToStackRecursively(new int[] { 12, 13 });
            CollectionAssert.AreEqual(new int[] { 2, 8 }, chunkStack1.GetChunkSizes());
            CollectionAssert.AreEqual(new int[] { 4 }, chunkStack2.GetChunkSizes());

            appender.PushToStackRecursively(new int[] { 14, 15 });
            CollectionAssert.AreEqual(new int[] { }, chunkStack1.GetChunkSizes());
            CollectionAssert.AreEqual(new int[] { 16 }, chunkStack2.GetChunkSizes());

            appender.PushToStackRecursively(new int[] { 16, 17 });
            CollectionAssert.AreEqual(new int[] { 2}, chunkStack1.GetChunkSizes());
            CollectionAssert.AreEqual(new int[] { 16 }, chunkStack2.GetChunkSizes());
        }

        private OppositeMergeSort<T> CreateSorter<T>(ISizeCalculator<T> sizeCalculator) where T : IComparable
        {
           return new OppositeMergeSort<T>(new ChunkStack<T>(
                100 * MemorySize.MB,
                sizeCalculator,
                null),
                new ChunkStack<T>(
                100 * MemorySize.MB,
                sizeCalculator,
                null));
        }
    }
}
