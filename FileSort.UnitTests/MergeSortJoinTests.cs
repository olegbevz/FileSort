using NUnit.Framework;
using FileSort.Core;
using System;
using System.Linq;

namespace FileSort.UnitTests
{
    [TestFixture]
    public class MergeSortJoinTests
    {
        private readonly ISortJoin<int> _sortJoin = new MergeSortJoin<int>();

        [TestCase]
        public void ShouldMergeTwoSimpleArrays()
        {
            var leftArray = new[] { 1, 3, 5, 7, 9 };
            var rightArray = new[] { 0, 2, 4, 6, 8, 10 };
            var expectedArray = new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            var arrayMerge = _sortJoin.Join(leftArray, rightArray);

            CollectionAssert.AreEqual(expectedArray, arrayMerge);
        }

        [TestCase]
        public void ShouldMergeMultipleSimpleArrays()
        {
            var array1 = new[] { 1, 5, 8, 14, 17 };
            var array2 = new[] { 0, 7, 10, 13, 19, 21 };
            var array3 = new[] { 3, 4, 9, 12, 16 };
            var array4 = new[] { 2, 6, 11, 15, 18, 20 };
            var expectedArray = Enumerable.Range(0, 22);
            var arrayMerge = _sortJoin.Join(array1, array2, array3, array4);

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
        public void ShouldMergeEmptyArraysWithNotEmptyArrays()
        {
            var array1 = Array.Empty<int>();
            var array2 = new[] { 0, 7, 10, 13, 19, 21 };
            var array3 = Array.Empty<int>();
            var array4 = new[] { 2, 6, 11, 15, 18, 20 };
            var expectedArray = new int[] { 0, 2, 6, 7, 10, 11, 13, 15, 18, 19, 20, 21 };
            var arrayMerge = _sortJoin.Join(array1, array2, array3, array4);

            CollectionAssert.AreEqual(expectedArray, arrayMerge);
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
        public void ShouldMergeNotEmptyArraysWithEmptyArrays()
        {
            var array1 = new[] { 1, 5, 8, 14, 17 };
            var array2 = Array.Empty<int>();
            var array3 = new[] { 3, 4, 9, 12, 16 };
            var array4 = Array.Empty<int>();
            var expectedArray = new int[] { 1, 3, 4, 5, 8, 9, 12, 14, 16, 17 };
            var arrayMerge = _sortJoin.Join(array1, array2, array3, array4);

            CollectionAssert.AreEqual(expectedArray, arrayMerge);
        }

        [TestCase]
        public void ShouldMergeTwoEmptyArrays()
        {
            var arrayMerge = _sortJoin.Join(Array.Empty<int>(), Array.Empty<int>());
            CollectionAssert.IsEmpty(arrayMerge);
        }

        [TestCase]
        public void ShouldMergeMultipleEmptyArrays()
        {
            var arrayMerge = _sortJoin.Join(
                Array.Empty<int>(), 
                Array.Empty<int>(),
                Array.Empty<int>(),
                Array.Empty<int>());

            CollectionAssert.IsEmpty(arrayMerge);
        }
    }
}
