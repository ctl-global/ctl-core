/*
    Copyright (c) 2014, CTL Global, Inc.
    Copyright (c) 2014, iD Commerce + Logistics
    All rights reserved.

    Redistribution and use in source and binary forms, with or without modification, are permitted
    provided that the following conditions are met:

    Redistributions of source code must retain the above copyright notice, this list of conditions
    and the following disclaimer. Redistributions in binary form must reproduce the above copyright
    notice, this list of conditions and the following disclaimer in the documentation and/or other
    materials provided with the distribution.
 
    THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR
    IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
    FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR
    CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
    CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
    SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
    THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
    OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
    POSSIBILITY OF SUCH DAMAGE.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ctl.Extensions
{
    public static class ListExtensions
    {
        /// <summary>
        /// Performs a binary search, returning the index of the first item in <paramref name="list"/> which
        /// compares greater or equal to <paramref name="value"/>, using the type's default comparer.
        /// </summary>
        /// <typeparam name="T">The type to search for.</typeparam>
        /// <param name="list">A list to search.</param>
        /// <param name="value">A value to search for.</param>
        /// <returns>
        /// The index of the first item which compares greater or equal to the given value.
        /// If the list has no such values, it will be to equal to the size of the list.
        /// </returns>
        /// <remarks>
        /// Because this performs a binary search, the list must be sorted according to the comparer.
        /// </remarks>
        public static int LowerBound<T>(this IList<T> list, T value)
        {
            return LowerBound(list, value, 0, list.Count, null);
        }

        /// <summary>
        /// Performs a binary search, returning the index of the first item in <paramref name="list"/> which
        /// compares greater or equal to <paramref name="value"/>, using the type's default comparer.
        /// </summary>
        /// <typeparam name="T">The type to search for.</typeparam>
        /// <param name="list">A list to search.</param>
        /// <param name="index">An index of where to start the search.</param>
        /// <param name="count">The number of items to search through, starting at <paramref name="index"/>.</param>
        /// <param name="value">A value to search for.</param>
        /// <returns>
        /// The index of the first item which compares greater or equal to the given value.
        /// If the list has no such values, it will be to equal to the size of the list.
        /// </returns>
        /// <remarks>
        /// Because this performs a binary search, the list must be sorted according to the comparer.
        /// </remarks>
        public static int LowerBound<T>(this IList<T> list, T value, int index, int count)
        {
            return LowerBound(list, value, index, count, null);
        }

        /// <summary>
        /// Performs a binary search, returning the index of the first item in <paramref name="list"/> which
        /// compares greater or equal to <paramref name="value"/>, using <paramref name="comparer"/>.
        /// </summary>
        /// <typeparam name="T">The type to search for.</typeparam>
        /// <param name="list">A list to search.</param>
        /// <param name="index">An index of where to start the search.</param>
        /// <param name="count">The number of items to search through, starting at <paramref name="index"/>.</param>
        /// <param name="value">A value to search for.</param>
        /// <param name="comparer">A comparer to use, or null to use the type's default comparer.</param>
        /// <returns>
        /// The index of the first item which compares greater or equal to the given value.
        /// If the list has no such values, it will be to equal to the size of the list.
        /// </returns>
        /// <remarks>
        /// Because this performs a binary search, the list must be sorted according to the comparer.
        /// </remarks>
        public static int LowerBound<T>(this IList<T> list, T value, int index, int count, IComparer<T> comparer)
        {
            if (list == null) throw new ArgumentNullException("list");
            if (index < 0 || index >= list.Count) throw new ArgumentOutOfRangeException("index", "index is out of bounds for given list.");
            if (count <= 0 || (index + count) > list.Count) throw new ArgumentOutOfRangeException("count", "count must be positive and within bounds of the list.");
            if (comparer == null) comparer = Comparer<T>.Default;

            while (0 < count)
            {
                var count2 = count >> 1;
                int mid = index + count2;

                if (comparer.Compare(list[mid], value) < 0)
                {
                    index = mid + 1;
                    count -= count2 + 1;
                }
                else
                {
                    count = count2;
                }
            }

            return index;
        }

        /// <summary>
        /// Performs a binary search, returning the index of the first item in <paramref name="list"/> which
        /// compares greater than <paramref name="value"/>, using the default comparer.
        /// </summary>
        /// <typeparam name="T">The type to search for.</typeparam>
        /// <param name="list">A list to search.</param>
        /// <param name="value">A value to search for.</param>
        /// <returns>
        /// The index of the first item which compares greater than the given value.
        /// If the list has no such values, it will be to equal to the size of the list.
        /// </returns>
        /// <remarks>
        /// Because this performs a binary search, the list must be sorted according to the comparer.
        /// </remarks>
        public static int UpperBound<T>(this IList<T> list, T value)
        {
            return UpperBound(list, value, 0, list.Count, null);
        }

        /// <summary>
        /// Performs a binary search, returning the index of the first item in <paramref name="list"/> which
        /// compares greater than <paramref name="value"/>, using the default comparer.
        /// </summary>
        /// <typeparam name="T">The type to search for.</typeparam>
        /// <param name="list">A list to search.</param>
        /// <param name="index">An index of where to start the search.</param>
        /// <param name="count">The number of items to search through, starting at <paramref name="index"/>.</param>
        /// <param name="value">A value to search for.</param>
        /// <returns>
        /// The index of the first item which compares greater than the given value.
        /// If the list has no such values, it will be to equal to the size of the list.
        /// </returns>
        /// <remarks>
        /// Because this performs a binary search, the list must be sorted according to the comparer.
        /// </remarks>
        public static int UpperBound<T>(this IList<T> list, T value, int index, int count)
        {
            return UpperBound(list, value, index, count, null);
        }

        /// <summary>
        /// Performs a binary search, returning the index of the first item in <paramref name="list"/> which
        /// compares greater than <paramref name="value"/>, using <paramref name="comparer"/>.
        /// </summary>
        /// <typeparam name="T">The type to search for.</typeparam>
        /// <param name="list">A list to search.</param>
        /// <param name="index">An index of where to start the search.</param>
        /// <param name="count">The number of items to search through, starting at <paramref name="index"/>.</param>
        /// <param name="value">A value to search for.</param>
        /// <param name="comparer">A comparer to use, or null to use the type's default comparer.</param>
        /// <returns>
        /// The index of the first item which compares greater than the given value.
        /// If the list has no such values, it will be to equal to the size of the list.
        /// </returns>
        /// <remarks>
        /// Because this performs a binary search, the list must be sorted according to the comparer.
        /// </remarks>
        public static int UpperBound<T>(this IList<T> list, T value, int index, int count, IComparer<T> comparer)
        {
            if (list == null) throw new ArgumentNullException("list");
            if (index < 0 || index >= list.Count) throw new ArgumentOutOfRangeException("index", "index is out of bounds for given list.");
            if (count <= 0 || (index + count) > list.Count) throw new ArgumentOutOfRangeException("count", "count must be positive and within bounds of the list.");
            if (comparer == null) comparer = Comparer<T>.Default;

            while (0 < count)
            {
                var count2 = count >> 1;
                int mid = index + count2;

                if (comparer.Compare(value, list[mid]) >= 0)
                {
                    index = mid + 1;
                    count -= count2 + 1;
                }
                else
                {
                    count = count2;
                }
            }

            return index;
        }

        /// <summary>
        /// Shuffles a list.
        /// </summary>
        /// <typeparam name="T">The type of item to shuffle.</typeparam>
        /// <param name="list">The list to shuffle</param>
        /// <param name="rng">A random number generator to use. If none is provided, the default random number generator will be used.</param>
        public static void Shuffle<T>(this IList<T> list, Random rng = null)
        {
            if (list == null) throw new ArgumentNullException(nameof(list));

            if (rng == null)
            {
                rng = StaticRandom.LocalInstance;
            }

            for (int i = list.Count; i != 1;)
            {
                int swapidx = rng.Next(i--);

                T tmp = list[i];
                list[i] = list[swapidx];
                list[swapidx] = tmp;
            }
        }
    }
}
