using System.Collections.Generic;

namespace FileSort
{
    public class FileSort
    {
        public static int[] Sort(IEnumerable<int> source)
        {
            var sorter = new OppositeMergeSort();

            return sorter.Sort(source);
        }
    }
}
