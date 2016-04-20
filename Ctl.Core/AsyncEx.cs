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
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ctl
{
    public static class AsyncEx
    {
        public static IAsyncEnumerable<TOut> Create<TState, TOut>(Func<TState> createState, Func<TState, CancellationToken, Task<bool>> moveNext, Func<TState, TOut> getCurrent)
        {
            return Create(ct => Task.FromResult(createState()), moveNext, getCurrent);
        }

        public static IAsyncEnumerable<TOut> Create<TState, TOut>(Func<CancellationToken, Task<TState>> createState, Func<TState, CancellationToken, Task<bool>> moveNext, Func<TState, TOut> getCurrent)
        {
            return new FunctionalAsyncEnumerable<TState, TOut>(createState, moveNext, getCurrent);
        }

        sealed class FunctionalAsyncEnumerable<TState, TOut> : IAsyncEnumerable<TOut>
        {
            readonly Func<CancellationToken, Task<TState>> createState;
            readonly Func<TState, CancellationToken, Task<bool>> moveNext;
            readonly Func<TState, TOut> getCurrent;

            public FunctionalAsyncEnumerable(Func<CancellationToken, Task<TState>> createState, Func<TState, CancellationToken, Task<bool>> moveNext, Func<TState, TOut> getCurrent)
            {
                this.createState = createState;
                this.moveNext = moveNext;
                this.getCurrent = getCurrent;
            }

            public IAsyncEnumerator<TOut> GetEnumerator()
            {
                return new FunctionalAsyncEnumerator<TState, TOut>(createState, moveNext, getCurrent);
            }
        }

        sealed class FunctionalAsyncEnumerator<TState, TOut> : IAsyncEnumerator<TOut>
        {
            Func<CancellationToken, Task<TState>> createState;
            readonly Func<TState, CancellationToken, Task<bool>> moveNext;
            readonly Func<TState, TOut> getCurrent;

            TState state;
            bool disposed;

            public FunctionalAsyncEnumerator(Func<CancellationToken, Task<TState>> createState, Func<TState, CancellationToken, Task<bool>> moveNext, Func<TState, TOut> getCurrent)
            {
                this.createState = createState;
                this.moveNext = moveNext;
                this.getCurrent = getCurrent;
            }

            public TOut Current
            {
                get { return getCurrent(state); }
            }

            public Task<bool> MoveNext(CancellationToken cancellationToken)
            {
                return createState == null ? moveNext(state, cancellationToken) : MoveNextImpl(cancellationToken);
            }

            async Task<bool> MoveNextImpl(CancellationToken cancellationToken)
            {
                if (createState != null)
                {
                    state = await createState(cancellationToken).ConfigureAwait(false);
                    createState = null;
                }

                return await moveNext(state, cancellationToken).ConfigureAwait(false);
            }

            public void Dispose()
            {
                if(!disposed && createState == null)
                {
                    IDisposable disp = state as IDisposable;
                    if (disp != null) disp.Dispose();

                    state = default(TState);
                    disposed = true;
                }
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
