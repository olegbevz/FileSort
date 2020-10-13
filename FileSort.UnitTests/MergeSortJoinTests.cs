using NUnit.Framework;
using System;

namespace FileSort.UnitTests
{
    [TestFixture]
    public class MergeSortJoinTests
    {
        private readonly ISortJoin<int> _sortJoin = new MergeSortJoin<int>();

        [TestCase]
        public void ShouldMergeSimpleArrays()
        {
            var leftArray = new[] { 1, 3, 5, 7, 9 };
            var rightArray = new[] { 0, 2, 4, 6, 8, 10 };
            var expectedArray = new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            var arrayMerge = _sortJoin.Join(leftArray, rightArray);

            CollectionAssert.AreEqual(expectedArray, arrayMerge);
        }

        [TestCase]
        public void ShouldMergeEmptyArrayWithNotEmptyArray()
        {
            var leftArray = Array.Empty<int>();
            var rightArray = new[] { 0, 2, 4, 6, 8, 10 };
            var arrayMerge = _sortJoin.Join(leftArray, rightArray);

            CollectionAssert.AreEqual(rightArray, arrayMerge);
        }

        [TestCase]
        public void ShouldMergeNotEmptyArrayWithEmptyArray()
        {
            var leftArray = new[] { 1, 3, 5, 7, 9 };
            var rightArray = Array.Empty<int>();
            var arrayMerge = _sortJoin.Join(leftArray, rightArray);

            CollectionAssert.AreEqual(leftArray, arrayMerge);
        }

        [TestCase]
        public void ShouldMergeEmptyArrays()
        {
            var arrayMerge = _sortJoin.Join(Array.Empty<int>(), Array.Empty<int>());

            CollectionAssert.IsEmpty(arrayMerge);
        }
    }
}
