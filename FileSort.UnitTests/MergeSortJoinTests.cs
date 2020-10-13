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
            var arrayMerge = new ChunkStack<int>.MemoryChunkReference(leftArray.Length + rightArray.Length, 0, null);
            _sortJoin.Merge(leftArray, rightArray, arrayMerge);

            CollectionAssert.AreEqual(expectedArray, arrayMerge.ToArray());
        }

        [TestCase]
        public void ShouldMergeEmptyArrayWithNotEmptyArray()
        {
            var leftArray = Array.Empty<int>();
            var rightArray = new[] { 0, 2, 4, 6, 8, 10 };
            var arrayMerge = new ChunkStack<int>.MemoryChunkReference(leftArray.Length + rightArray.Length, 0, null);
            _sortJoin.Merge(leftArray, rightArray, arrayMerge);

            CollectionAssert.AreEqual(rightArray, arrayMerge.ToArray());
        }

        [TestCase]
        public void ShouldMergeNotEmptyArrayWithEmptyArray()
        {
            var leftArray = new[] { 1, 3, 5, 7, 9 };
            var rightArray = Array.Empty<int>();
            var arrayMerge = new ChunkStack<int>.MemoryChunkReference(leftArray.Length + rightArray.Length, 0, null);
            _sortJoin.Merge(leftArray, rightArray, arrayMerge);

            CollectionAssert.AreEqual(leftArray, arrayMerge.ToArray());
        }

        [TestCase]
        public void ShouldMergeEmptyArrays()
        {
            var arrayMerge = new ChunkStack<int>.MemoryChunkReference(0, 0, null);
            _sortJoin.Merge(Array.Empty<int>(), Array.Empty<int>(), arrayMerge);

            CollectionAssert.IsEmpty(arrayMerge.ToArray());
        }
    }
}
