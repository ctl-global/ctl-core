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
using System.Threading.Tasks;

namespace Ctl.Extensions
{
    /// <summary>
    /// Provides extension methods for System.Threading.Tasks.Task.
    /// </summary>
    public static class TaskExtensions
    {
        /// <summary>
        /// Wraps a task, setting its AsyncState
        /// </summary>
        /// <typeparam name="T">The type of the task.</typeparam>
        /// <param name="task">The task to wrap.</param>
        /// <param name="state">The state to use.</param>
        /// <returns>A task with its AsyncState property set to <paramref name="state"/>.</returns>
        public static Task<T> WithState<T>(this Task<T> task, object state)
        {
            if (task == null) throw new ArgumentNullException("task");

            if (object.ReferenceEquals(task.AsyncState, state))
            {
                return task;
            }

            TaskCompletionSource<T> tcs = new TaskCompletionSource<T>(state);

            task.ContinueWith((t, o) =>
            {
                TaskCompletionSource<T> innertcs = (TaskCompletionSource<T>)o;

                if (t.IsFaulted) innertcs.SetException(t.Exception.InnerExceptions);
                else if (t.IsCanceled) innertcs.SetCanceled();
                else innertcs.SetResult(t.Result);
            }, tcs, TaskContinuationOptions.ExecuteSynchronously);

            return tcs.Task;
        }

        /// <summary>
        /// Wraps a task, setting its AsyncState
        /// </summary>
        /// <param name="task">The task to wrap.</param>
        /// <param name="state">The state to use.</param>
        /// <returns>A task with its AsyncState property set to <paramref name="state"/>.</returns>
        public static Task WithState(this Task task, object state)
        {
            if (task == null) throw new ArgumentNullException("task");

            if (object.ReferenceEquals(task.AsyncState, state))
            {
                return task;
            }

            TaskCompletionSource<int> tcs = new TaskCompletionSource<int>(state);

            task.ContinueWith((t, o) =>
            {
                TaskCompletionSource<int> innertcs = (TaskCompletionSource<int>)o;

                if (t.IsFaulted) innertcs.SetException(t.Exception.InnerExceptions);
                else if (t.IsCanceled) innertcs.SetCanceled();
                else innertcs.SetResult(0);
            }, tcs, TaskContinuationOptions.ExecuteSynchronously);

            return tcs.Task;
        }

        /// <summary>
        /// Continues a task with an AsyncCallback and state, for APM compatibility.
        /// </summary>
        /// <typeparam name="T">The type of the task.</typeparam>
        /// <param name="task">The task to continue from.</param>
        /// <param name="callback">The callback to run.</param>
        /// <param name="state">The object to use for AsyncState.</param>
        /// <returns>A task with its AsyncState property set to <paramref name="state"/>.</returns>
        public static Task<T> ContinueWith<T>(this Task<T> task, AsyncCallback callback, object state)
        {
            if (task == null) throw new ArgumentNullException("task");
            
            task = task.WithState(state);

            if (callback != null)
            {
                task.ContinueWith((t, o) => ((AsyncCallback)o)(t), callback, TaskContinuationOptions.ExecuteSynchronously);
            }

            return task;
        }

        /// <summary>
        /// Continues a task with an AsyncCallback and state, for APM compatibility.
        /// </summary>
        /// <param name="task">The task to continue from.</param>
        /// <param name="callback">The callback to run.</param>
        /// <param name="state">The object to use for AsyncState.</param>
        /// <returns>A task with its AsyncState property set to <paramref name="state"/>.</returns>
        public static Task ContinueWith(this Task task, AsyncCallback callback, object state)
        {
            if (task == null) throw new ArgumentNullException("task");

            task = task.WithState(state);

            if (callback != null)
            {
                task.ContinueWith((t, o) => ((AsyncCallback)o)(t), callback, TaskContinuationOptions.ExecuteSynchronously);
            }

            return task;
        }

        /// <summary>
        /// This method does nothing. It is intended to get rid of warning CS4014 ("Because this call is not awaited...") in instances where the user does not intend to use a returned Task.
        /// </summary>
        /// <param name="task">The task to fire and forget.</param>
        public static void FireAndForget(this Task task)
        {
            // do nothing.
        }
    }
}
