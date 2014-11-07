/*
    Copyright (c) 2014, CTL Global, Inc.
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
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ctl.Extensions
{
    /// <summary>
    /// Extensions for IAsyncEnumerable.
    /// </summary>
    public static class AsyncEnumerableExtensions
    {
        public static IAsyncEnumerable<T> Prefetch<T>(this IAsyncEnumerable<T> e)
        {
            return AsyncEx.Create(
                ct => Task.FromResult(new PrefetchState<T>()),
                async (state, ct) =>
                {
                    using (ct.Register(o => ((PrefetchState<T>)o).Cancellation.Cancel(), state, false))
                    {
                        if (state.Enumerator == null)
                        {
                            state.Enumerator = e.GetEnumerator();
                            state.NextTask = state.Enumerator.MoveNext(state.Cancellation.Token);
                        }

                        if (!await state.NextTask.ConfigureAwait(false))
                        {
                            return false;
                        }

                        state.Current = state.Enumerator.Current;
                        state.NextTask = state.Enumerator.MoveNext(state.Cancellation.Token);
                        return true;
                    }
                },
                state => state.Current);
        }

        sealed class PrefetchState<T> : IDisposable
        {
            public IAsyncEnumerator<T> Enumerator;
            public T Current;
            public Task<bool> NextTask;
            public CancellationTokenSource Cancellation = new CancellationTokenSource();

            public void Dispose()
            {
                if (Enumerator != null)
                {
                    Enumerator.Dispose();
                    Enumerator = null;
                }

                if (Cancellation != null)
                {
                    Cancellation.Dispose();
                    Cancellation = null;
                }
            }
        }

        /// <summary>
        /// Generates a lookahead sequence, where each element has a window to the next element.
        /// </summary>
        /// <typeparam name="T">The type of sequence to act on.</typeparam>
        /// <param name="e">The sequence to operate on.</param>
        /// <returns>A lookahead sequence.</returns>
        public static IAsyncEnumerable<Lookahead<T>> Lookahead<T>(this IAsyncEnumerable<T> e)
        {
            return AsyncEx.Create(
                ct => Task.FromResult(new LookaheadState<T>()),
                async (state, ct) =>
                {
                    if(!state.HasRun)
                    {
                        state.Enumerator = e.GetEnumerator();
                        state.HasRun = true;

                        state.HasCurrent = await state.Enumerator.MoveNext(ct).ConfigureAwait(false);
                    }

                    bool hasPrev = false;

                    if(state.HasCurrent)
                    {
                        state.Prev = state.Enumerator.Current;
                        hasPrev = true;
                        
                        state.HasCurrent = await state.Enumerator.MoveNext(ct).ConfigureAwait(false);
                    }

                    return hasPrev;
                },
                state => state.HasCurrent ? new Lookahead<T>(state.Prev, state.Enumerator.Current) : new Lookahead<T>(state.Prev));
        }

        sealed class LookaheadState<T> : IDisposable
        {
            public T Prev;
            public IAsyncEnumerator<T> Enumerator;
            public bool HasCurrent, HasRun;

            public void Dispose()
            {
                if(Enumerator != null)
                {
                    Enumerator.Dispose();
                    Enumerator = null;
                    Prev = default(T);
                }
            }
        }
    }

    /// <summary>
    /// A lookahead value.
    /// </summary>
    /// <typeparam name="T">The type to look ahead with</typeparam>
    public sealed class Lookahead<T>
    {
        T next;

        /// <summary>
        /// The current item in the sequence.
        /// </summary>
        public T Current { get; private set; }

        /// <summary>
        /// The next item in the sequence.
        /// </summary>
        public T Next
        {
            get
            {
                if (!HasNext) throw new InvalidOperationException("Unable to retrieve Next item from the last item in a sequence.");
                return next;
            }
            set
            {
                next = value;
            }
        }

        /// <summary>
        /// If true, has a next item.
        /// </summary>
        public bool HasNext { get; private set; }

        /// <summary>
        /// Inititializes a new Lookahead value no next item.
        /// </summary>
        /// <param name="current">The current item in the sequence.</param>
        public Lookahead(T current)
        {
            Current = current;
            HasNext = false;
        }

        /// <summary>
        /// Initializes a new Lookahead value.
        /// </summary>
        /// <param name="current">The current item in the sequence.</param>
        /// <param name="next">The next item in the sequence.</param>
        public Lookahead(T current, T next)
        {
            Current = current;
            Next = next;
            HasNext = true;
        }
    }
}
