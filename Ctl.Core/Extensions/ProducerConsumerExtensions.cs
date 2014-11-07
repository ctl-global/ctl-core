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
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Ctl.Extensions
{
    /// <summary>
    /// Provides extension methods for IProducerConsumerCollection&lt;T&gt;.
    /// </summary>
    public static class ProducerConsumerExtensions
    {
        /// <summary>
        /// Empties a IProducerConsumerCollection&lt;T&gt; into an array.
        /// </summary>
        /// <typeparam name="T">The type of item to pull from the IProducerConsumerCollection&lt;T&gt;.</typeparam>
        /// <param name="bag">The collection to empty.</param>
        /// <returns>An array of items taken from the collection.</returns>
        public static T[] EmptyToArray<T>(this IProducerConsumerCollection<T> bag)
        {
            if (bag == null) throw new ArgumentNullException("bag");

            List<T> items = new List<T>();

            T item;
            while (bag.TryTake(out item))
            {
                items.Add(item);
            }

            return items.ToArray();
        }
    }
}
