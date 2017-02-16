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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ctl
{
    public static class AsyncEx
    {
        public static readonly Task<bool> TrueTask = Task.FromResult(true);
        public static readonly Task<bool> FalseTask = Task.FromResult(false);

        [Obsolete, EditorBrowsable(EditorBrowsableState.Never)]
        public static IAsyncEnumerable<TOut> Create<TState, TOut>(Func<TState> createState, Func<TState, CancellationToken, Task<bool>> moveNext, Func<TState, TOut> getCurrent)
        {
            return AsyncEnumerable.Defer(() =>
            {
                var state = createState();
                return Create(ct => moveNext(state, ct), () => getCurrent(state));
            });
        }

        [Obsolete, EditorBrowsable(EditorBrowsableState.Never)]
        public static IAsyncEnumerable<TOut> Create<TState, TOut>(Func<CancellationToken, Task<TState>> createState, Func<TState, CancellationToken, Task<bool>> moveNext, Func<TState, TOut> getCurrent)
        {
            return AsyncEnumerable.Defer(() =>
            {
                TState state = default(TState);
                bool isInit = false;

                return Create(ct =>
                {
                    if (isInit) return moveNext(state, ct);

                    return createState(ct)
                        .ContinueWith(tstate =>
                        {
                            state = tstate.Result;
                            return moveNext(state, ct);
                        }, TaskContinuationOptions.ExecuteSynchronously)
                        .Unwrap();
                }, () => getCurrent(state));
            });
        }

        public static IAsyncEnumerable<TOut> Create<TOut>(Func<CancellationToken, Task<bool>> moveNext, Func<TOut> getCurrent)
        {
            return new FunctionalAsyncEnumerable<TOut>(moveNext, getCurrent);
        }

        sealed class FunctionalAsyncEnumerable<TOut> : IAsyncEnumerable<TOut>
        {
            readonly Func<CancellationToken, Task<bool>> moveNext;
            readonly Func<TOut> getCurrent;

            public FunctionalAsyncEnumerable(Func<CancellationToken, Task<bool>> moveNext, Func<TOut> getCurrent)
            {
                this.moveNext = moveNext;
                this.getCurrent = getCurrent;
            }

            public IAsyncEnumerator<TOut> GetEnumerator() => new FunctionalAsyncEnumerator<TOut>(moveNext, getCurrent);
        }

        sealed class FunctionalAsyncEnumerator<TOut> : IAsyncEnumerator<TOut>
        {
            Func<CancellationToken, Task<bool>> moveNext;
            Func<TOut> getCurrent;

            public FunctionalAsyncEnumerator(Func<CancellationToken, Task<bool>> moveNext, Func<TOut> getCurrent)
            {
                this.moveNext = moveNext;
                this.getCurrent = getCurrent;
            }

            public TOut Current => getCurrent();

            public Task<bool> MoveNext(CancellationToken cancellationToken) => moveNext(cancellationToken);

            public void Dispose()
            {
                moveNext = null;
                getCurrent = null;
            }
        }

        /// <summary>
        /// Wraps an async Main() function, providing cancellation support with CTRL+C.
        /// </summary>
        /// <param name="func">The main function to run.</param>
        public static void RunMain(Func<string[], CancellationToken, Task> func)
        {
            using (CancellationTokenSource cts = new CancellationTokenSource())
            {
                Console.CancelKeyPress += (s, e) =>
                {
                    Console.Error.WriteLine("Cancellation pending...");

                    e.Cancel = true;
                    cts.Cancel();
                };

                RunMainImpl(func, cts.Token).Wait();
            }
        }

        static async Task RunMainImpl(Func<string[], CancellationToken, Task> func, CancellationToken token)
        {
            try
            {
                await func(Environment.GetCommandLineArgs(), token).ConfigureAwait(false);
            }
            catch (OperationCanceledException ex) when (ex.CancellationToken == token)
            {
                Console.Error.WriteLine("Operation cancelled.");
            }
        }
    }
}
