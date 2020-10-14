using System;
using System.Collections.Generic;

namespace FileSort
{
    public interface ISortMethod<T> where T : IComparable
    {
        IEnumerable<T> Sort(IEnumerable<T> source);
    }
}