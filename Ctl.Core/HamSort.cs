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
    /// Extension methods for ordering collections to minimize distance between adjacent items.
    /// </summary>
    public static class HamSort
    {
        /// <summary>
        /// Orders a collection in a way that minimizes the distance between adjacent items.
        /// </summary>
        /// <typeparam name="T">The type to order.</typeparam>
        /// <param name="items">A collection of items to sort.</param>
        /// <param name="getDistance">A function returning the distance between two items.</param>
        /// <param name="groupSize">The size of each group. Trades resources for order efficiency. Memory usage increases exponentially with group size, and CPU usage increases linearly. Pass in -1 for an infinite group size, but be weary.</param>
        /// <returns>A collection of items ordered to minimize distance between adjacent items.</returns>
        /// <remarks>
        /// If left unchecked, the underlying algorithm could consume a huge amount of RAM. To work around this, distinct
        /// items are buffered into groups. A group size of 1024 will consume about 24 MiB of RAM not including the
        /// items themselves. This will result in less efficiency when switching between groups.
        /// </remarks>
        public static IEnumerable<T> OrderByDistance<T>(this IEnumerable<T> items, Func<T, T, int> getDistance, int groupSize = 256)
        {
            return OrderByDistance(items, getDistance, EqualityComparer<T>.Default, groupSize);
        }

        /// <summary>
        /// Orders a collection in a way that minimizes the distance between adjacent items.
        /// </summary>
        /// <typeparam name="T">The type to order.</typeparam>
        /// <param name="items">A collection of items to sort.</param>
        /// <param name="getDistance">A function returning the distance between two items.</param>
        /// <param name="comparer">A comparer for items.</param>
        /// <param name="groupSize">The size of each group. Trades resources for order efficiency. Memory usage increases exponentially with group size, and CPU usage increases linearly. Pass in -1 for an infinite group size, but be weary.</param>
        /// <returns>A collection of items ordered to minimize distance between adjacent items.</returns>
        /// <remarks>
        /// If left unchecked, the underlying algorithm could consume a huge amount of RAM. To work around this, distinct
        /// items are buffered into groups. A group size of 1024 will consume about 24 MiB of RAM not including the
        /// items themselves. This will result in less efficiency when switching between groups.
        /// </remarks>
        public static IEnumerable<T> OrderByDistance<T>(this IEnumerable<T> items, Func<T, T, int> getDistance, IEqualityComparer<T> comparer, int groupSize = 256)
        {
            if (items == null) throw new ArgumentNullException("items");
            if (getDistance == null) throw new ArgumentNullException("getDistance");
            if (comparer == null) throw new ArgumentNullException("comparer");

            if (groupSize < 1)
            {
                return items.GroupBy(x => x, comparer)
                    .ToArray()
                    .OrderByDistanceImpl((x, y) => getDistance(x.Key, y.Key))
                    .SelectMany(x => x);
            }

            return items
                .GroupBy(x => x, comparer)
                .Buffer(groupSize)
                .SelectMany(x => x.Length == 1 ? x : x.OrderByDistanceImpl((y, z) => getDistance(y.Key, z.Key)))
                .SelectMany(x => x);
        }

        /// <summary>
        /// Orders a collection in a way that minimizes the distance between adjacent items.
        /// </summary>
        /// <typeparam name="T">The type to order.</typeparam>
        /// <typeparam name="TKey">The type of key to group on.</typeparam>
        /// <param name="items">A collection of items to sort.</param>
        /// <param name="getDistance">A function returning the distance between two items.</param>
        /// <param name="getKey">Retrieves the key for an item.</param>
        /// <param name="groupSize">The size of each group. Trades resources for order efficiency. Memory usage increases exponentially with group size, and CPU usage increases linearly. Pass in -1 for an infinite group size, but be weary.</param>
        /// <returns>A collection of items ordered to minimize distance between adjacent items.</returns>
        /// <remarks>
        /// If left unchecked, the underlying algorithm could consume a huge amount of RAM. To work around this, distinct
        /// items are buffered into groups. A group size of 1024 will consume about 24 MiB of RAM not including the
        /// items themselves. This will result in less efficiency when switching between groups.
        /// </remarks>
        public static IEnumerable<T> OrderByDistance<T, TKey>(this IEnumerable<T> items, Func<T, T, int> getDistance, Func<T, TKey> getKey, int groupSize = 256)
        {
            return OrderByDistance(items, getDistance, getKey, EqualityComparer<TKey>.Default, groupSize);
        }

        /// <summary>
        /// Orders a collection in a way that minimizes the distance between adjacent items.
        /// </summary>
        /// <typeparam name="T">The type to order.</typeparam>
        /// <typeparam name="TKey">The type of key to group on.</typeparam>
        /// <param name="items">A collection of items to sort.</param>
        /// <param name="getDistance">A function returning the distance between two items.</param>
        /// <param name="getKey">Retrieves the key for an item.</param>
        /// <param name="comparer">A comparer for items.</param>
        /// <param name="groupSize">The size of each group. Trades resources for order efficiency. Memory usage increases exponentially with group size, and CPU usage increases linearly. Pass in -1 for an infinite group size, but be weary.</param>
        /// <returns>A collection of items ordered to minimize distance between adjacent items.</returns>
        /// <remarks>
        /// If left unchecked, the underlying algorithm could consume a huge amount of RAM. To work around this, distinct
        /// items are buffered into groups. A group size of 1024 will consume about 24 MiB of RAM not including the
        /// items themselves. This will result in less efficiency when switching between groups.
        /// </remarks>
        public static IEnumerable<T> OrderByDistance<T, TKey>(this IEnumerable<T> items, Func<T, T, int> getDistance, Func<T, TKey> getKey, IEqualityComparer<TKey> comparer, int groupSize = 256)
        {
            if (items == null) throw new ArgumentNullException("items");
            if (getDistance == null) throw new ArgumentNullException("getDistance");
            if (getKey == null) throw new ArgumentNullException("getKey");
            if (comparer == null) throw new ArgumentNullException("comparer");

            if (groupSize < 1)
            {
                return items.GroupBy(getKey, comparer)
                    .ToArray()
                    .OrderByDistanceImpl((x, y) => getDistance(x.First(), y.First()))
                    .SelectMany(x => x);
            }

            return items
                .GroupBy(getKey, comparer)
                .Buffer(groupSize)
                .SelectMany(x => x.Length == 1 ? x : x.OrderByDistanceImpl((y, z) => getDistance(y.First(), z.First())))
                .SelectMany(x => x);
        }

        /// <summary>
        /// Buffers a collection of items.
        /// </summary>
        /// <typeparam name="T">The type to buffer.</typeparam>
        /// <param name="items">A collection of items to buffer.</param>
        /// <param name="count">The size of each buffer.</param>
        /// <returns>A collection of buffers.</returns>
        /// <remarks>
        /// This is specialized for use with OrderByDistance, and should not be put in general use as-is.
        /// The arrays returned are re-used with each MoveNext(), which could cause confusion.
        /// </remarks>
        static IEnumerable<T[]> Buffer<T>(this IEnumerable<T> items, int count)
        {
            if (items == null) throw new ArgumentNullException("items");
            if (count <= 0) throw new ArgumentOutOfRangeException("count", "count mus be greater than zero.");

            T[] ret = new T[count];
            int pos = 0;

            foreach (T item in items)
            {
                ret[pos] = item;

                if (++pos == count)
                {
                    pos = 0;
                    yield return ret;
                }
            }

            if (pos != 0)
            {
                Array.Resize(ref ret, pos);
                yield return ret;
            }
        }

        /// <summary>
        /// Orders a collection in a way that minimizes the distance between adjacent items.
        /// </summary>
        /// <typeparam name="T">The type to order.</typeparam>
        /// <param name="items">A list of distinct items to sort.</param>
        /// <param name="getDistance">A function returning the distance between two items.</param>
        /// <returns>A collection of items ordered to minimize distance between adjacent items.</returns>
        static IEnumerable<T> OrderByDistanceImpl<T>(this T[] items, Func<T, T, int> getDistance)
        {
            if (items == null) throw new ArgumentNullException("items");
            if (getDistance == null) throw new ArgumentNullException("getDistance");
            if (items.Length <= 0 || items.Length > short.MaxValue) throw new ArgumentOutOfRangeException("count", "count must be greater than 0 and less or equal to 32767.");

            // Create an complete graph, with items as the nodes and the distances between each item as the weighted edges.
            // Note: there are (n^2)/2 edges. This can get very big very quick!

            int item_count = items.Length;
            short[,] adjs = new short[item_count, 2];
            byte[] counts = new byte[item_count];

            {
                int edge_count = item_count * item_count / 2;
                WeightedEdge[] edges = new WeightedEdge[edge_count];

                int edge_no = 0;
                for (int j = 1; j < item_count; ++j)
                {
                    T jv = items[j];

                    for (int i = 0; i < j; ++i)
                    {
                        edges[edge_no++] = new WeightedEdge
                        {
                            Index1 = (short)i,
                            Index2 = (short)j,
                            Weight = Math.Abs(getDistance(items[i], jv))
                        };
                    }
                }

                // Sort the edges by their weight.

                Array.Sort(edges, (x, y) => x.Weight - y.Weight);

                // Initialize our disjoint set.

                for (int i = 0; i < edge_count; ++i)
                {
                    edges[i].ParentIndex = (short)i;
                }

                // Approximate a minimum Hamiltonian path by building a minimum spanning tree using Prim's algorithm.

                int path_len = 0;

                for (int i = 0; i < edge_count; ++i)
                {
                    int eidx1 = edges[i].Index1;
                    int eidx2 = edges[i].Index2;

                    if (counts[eidx1] == 2 || counts[eidx2] == 2)
                    {
                        // this edge would fork the path.
                        continue;
                    }

                    int idx1 = Find(edges, eidx1);
                    int idx2 = Find(edges, eidx2);

                    if (idx1 == idx2)
                    {
                        // this edge would form a cycle.
                        continue;
                    }

                    // disjoint set union.

                    if (edges[idx1].Rank < edges[idx2].Rank)
                    {
                        edges[idx1].ParentIndex = (short)idx2;
                    }
                    else if (edges[idx1].Rank > edges[idx2].Rank)
                    {
                        edges[idx2].ParentIndex = (short)idx1;
                    }
                    else
                    {
                        edges[idx2].ParentIndex = (short)idx1;
                        edges[idx1].Rank++;
                    }

                    // maintain adjacency list.

                    adjs[eidx1, counts[eidx1]++] = (short)eidx2;
                    adjs[eidx2, counts[eidx2]++] = (short)eidx1;

                    // is the path complete?

                    if (++path_len == item_count - 1) break;
                }
            }

            // Iterate the tree.

            int pidx = Array.IndexOf(counts, (byte)1);
            int previdx = -1;

            for (int i = 0; i < item_count; ++i)
            {
                yield return items[pidx];

                {
                    int pidx2 = pidx;

                    pidx = adjs[pidx, counts[pidx] > 1 && adjs[pidx, 1] != previdx ? 1 : 0];
                    previdx = pidx2;
                }
            }
        }

        static int Find(WeightedEdge[] edges, int x)
        {
            int px = edges[x].ParentIndex;

            if (px != x)
            {
                edges[x].ParentIndex = (short)(px = Find(edges, px));
            }

            return px;
        }

        struct WeightedEdge
        {
            public int Weight;
            public short Index1, Index2, Rank, ParentIndex;
        }
    }
}
