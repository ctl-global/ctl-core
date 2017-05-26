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

namespace Ctl
{
    /// <summary>
    /// Provides utility methods for working with hashes.
    /// </summary>
    public static class HashUtils
    {
        /// <summary>
        /// Creates a combined, order-dependent hash for multiple items.
        /// </summary>
        /// <param name="items">Items to hash.</param>
        /// <returns>An order-dependent hash for items in <paramref name="items"/></returns>
        public static int MultiHash(IEnumerable<object> items)
        {
            if (items == null) throw new ArgumentNullException("items");

            int h = 0;

            foreach (object item in items)
            {
                h = Combine(h, item != null ? item.GetHashCode() : 0);
            }

            return h;
        }

        /// <summary>
        /// Creates a combined, order-dependent hash for multiple items.
        /// </summary>
        /// <param name="items">Items to hash.</param>
        /// <returns>An order-dependent hash.</returns>
        public static int MultiHash(params object[] items)
        {
            if (items == null) throw new ArgumentNullException("items");

            return MultiHash(items.AsEnumerable());
        }

        /// <summary>
        /// Creates a combined, order-dependent hash from multiple hashes.
        /// </summary>
        /// <param name="hashes">Hashes to combine.</param>
        /// <returns>An order-dependent hash.</returns>
        public static int Combine(IEnumerable<int> hashes)
        {
            if (hashes == null) throw new ArgumentNullException("hashes");

            int h = 0;

            foreach (int hash in hashes)
            {
                h = Combine(h, hash);
            }

            return h;
        }

        /// <summary>
        /// Creates a combined, order-dependent hash from multiple hashes.
        /// </summary>
        /// <param name="hashes">Hashes to combine.</param>
        /// <returns>An order-dependent hash.</returns>
        public static int Combine(params int[] hashes)
        {
            if (hashes == null) throw new ArgumentNullException("hashes");

            return Combine(hashes.AsEnumerable());
        }

        /// <summary>
        /// Creates a combined, order-dependent hash from two hashes.
        /// </summary>
        /// <param name="x">The first hash to combine.</param>
        /// <param name="y">The second hash to combine.</param>
        /// <returns>An order-dependent hash.</returns>
        public static int Combine(int x, int y)
        {
            unchecked
            {
                // This isn't a particularly strong way to combine hashes, but it's
                // cheap, respects ordering, and should work for the majority of cases.
                return (x << 5) + 3 + x ^ y;
            }
        }
    }
}
