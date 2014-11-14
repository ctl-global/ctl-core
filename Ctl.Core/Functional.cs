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
using System.Diagnostics.Contracts;

namespace Ctl
{
    /// <summary>
    /// Methods for creating interface wrappers around functional comparers.
    /// </summary>
    public static class Functional
    {
        /// <summary>
        /// Creates an IComparer(Of T) which wraps a delegate.
        /// </summary>
        /// <typeparam name="T">The type of item to compare.</typeparam>
        /// <param name="comparer">A function comparing two items.</param>
        /// <returns>An IComparer(Of T) wrapping <paramref name="comparer"/>.</returns>
        [Obsolete("This method is obsolete. Comparer<T>.Create should be used instead.")]
        public static IComparer<T> CreateComparer<T>(Comparison<T> comparer)
        {
            if (comparer == null) throw new ArgumentNullException("comparer");
            return Comparer<T>.Create(comparer);
        }

        /// <summary>
        /// Creates an IEqualityComparer(Of T) which wraps delegates.
        /// </summary>
        /// <typeparam name="T">The type of item to compare.</typeparam>
        /// <param name="comparer">A function comparing two items.</param>
        /// <param name="hasher">A function which hashes items.</param>
        /// <returns>An IEqualityComparer(Of T) wrapping <paramref name="comparer"/> and <paramref name="hasher"/>.</returns>
        public static IEqualityComparer<T> CreateComparer<T>(Func<T, T, bool> comparer, Func<T, int> hasher)
        {
            if (comparer == null) throw new ArgumentNullException("comparer");
            if (hasher == null) throw new ArgumentNullException("hasher");

            return new FunctionalEqualityComparer<T>(comparer, hasher);
        }

        sealed class FunctionalEqualityComparer<T> : IEqualityComparer<T>
        {
            readonly Func<T, T, bool> comparer;
            readonly Func<T, int> hasher;

            public FunctionalEqualityComparer(Func<T, T, bool> comparer, Func<T, int> hasher)
            {
                this.comparer = comparer;
                this.hasher = hasher;
            }

            public bool Equals(T x, T y)
            {
                return comparer(x, y);
            }

            public int GetHashCode(T obj)
            {
                return hasher(obj);
            }
        }
    }
}
