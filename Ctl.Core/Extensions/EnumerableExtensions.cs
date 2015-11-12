/*
    Copyright (c) 2014, CTL Global, Inc.
    Copyright (c) 2012, iD Commerce + Logistics
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
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Ctl.Extensions
{
    /// <summary>
    /// Provides extension methods for Enumerable.
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Simultaneously retreieves the minimum and maximum element from a sequence.
        /// </summary>
        /// <typeparam name="T">The type of item in the sequence.</typeparam>
        /// <param name="items">The sequence to operate on.</param>
        /// <returns>A tuple containing the minimum value in Item1, and maximum value in Item2.</returns>
        public static Tuple<T, T> MinMax<T>(this IEnumerable<T> items)
        {
            return MinMax(items, x => x, null);
        }

        /// <summary>
        /// Simultaneously retreieves the minimum and maximum element from a sequence.
        /// </summary>
        /// <typeparam name="T">The type of item in the sequence.</typeparam>
        /// <param name="items">The sequence to operate on.</param>
        /// <param name="comparer">A comparer. If null, the type's default comparer will be used.</param>
        /// <returns>A tuple containing the minimum value in Item1, and maximum value in Item2.</returns>
        public static Tuple<T, T> MinMax<T>(this IEnumerable<T> items, IComparer<T> comparer)
        {
            return MinMax(items, x => x, comparer);
        }
        
        /// <summary>
        /// Simultaneously retreieves the minimum and maximum element from a sequence.
        /// </summary>
        /// <typeparam name="T">The type of item in the sequence.</typeparam>
        /// <typeparam name="TElement">The type of element to extract.</typeparam>
        /// <param name="items">The sequence to operate on.</param>
        /// <param name="extractor">A function to extract the element from an item.</param>
        /// <returns>A tuple containing the minimum value in Item1, and maximum value in Item2.</returns>
        public static Tuple<TElement, TElement> MinMax<T, TElement>(this IEnumerable<T> items, Func<T, TElement> extractor)
        {
            return MinMax(items, extractor, null);
        }

        /// <summary>
        /// Simultaneously retreieves the minimum and maximum element from a sequence.
        /// </summary>
        /// <typeparam name="T">The type of item in the sequence.</typeparam>
        /// <typeparam name="TElement">The type of element to extract.</typeparam>
        /// <param name="items">The sequence to operate on.</param>
        /// <param name="extractor">A function to extract the element from an item.</param>
        /// <param name="comparer">A comparer. If null, the type's default comparer will be used.</param>
        /// <returns>A tuple containing the minimum value in Item1, and maximum value in Item2.</returns>
        public static Tuple<TElement, TElement> MinMax<T, TElement>(this IEnumerable<T> items, Func<T, TElement> extractor, IComparer<TElement> comparer)
        {
            if (comparer == null)
            {
                comparer = Comparer<TElement>.Default;
            }

            TElement min, max;

            using (var e = items.GetEnumerator())
            {
                if (!e.MoveNext())
                {
                    throw new ArgumentException("items must contain at least one item.", "items");
                }

                min = max = extractor(e.Current);

                while (e.MoveNext())
                {
                    TElement val = extractor(e.Current);

                    if (comparer.Compare(val, min) < 0)
                    {
                        min = val;
                    }

                    if (comparer.Compare(val, max) > 0)
                    {
                        max = val;
                    }
                }
            }

            return Tuple.Create(min, max);
        }

        /// <summary>
        /// Creates a HashSet from the given sequence.
        /// </summary>
        /// <typeparam name="T">The type of the HashSet.</typeparam>
        /// <param name="items">The sequence to create a hashset from. If the sequence contains duplicates, an exception will be thrown.</param>
        /// <returns>A hashset of the items.</returns>
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> items)
        {
            return ToHashSet(items, null);
        }

        /// <summary>
        /// Creates a HashSet from the given sequence.
        /// </summary>
        /// <typeparam name="T">The type of the HashSet.</typeparam>
        /// <param name="items">The sequence to create a hashset from. If the sequence contains duplicates, an exception will be thrown.</param>
        /// <param name="comparer">A comparer used to hash items in the sequence. If null, the type's default equality comparer will be used.</param>
        /// <returns>A hashset of the items.</returns>
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> items, IEqualityComparer<T> comparer)
        {
            if (items == null) throw new ArgumentNullException("items");

            HashSet<T> set = new HashSet<T>(comparer);

            foreach (var v in items)
            {
                if (v == null) throw new ArgumentNullException("items", "Sequence must not contain null values.");

                if (!set.Add(v))
                {
                    throw new ArgumentException("Sequence must not contain duplicate values.", "items");
                }
            }

            return set;
        }

        /// <summary>
        /// Creates a HashSet from the given sequence.
        /// </summary>
        /// <typeparam name="T">The type of the HashSet.</typeparam>
        /// <param name="items">The sequence to create a hashset from. If the sequence contains duplicates, the set will contain only one of them -- no exceptions will be thrown.</param>
        /// <returns>A hashset of the items.</returns>
        public static HashSet<T> ToDistinctHashSet<T>(this IEnumerable<T> items)
        {
            if (items == null) throw new ArgumentNullException("items");
            return new HashSet<T>(items);
        }

        /// <summary>
        /// Creates a HashSet from the given sequence.
        /// </summary>
        /// <typeparam name="T">The type of the HashSet.</typeparam>
        /// <param name="items">The sequence to create a hashset from. If the sequence contains duplicates, the set will contain only one of them -- no exceptions will be thrown.</param>
        /// <param name="comparer">A comparer used to hash items in the sequence. If null, the type's default equality comparer will be used.</param>
        /// <returns>A hashset of the items.</returns>
        public static HashSet<T> ToDistinctHashSet<T>(this IEnumerable<T> items, IEqualityComparer<T> comparer)
        {
            if (items == null) throw new ArgumentNullException("items");
            return new HashSet<T>(items, comparer);
        }

        /// <summary>
        /// Returns an empty sequence if the one passed is null.
        /// </summary>
        /// <typeparam name="T">The type of the sequence.</typeparam>
        /// <param name="items">The sequence to return.</param>
        /// <returns>If <paramref name="items"/> is not null, then <paramref name="items"/>. Otherwise, an empty sequence.</returns>
        /// <remarks>Name taken from *nix "touch" utility.</remarks>
        public static IEnumerable<T> Touch<T>(this IEnumerable<T> items)
        {
            return items != null ? items : Enumerable.Empty<T>();
        }

        /// <summary>
        /// Finds the index of the first item 
        /// </summary>
        /// <typeparam name="T">The type of items to search.</typeparam>
        /// <param name="items">The collection of items to search.</param>
        /// <param name="predicate">The predicate to test for a match.</param>
        /// <returns>The index of the first item matching <paramref name="predicate"/>.</returns>
        public static int IndexOfSingle<T>(this IEnumerable<T> items, Func<T, bool> predicate)
        {
            if (items == null) throw new ArgumentNullException("items");
            if (predicate == null) throw new ArgumentNullException("predicate");

            int i = 0;

            foreach (T item in items)
            {
                if (predicate(item)) return i;
                ++i;
            }

            return -1;
        }

        /// <summary>
        /// Sorts items based on a comparison function.
        /// </summary>
        /// <typeparam name="T">The type to sort.</typeparam>
        /// <param name="items">The items to sort.</param>
        /// <param name="comparison">The comparison.</param>
        /// <returns>A sorted collection of items.</returns>
        public static IEnumerable<T> Sort<T>(this IEnumerable<T> items, Comparison<T> comparison)
        {
            if (items == null) throw new ArgumentNullException("items");
            if (comparison == null) throw new ArgumentNullException("comparison");

            return items.OrderBy(x => x, Comparer<T>.Create(comparison));
        }

        /// <summary>
        /// Returns a collection of distinct items, preserving the order they came in, using the default comparer.
        /// </summary>
        /// <typeparam name="T">The type of items in the collection.</typeparam>
        /// <param name="items">A collection to process.</param>
        /// <returns>A collection of distinct items, with order preserved.</returns>
        public static IEnumerable<T> OrderedDistinct<T>(this IEnumerable<T> items)
        {
            if (items == null) throw new ArgumentNullException("items");

            return OrderedDistinct(items, EqualityComparer<T>.Default);
        }

        /// <summary>
        /// Returns a collection of distinct items, preserving the order they came in.
        /// </summary>
        /// <typeparam name="T">The type of items in the collection.</typeparam>
        /// <param name="items">A collection to process.</param>
        /// <param name="comparer">A comparer to test for distinct items.</param>
        /// <returns>A collection of distinct items, with order preserved.</returns>
        public static IEnumerable<T> OrderedDistinct<T>(this IEnumerable<T> items, IEqualityComparer<T> comparer)
        {
            if (items == null) throw new ArgumentNullException("items");
            if (comparer == null) throw new ArgumentNullException("comparer");

            HashSet<T> set = new HashSet<T>(comparer);

            foreach (T item in items)
            {
                if (set.Add(item))
                {
                    yield return item;
                }
            }
        }

        /// <summary>
        /// Performs a merge sort on two ordered sequences using the default comparer.
        /// </summary>
        /// <typeparam name="T">The type of item to merge.</typeparam>
        /// <typeparam name="TKey">The type of key to sort on.</typeparam>
        /// <param name="left">The first ordered sequence to merge.</param>
        /// <param name="right">The second ordered sequence to merge.</param>
        /// <param name="getKey">A function to retrieve the sort key from an item.</param>
        /// <returns>An ordered sequence merging the items of each input sequence.</returns>
        public static IEnumerable<T> OrderedMerge<T, TKey>(this IEnumerable<T> left, IEnumerable<T> right, Func<T, TKey> getKey)
        {
            if (left == null) throw new ArgumentNullException("left");
            if (right == null) throw new ArgumentNullException("right");
            if (getKey == null) throw new ArgumentNullException("getKey");

            return OrderedMerge(left, right, getKey, Comparer<TKey>.Default);
        }

        /// <summary>
        /// Performs a merge sort on two ordered sequences.
        /// </summary>
        /// <typeparam name="T">The type of item to merge.</typeparam>
        /// <typeparam name="TKey">The type of key to sort on.</typeparam>
        /// <param name="left">The first ordered sequence to merge.</param>
        /// <param name="right">The second ordered sequence to merge.</param>
        /// <param name="getKey">A function to retrieve the sort key from an item.</param>
        /// <param name="comparer">The key comparer to use for sorting.</param>
        /// <returns>An ordered sequence merging the items of each input sequence.</returns>
        public static IEnumerable<T> OrderedMerge<T, TKey>(this IEnumerable<T> left, IEnumerable<T> right, Func<T, TKey> getKey, IComparer<TKey> comparer)
        {
            if (left == null) throw new ArgumentNullException("left");
            if (right == null) throw new ArgumentNullException("right");
            if (getKey == null) throw new ArgumentNullException("getKey");

            if (comparer == null)
            {
                comparer = Comparer<TKey>.Default;
            }

            using (IEnumerator<T> liter = left.GetEnumerator())
            using (IEnumerator<T> riter = right.GetEnumerator())
            {
                bool haveLeft = liter.MoveNext();
                bool haveRight = riter.MoveNext();

                while (haveLeft && haveRight)
                {
                    if (comparer.Compare(getKey(liter.Current), getKey(riter.Current)) < 0)
                    {
                        yield return liter.Current;
                        haveLeft = liter.MoveNext();
                    }
                    else
                    {
                        yield return riter.Current;
                        haveRight = riter.MoveNext();
                    }
                }

                while (haveLeft)
                {
                    yield return liter.Current;
                    haveLeft = liter.MoveNext();
                }

                while (haveRight)
                {
                    yield return riter.Current;
                    haveRight = riter.MoveNext();
                }
            }
        }
        /// <summary>
        /// Performs a set intersect of two sorted sequences.
        /// </summary>
        /// <typeparam name="T">The type of item to operate on.</typeparam>
        /// <param name="left">The first sequence to use.</param>
        /// <param name="right">The second sequence ot use.</param>
        /// <param name="keyComparer">A comparer to sort items with. If null, the type's default comparer is used.</param>
        /// <returns>A sequence of items.</returns>
        public static IEnumerable<T> OrderedIntersect<T>(this IEnumerable<T> left, IEnumerable<T> right, IComparer<T> keyComparer)
        {
            return OrderedIntersect(left, right, x => x, keyComparer);
        }

        /// <summary>
        /// Performs a set intersect of multiple sorted sequences.
        /// </summary>
        /// <typeparam name="T">The type of item to operate on.</typeparam>
        /// <param name="collections">Sequences to to union.</param>
        /// <param name="keyComparer">A comparer to sort items with. If null, the type's default comparer is used.</param>
        /// <returns>A sequence of items.</returns>
        public static IEnumerable<T> OrderedIntersect<T>(this IEnumerable<IEnumerable<T>> collections, IComparer<T> keyComparer)
        {
            return OrderedIntersect(collections, x => x, keyComparer);
        }

        /// <summary>
        /// Performs a set intersect of multiple sorted sequences.
        /// </summary>
        /// <typeparam name="T">The type of item to operate on.</typeparam>
        /// <typeparam name="TKey">The key to use for comparison.</typeparam>
        /// <param name="collections">Sequences to to union.</param>
        /// <param name="keyExtractor">A function to extract keys from items.</param>
        /// <param name="keyComparer">A comparer to sort keys with. If null, the type's default comparer is used.</param>
        /// <returns>A sequence of items.</returns>
        public static IEnumerable<T> OrderedIntersect<T, TKey>(this IEnumerable<IEnumerable<T>> collections, Func<T, TKey> keyExtractor, IComparer<TKey> keyComparer)
        {
            return ApplyMultiple(collections, keyExtractor, keyComparer, (left, right, e, c) => OrderedIntersect(left, right, e, c));
        }

        /// <summary>
        /// Performs a set intersect of two sorted sequences.
        /// </summary>
        /// <typeparam name="T">The type of item to operate on.</typeparam>
        /// <typeparam name="TKey">The key to use for comparison.</typeparam>
        /// <param name="left">The first sequence to use.</param>
        /// <param name="right">The second sequence ot use.</param>
        /// <param name="keyExtractor">A function to extract keys from items.</param>
        /// <param name="keyComparer">A comparer to sort keys with. If null, the type's default comparer is used.</param>
        /// <returns>A sequence of items.</returns>
        public static IEnumerable<T> OrderedIntersect<T, TKey>(this IEnumerable<T> left, IEnumerable<T> right, Func<T, TKey> keyExtractor, IComparer<TKey> keyComparer)
        {
            if (left == null) throw new ArgumentNullException("left");
            if (right == null) throw new ArgumentNullException("right");
            if (keyExtractor == null) throw new ArgumentNullException("keyExtractor");
            if (keyComparer == null) keyComparer = Comparer<TKey>.Default;

            using (IEnumerator<T> eLeft = left.GetEnumerator(), eRight = right.GetEnumerator())
            {
                bool haveLeft = eLeft.MoveNext();
                bool haveRight = eRight.MoveNext();

                if (!haveLeft || !haveRight)
                {
                    yield break;
                }

                T valueLeft = eLeft.Current;
                TKey keyLeft = keyExtractor(valueLeft);

                T valueRight = eRight.Current;
                TKey keyRight = keyExtractor(valueRight);

                do
                {
                    int cmp = keyComparer.Compare(keyLeft, keyRight);

                    if (cmp < 0)
                    {
                        haveLeft = GetNext(eLeft, ref valueLeft, ref keyLeft, keyExtractor, keyComparer);
                    }
                    else if (cmp > 0)
                    {
                        haveRight = GetNext(eRight, ref valueRight, ref keyRight, keyExtractor, keyComparer);
                    }
                    else
                    {
                        yield return valueLeft;
                        haveLeft = GetNext(eLeft, ref valueLeft, ref keyLeft, keyExtractor, keyComparer);
                        haveRight = GetNext(eRight, ref valueRight, ref keyRight, keyExtractor, keyComparer);
                    }
                }
                while (haveLeft && haveRight);
            }
        }

        /// <summary>
        /// Performs a set union of two sorted sequences.
        /// </summary>
        /// <typeparam name="T">The type of item to operate on.</typeparam>
        /// <param name="left">The first sequence to use.</param>
        /// <param name="right">The second sequence ot use.</param>
        /// <param name="keyComparer">A comparer to sort items with. If null, the type's default comparer is used.</param>
        /// <returns>A sequence of items.</returns>
        public static IEnumerable<T> OrderedUnion<T>(this IEnumerable<T> left, IEnumerable<T> right, IComparer<T> keyComparer)
        {
            return OrderedUnion(left, right, x => x, keyComparer);
        }

        /// <summary>
        /// Performs a set union of multiple sorted sequences.
        /// </summary>
        /// <typeparam name="T">The type of item to operate on.</typeparam>
        /// <param name="collections">Sequences to to union.</param>
        /// <param name="keyComparer">A comparer to sort items with. If null, the type's default comparer is used.</param>
        /// <returns>A sequence of items.</returns>
        public static IEnumerable<T> OrderedUnion<T>(this IEnumerable<IEnumerable<T>> collections, IComparer<T> keyComparer)
        {
            return OrderedUnion(collections, x => x, keyComparer);
        }

        /// <summary>
        /// Performs a set union of multiple sorted sequences.
        /// </summary>
        /// <typeparam name="T">The type of item to operate on.</typeparam>
        /// <typeparam name="TKey">The key to use for comparison.</typeparam>
        /// <param name="collections">Sequences to to union.</param>
        /// <param name="keyExtractor">A function to extract keys from items.</param>
        /// <param name="keyComparer">A comparer to sort keys with. If null, the type's default comparer is used.</param>
        /// <returns>A sequence of items.</returns>
        public static IEnumerable<T> OrderedUnion<T, TKey>(this IEnumerable<IEnumerable<T>> collections, Func<T, TKey> keyExtractor, IComparer<TKey> keyComparer)
        {
            return ApplyMultiple(collections, keyExtractor, keyComparer, (left, right, e, c) => OrderedUnion(left, right, e, c));
        }

        /// <summary>
        /// Performs a set union of two sorted sequences.
        /// </summary>
        /// <typeparam name="T">The type of item to operate on.</typeparam>
        /// <typeparam name="TKey">The key to use for comparison.</typeparam>
        /// <param name="left">The first sequence to use.</param>
        /// <param name="right">The second sequence ot use.</param>
        /// <param name="keyExtractor">A function to extract keys from items.</param>
        /// <param name="keyComparer">A comparer to sort keys with. If null, the type's default comparer is used.</param>
        /// <returns>A sequence of items.</returns>
        public static IEnumerable<T> OrderedUnion<T, TKey>(this IEnumerable<T> left, IEnumerable<T> right, Func<T, TKey> keyExtractor, IComparer<TKey> keyComparer)
        {
            if (left == null) throw new ArgumentNullException("left");
            if (right == null) throw new ArgumentNullException("right");
            if (keyExtractor == null) throw new ArgumentNullException("keyExtractor");
            if (keyComparer == null) keyComparer = Comparer<TKey>.Default;

            using (IEnumerator<T> eLeft = left.GetEnumerator(), eRight = right.GetEnumerator())
            {
                bool haveLeft = eLeft.MoveNext();
                bool haveRight = eRight.MoveNext();

                if (!haveLeft && !haveRight)
                {
                    yield break;
                }

                T valueLeft = default(T), valueRight = default(T);
                TKey keyLeft = default(TKey), keyRight = default(TKey);

                if (haveLeft)
                {
                    valueLeft = eLeft.Current;
                    keyLeft = keyExtractor(valueLeft);
                }

                if (haveRight)
                {
                    valueRight = eRight.Current;
                    keyRight = keyExtractor(valueRight);
                }

                while (haveLeft && haveRight)
                {
                    int cmp = keyComparer.Compare(keyLeft, keyRight);

                    if (cmp < 0)
                    {
                        yield return valueLeft;
                        haveLeft = GetNext(eLeft, ref valueLeft, ref keyLeft, keyExtractor, keyComparer);
                    }
                    else if (cmp > 0)
                    {
                        yield return valueRight;
                        haveRight = GetNext(eRight, ref valueRight, ref keyRight, keyExtractor, keyComparer);
                    }
                    else
                    {
                        yield return valueLeft;
                        haveLeft = GetNext(eLeft, ref valueLeft, ref keyLeft, keyExtractor, keyComparer);
                        haveRight = GetNext(eRight, ref valueRight, ref keyRight, keyExtractor, keyComparer);
                    }
                }

                while (haveLeft)
                {
                    yield return valueLeft;
                    haveLeft = GetNext(eLeft, ref valueLeft, ref keyLeft, keyExtractor, keyComparer);
                }

                while (haveRight)
                {
                    yield return valueRight;
                    haveRight = GetNext(eRight, ref valueRight, ref keyRight, keyExtractor, keyComparer);
                }
            }
        }

        /// <summary>
        /// Retrieves the next value from a sequence, filtering for duplicates and testing for proper ordering.
        /// </summary>
        static bool GetNext<T, TKey>(IEnumerator<T> e, ref T val, ref TKey key, Func<T, TKey> keyExtractor, IComparer<TKey> keyComparer)
        {
            Debug.Assert(e != null);
            Debug.Assert(keyExtractor != null);
            Debug.Assert(keyComparer != null);

            TKey newKey;

            while (e.MoveNext())
            {
                val = e.Current;

                newKey = keyExtractor(val);

                int cmp = keyComparer.Compare(key, newKey);

                if (cmp < 0)
                {
                    key = newKey;
                    return true;
                }

                if (cmp > 0)
                {
                    throw new InvalidOperationException("Collections given to sorted set methods must be ordered.");
                }
            }

            return false;
        }

        /// <summary>
        /// Recursively applies a merge function to a collection of sequences, flattening them into a single sequence.
        /// </summary>
        static IEnumerable<T> ApplyMultiple<T, TKey, TComparer>(this IEnumerable<IEnumerable<T>> collections, Func<T, TKey> keyExtractor, TComparer keyComparer, Func<IEnumerable<T>, IEnumerable<T>, Func<T, TKey>, TComparer, IEnumerable<T>> func)
        {
            Debug.Assert(collections != null);
            Debug.Assert(keyExtractor != null);
            Debug.Assert(keyComparer != null);
            Debug.Assert(func != null);

            // first pass - merge adjacent collections into a list.

            List<IEnumerable<T>> list = new List<IEnumerable<T>>();

            using (IEnumerator<IEnumerable<T>> e = collections.GetEnumerator())
            {
                while (e.MoveNext())
                {
                    IEnumerable<T> left = e.Current ?? Enumerable.Empty<T>();

                    if (e.MoveNext())
                    {
                        IEnumerable<T> right = e.Current ?? Enumerable.Empty<T>();
                        list.Add(func(left, right, keyExtractor, keyComparer));
                    }
                    else
                    {
                        list.Add(left);
                        break;
                    }
                }
            }

            // second pass - merge adjacent collections, until
            // there is only a single collection in the list.

            int count = list.Count;

            while (count > 1)
            {
                int idx = 0;

                for (int i = 0, len = count & ~1; i < len; i += 2)
                {
                    list[idx++] = func(list[i], list[i + 1], keyExtractor, keyComparer);
                }

                if ((count & 1) != 0)
                {
                    list[idx++] = list[count - 1];
                }

                count = idx;
            }

            return count != 0 ? list[0] : Enumerable.Empty<T>();
        }

        /// <summary>
        /// Returns an enumerable sequence which throws an OperationCancelledException if a cancellation token is set during enumeration.
        /// </summary>
        /// <typeparam name="T">The enumerable's type.</typeparam>
        /// <param name="e">An enumerable sequence.</param>
        /// <param name="cancellationToken">A token used for cancellation.</param>
        /// <returns>An enumerable sequence.</returns>
        public static IEnumerable<T> WithCancellation<T>(this IEnumerable<T> e, CancellationToken cancellationToken)
        {
            if (e == null) throw new ArgumentNullException("e");
            return cancellationToken.CanBeCanceled ? WithCancellationImpl(e, cancellationToken) : e;
        }

        static IEnumerable<T> WithCancellationImpl<T>(this IEnumerable<T> e, CancellationToken cancellationToken)
        {
            if (e == null) throw new ArgumentNullException("e");

            cancellationToken.ThrowIfCancellationRequested();

            using (var ee = e.GetEnumerator())
            {
                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (!ee.MoveNext())
                    {
                        yield break;
                    }

                    yield return ee.Current;
                }
            }
        }

        /// <summary>
        /// Pads the right side of a sequence with a value.
        /// </summary>
        /// <typeparam name="T">The enumerable's type.</typeparam>
        /// <param name="e">The enumerable sequence.</param>
        /// <param name="totalCount">The amount of items to return.</param>
        /// <param name="value">The value to pad the sequence with.</param>
        /// <returns>A sequence padded to be of at least a specified number of items.</returns>
        public static IEnumerable<T> PadRight<T>(this IEnumerable<T> e, int totalCount, T value = default(T))
        {
            if (e == null) throw new ArgumentNullException("e");

            foreach (T x in e)
            {
                yield return x;
                --totalCount;
            }

            while (--totalCount >= 0)
            {
                yield return value;
            }
        }

        /// <summary>
        /// Merges adjacent items in a sequence.
        /// </summary>
        /// <typeparam name="T">The input type to merge.</typeparam>
        /// <typeparam name="TOut">The output type.</typeparam>
        /// <param name="e">A sequence to merge.</param>
        /// <param name="testFunc">Takes a previous and current item, returning true if the two should be merged.</param>
        /// <param name="mergeFunc">Merges a range of input items into a single output item.</param>
        /// <returns>A sequence of merged items.</returns>
        public static IEnumerable<TOut> MergeAdjacent<T, TOut>(this IEnumerable<T> e, Func<T, T, bool> testFunc, Func<IEnumerable<T>, TOut> mergeFunc)
        {
            List<T> parts = new List<T>();

            foreach (var cur in e)
            {
                if (parts.Count != 0 && !testFunc(parts[parts.Count - 1], cur))
                {
                    yield return mergeFunc(parts);
                    parts.Clear();
                }

                parts.Add(cur);
            }

            if (parts.Count != 0)
            {
                yield return mergeFunc(parts);
                parts.Clear();
            }
        }

        /// <summary>
        /// Intersects multiple collections using the default comparer.
        /// </summary>
        /// <typeparam name="T">The type to intersect.</typeparam>
        /// <param name="collections">A set of collections to intersect.</param>
        /// <returns>A single collection of intersected values.</returns>
        public static IEnumerable<T> Intersect<T>(this IEnumerable<IEnumerable<T>> collections)
        {
            return Intersect(collections, EqualityComparer<T>.Default);
        }

        /// <summary>
        /// Intersects multiple collections.
        /// </summary>
        /// <typeparam name="T">The type to intersect.</typeparam>
        /// <param name="collections">A set of collections to intersect.</param>
        /// <param name="comparer">A comparer to use.</param>
        /// <returns>A single collection of intersected values.</returns>
        public static IEnumerable<T> Intersect<T>(this IEnumerable<IEnumerable<T>> collections, IEqualityComparer<T> comparer)
        {
            return ApplyMultiple(collections, x => x, comparer, (x, y, ke, c) => Enumerable.Intersect(x, y, c));
        }
    }
}
