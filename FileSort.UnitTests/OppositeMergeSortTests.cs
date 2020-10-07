using NUnit.Framework;
using System;
using System.Linq;

namespace FileSort.UnitTests
{
    [TestFixture]
    public class OppositeMergeSortTests
    {
        private readonly OppositeMergeSort _sorter = new OppositeMergeSort();

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
            var sourceArray = new FileEntry[] 
            {
                FileEntry.Parse("415. Apple"),
                FileEntry.Parse("30432. Something something something"),
                FileEntry.Parse("1. Apple"),
                FileEntry.Parse("32. Cherry is the best"),
                FileEntry.Parse("2. Banana is yellow")
            };
            var expectedArray = new FileEntry[] 
            {
                FileEntry.Parse("1. Apple"),
                FileEntry.Parse("415. Apple"),
                FileEntry.Parse("2. Banana is yellow"),
                FileEntry.Parse("32. Cherry is the best"),
                FileEntry.Parse("30432. Something something something")
            };

            var sortedArray = _sorter.Sort(sourceArray);

            CollectionAssert.AreEqual(expectedArray, sortedArray);
        }

        public struct FileEntry : IComparable
        {
            public static FileEntry Parse(string data)
            {
                var parts = data.Split('.');
                return new FileEntry(int.Parse(parts[0]), parts[1]);
            }

            public FileEntry(int number, string name)
            {
                Number = number;
                Name = name;
            }

            public int Number;

            public string Name;

            public int CompareTo(object obj)
            {
                if (!(obj is FileEntry otherEntry))
                    return -1;

                if (Name != null && otherEntry.Name != null)
                {
                    var compareResult = Name.CompareTo(otherEntry.Name);

                    if (compareResult != 0)
                        return compareResult;
                }

                return Number.CompareTo(otherEntry.Number);
            }
        }
    }
}
