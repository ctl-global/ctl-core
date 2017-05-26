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

#if NET451

using Ctl.Extensions;
using System;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace Ctl
{
    /// <summary>
    /// A utility class to ease async operations across remote app domain boundaries.
    /// </summary>
    public static class RemoteTask
    {
        /// <summary>
        /// Completes a remote task. Must be called from the client.
        /// </summary>
        /// <typeparam name="T">The type returned by the remote task.</typeparam>
        /// <param name="remoteTask">The remote task to start.</param>
        /// <param name="cancellationToken">A cancellation token to pass to the remote task.</param>
        /// <returns>A task representing the remote async operation.</returns>
        public static async Task<T> ClientComplete<T>(RemoteTask<T> remoteTask, CancellationToken cancellationToken)
        {
            Contract.Requires(remoteTask != null);
            Contract.Ensures(Contract.Result<Task<T>>() != null);

            using (cancellationToken.Register(remoteTask.Cancel))
            {
                RemoteTaskCompletionSource<T> tcs = new RemoteTaskCompletionSource<T>();
                remoteTask.Complete(tcs);

                return await tcs.Task.ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Starts a remote task. Must be called from the server.
        /// </summary>
        /// <typeparam name="T">The type returned by the function.</typeparam>
        /// <param name="func">The asynchronous function</param>
        /// <returns>A remote task for handing to the client, representing the operation.</returns>
        public static RemoteTask<T> ServerStart<T>(Func<CancellationToken, Task<T>> func)
        {
            Contract.Requires(func != null);
            Contract.Ensures(Contract.Result<RemoteTask<T>>() != null);
            return new RemoteTask<T>(func);
        }
    }

    /// <summary>
    /// Represents a remote asynchronous operation.
    /// </summary>
    /// <typeparam name="T">The type of value returned by the operation.</typeparam>
    public sealed class RemoteTask<T> : MarshalByRefObject
    {
        readonly CancellationTokenSource cts = new CancellationTokenSource();
        readonly Task<T> task;

        internal RemoteTask(Func<CancellationToken, Task<T>> starter)
        {
            Contract.Requires(starter != null);
            this.task = starter(cts.Token);
        }

        /// <summary>
        /// Binds a task completion source to inherit the results of this task.
        /// </summary>
        /// <param name="tcs">A task completion source to set upon completion of this task.</param>
        internal void Complete(RemoteTaskCompletionSource<T> tcs)
        {
            Contract.Requires(tcs != null);

            task.ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    tcs.TrySetException(t.Exception.ToSingle());
                }
                else if (t.IsCanceled)
                {
                    tcs.TrySetCancelled();
                }
                else
                {
                    tcs.TrySetResult(t.Result);
                }
            }, TaskContinuationOptions.ExecuteSynchronously);
        }

        internal void Cancel()
        {
            cts.Cancel();
        }

        [ContractInvariantMethod]
        void Invariants()
        {
            Contract.Invariant(cts != null);
            Contract.Invariant(task != null);
        }
    }

    sealed class RemoteTaskCompletionSource<T> : MarshalByRefObject
    {
        readonly TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();

        public bool TrySetResult(T result) { return tcs.TrySetResult(result); }
        public bool TrySetCancelled() { return tcs.TrySetCanceled(); }
        public bool TrySetException(Exception ex) { return tcs.TrySetException(ex); }

        public Task<T> Task
        {
            get
            {
                Contract.Ensures(Contract.Result<Task<T>>() != null);
                return tcs.Task;
            }
        }

        [ContractInvariantMethod]
        void Invariants()
        {
            Contract.Invariant(tcs != null);
        }
    }
}

#endif