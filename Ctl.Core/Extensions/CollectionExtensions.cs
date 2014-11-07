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
using System.Linq;

namespace Ctl.Extensions
{
    /// <summary>
    /// Provides extension methods for ICollection&lt;T&gt;.
    /// </summary>
    public static class CollectionExtensions
    {
        /// <summary>
        /// Tests if a collection is null or empty.
        /// </summary>
        /// <typeparam name="T">The collection's type.</typeparam>
        /// <param name="set">The collection to test.</param>
        /// <returns>If the collection is null or empty, true. Otherwise, false.</returns>
        public static bool IsNullOrEmpty<T>(this ICollection<T> set)
        {
            return set == null || set.Count == 0;
        }

        /// <summary>
        /// Adds a range of items to a collection.
        /// </summary>
        /// <typeparam name="T">The collection's type.</typeparam>
        /// <param name="set">The collection to add to.</param>
        /// <param name="items">Items to add.</param>
        public static void AddRange<T>(this ICollection<T> set, IEnumerable<T> items)
        {
            if (set == null) throw new ArgumentNullException("set");
            if (items == null) throw new ArgumentNullException("items");

            foreach (T item in items)
            {
                set.Add(item);
            }
        }

        /// <summary>
        /// Removes a range of items from a collection.
        /// </summary>
        /// <typeparam name="T">The type of the collection.</typeparam>
        /// <param name="set">The collection to remove from.</param>
        /// <param name="items">The items to remove.</param>
        /// <returns>An array of booleans indicating if the item at that index was found and removed.</returns>
        public static bool[] RemoveRange<T>(this ICollection<T> set, IEnumerable<T> items)
        {
            if (set == null) throw new ArgumentNullException("set");
            if (items == null) throw new ArgumentNullException("items");

            return items.Select(x => set.Remove(x)).ToArray();
        }

        /// <summary>
        /// Removes a range of items from a collection.
        /// </summary>
        /// <typeparam name="T">The type of the collection.</typeparam>
        /// <param name="set">The collection to remove from.</param>
        /// <param name="items">The items to remove.</param>
        /// <returns>An array of booleans indicating if the item at that index was found and removed.</returns>
        public static bool[] RemoveRange<T>(this ICollection<T> set, params T[] items)
        {
            if (set == null) throw new ArgumentNullException("set");
            if (items == null) throw new ArgumentNullException("items");

            return RemoveRange(set, items.AsEnumerable());
        }

        /// <summary>
        /// Synchronizes the items in two collections, performing a minimal number of operations.
        /// </summary>
        /// <remarks>
        /// This preserves existing items in the collection, so selections are not lost when used on an ObservableCollection.
        /// </remarks>
        public static SyncChanges<T> SyncFrom<T, TKey>(this ICollection<T> c, IEnumerable<T> source, Func<T, TKey> getKey)
        {
            if (c == null) throw new ArgumentNullException("c");
            if (source == null) throw new ArgumentNullException("source");
            if (getKey == null) throw new ArgumentNullException("getKey");

            SyncChanges<T> ret = new SyncChanges<T>();

            HashSet<TKey> keep = new HashSet<TKey>(c.Select(getKey));

            keep.IntersectWith(source.Select(getKey));

            foreach (T item in c.Where(x => !keep.Contains(getKey(x))).ToArray())
            {
                c.Remove(item);
                ret.Removed.Add(item);
            }

            foreach (T item in source.Where(x => !keep.Contains(getKey(x))))
            {
                c.Add(item);
                ret.Added.Add(item);
            }

            return ret;
        }

        /// <summary>
        /// Synchronizes the items in two collections, performing a minimal number of operations.
        /// </summary>
        /// <remarks>
        /// This preserves existing items in the collection, so selections are not lost when used on an ObservableCollection.
        /// </remarks>
        public static SyncChanges<T> SyncFrom<T, TKey>(this ICollection<T> c, IEnumerable<TKey> newKeys, Func<T, TKey> getKey, Func<TKey, T> getObject)
        {
            if (c == null) throw new ArgumentNullException("c");
            if (newKeys == null) throw new ArgumentNullException("newKeys");
            if (getKey == null) throw new ArgumentNullException("getKey");
            if (getObject == null) throw new ArgumentNullException("getObject");

            SyncChanges<T> ret = new SyncChanges<T>();
            HashSet<TKey> keep = new HashSet<TKey>(c.Select(getKey));

            keep.IntersectWith(newKeys);

            foreach (T item in c.Where(x => !keep.Contains(getKey(x))).ToArray())
            {
                c.Remove(item);
                ret.Removed.Add(item);
            }

            foreach (TKey key in newKeys.Except(keep))
            {
                T item = getObject(key);

                c.Add(item);
                ret.Added.Add(item);
            }

            return ret;
        }
    }

    /// <summary>
    /// Describes changes made while syncing a collection.
    /// </summary>
    /// <typeparam name="T">The type of item synced.</typeparam>
    public sealed class SyncChanges<T>
    {
        readonly IList<T> added = new List<T>(), removed = new List<T>();

        /// <summary>
        /// Gets items added during the sync.
        /// </summary>
        public IList<T> Added { get { return added; } }

        /// <summary>
        /// Gets items removed during the sync.
        /// </summary>
        public IList<T> Removed { get { return removed; } }
    }
}
