using System;
using System.Collections.Generic;

namespace FileSort.Core
{
    public interface ISortMethod<T> where T : IComparable
    {
        IEnumerable<T> Sort(IEnumerable<T> source);
    }
}