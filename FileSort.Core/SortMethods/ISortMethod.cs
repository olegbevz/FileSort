using System;
using System.Collections.Generic;

namespace FileSort.Core
{
    /// <summary>
    /// ISortMethod represents astract interface for sort logic
    /// </summary>
    /// <typeparam name="T">Type of data</typeparam>
    public interface ISortMethod<T> where T : IComparable
    {
        IEnumerable<T> Sort(IEnumerable<T> source);
    }
}