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

namespace Ctl
{
    /// <summary>
    /// Buffers items to a fixed-size array.
    /// </summary>
    /// <typeparam name="T">The type to buffer.</typeparam>
    public sealed class ArrayBuffer<T>
    {
        T[] arr;
        int len;

        /// <summary>
        /// Adds an item to the buffer.
        /// </summary>
        /// <param name="item">The item to add.</param>
        public void Add(T item)
        {
            Grow(1, false);
            arr[len] = item;
            ++len;
        }

        /// <summary>
        /// Adds a collection of items to the buffer.
        /// </summary>
        /// <param name="items">Items to add.</param>
        public void AddRange(IEnumerable<T> items)
        {
            if (items == null) throw new ArgumentNullException("items");

            ICollection<T> c = items as ICollection<T>;

            if (c != null && c.Count != 0)
            {
                Grow(c.Count, true);

                c.CopyTo(arr, len);
                len += c.Count;
            }
            else
            {
                foreach (var item in items)
                {
                    Add(item);
                }
            }
        }

        /// <summary>
        /// Retrieves the internal array.
        /// </summary>
        /// <returns>The buffered array.</returns>
        public T[] ToArray()
        {
            if (arr != null)
            {
                if (arr.Length != len)
                {
                    Array.Resize(ref arr, len);
                }

                return arr;
            }

            arr = new T[0];
            return arr;
        }

        void Grow(int size, bool exact)
        {
            if (arr != null && (arr.Length - len) >= size)
            {
                return;
            }

            int capacity =
                len == 0 ? (exact ? size : Math.Max(size, 4)) :
                Math.Max(checked(len + size), checked(len * 4) / 2);

            Array.Resize(ref arr, capacity);
        }
    }
}
