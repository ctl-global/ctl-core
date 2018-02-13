using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ctl
{
    /// <summary>
    /// A semaphore for async workloads.
    /// </summary>
    public sealed class AsyncSemaphore
    {
        long active_waiting;
        readonly int maxActive;
        readonly ConcurrentQueue<ActionContext> actions = new ConcurrentQueue<ActionContext>();

        /// <summary>
        /// Instantiates the async semaphore.
        /// </summary>
        /// <param name="maxActive">The maximum number of active tasks to run at once.</param>
        public AsyncSemaphore(int maxActive)
        {
            if (maxActive < 1) throw new ArgumentOutOfRangeException(nameof(maxActive), $"{nameof(maxActive)} must be at least 1.");
            this.maxActive = maxActive;
        }

        /// <summary>
        /// Runs an action on the semaphore.
        /// </summary>
        /// <param name="action">The action to run.</param>
        /// <param name="token">A cancellation token, used to cancel the action if it can't run in time.</param>
        /// <returns>A task representing the asynchronous action.</returns>
        public Task Run(Func<Task> action, CancellationToken token = default)
        {
            if (action == null) return Task.FromException(new ArgumentNullException(nameof(action)));
            if (token.IsCancellationRequested) return Task.FromCanceled(token);

            async Task<int> Trampoline()
            {
                await action().ConfigureAwait(false);
                return 0;
            }

            return Run(Trampoline, token);
        }

        /// <summary>
        /// Runs an action on the semaphore.
        /// </summary>
        /// <typeparam name="T">The return type of the action.</typeparam>
        /// <param name="action">The action to run.</param>
        /// <param name="token">A cancellation token, used to cancel the action if it can't run in time.</param>
        /// <returns>A task representing the asynchronous action.</returns>
        public Task<T> Run<T>(Func<Task<T>> action, CancellationToken token = default)
        {
            if (action == null) return Task.FromException<T>(new ArgumentNullException(nameof(action)));
            if (token.IsCancellationRequested) return Task.FromCanceled<T>(token);

            ActionContext<T> state = new ActionContext<T>(action, token);
            Task<T> task = state.Task;

            if (token.CanBeCanceled)
            {
                state.Cancellation = token.Register(o => ((ActionContext)o).TryCancel(), state, false);
            }

            actions.Enqueue(state);
            Interlocked.Increment(ref active_waiting);

            TryStartNext();
            return task;
        }

        /// <summary>
        /// Starts the next task, if one is available.
        /// </summary>
        void TryStartNext()
        {
            while (true)
            {
                #region increment our active Task count.

                long old_active_waiting = Volatile.Read(ref active_waiting);

                while (true)
                {
                    int active = (int)(old_active_waiting >> 32);
                    int waiting = (int)(old_active_waiting & int.MaxValue);

                    if (active == maxActive || waiting == 0)
                    {
                        return;
                    }

                    long exch_active_waiting = ((long)(active + 1) << 32) | (long)(waiting - 1);

                    long new_active_waiting = Interlocked.CompareExchange(ref active_waiting, exch_active_waiting, old_active_waiting);

                    if (new_active_waiting != old_active_waiting)
                    {
                        old_active_waiting = new_active_waiting;
                        continue;
                    }

                    break;
                }

                #endregion

                if (!actions.TryDequeue(out ActionContext ctx))
                {
                    throw new Exception($"{nameof(AsyncSemaphore)} has become corrupted; expected available action to dequeue.");
                }

                if (ctx.TryStart())
                {
                    RunAction(ctx);
                    break;
                }

                // context has been cancelled, clean up and loop again.

                Interlocked.Add(ref active_waiting, -(1L << 32));
            }
        }

        /// <summary>
        /// Runs the action, starting a new action once it's finished.
        /// </summary>
        /// <param name="ctx">The context to run.</param>
        void RunAction(ActionContext ctx)
        {
            ctx.Run().ContinueWith((task, state) =>
            {
                var @this = (AsyncSemaphore)state;

                Interlocked.Add(ref @this.active_waiting, -(1L << 32));
                @this.TryStartNext();
            }, this, TaskContinuationOptions.ExecuteSynchronously);
        }

        sealed class ActionContext<T> : ActionContext
        {
            Func<Task<T>> action;
            TaskCompletionSource<T> completionSource = new TaskCompletionSource<T>();

            public ActionContext(Func<Task<T>> action, CancellationToken cancellationToken) : base(cancellationToken)
            {
                this.action = action;
            }

            /// <summary>
            /// Runs the associated action, cleaning up once it's done.
            /// </summary>
            public override Task Run()
            {
                return action().ContinueWith((task, state) =>
                {
                    var @this = (ActionContext<T>)state;

                    if (task.IsFaulted)
                    {
                        @this.completionSource.TrySetException(task.Exception.InnerExceptions);
                    }
                    else
                    {
                        @this.completionSource.TrySetResult(task.Result);
                    }

                    Dispose();
                }, this, TaskContinuationOptions.ExecuteSynchronously);
            }

            /// <summary>
            /// Attempts to cancel the action. If it's already running, it can't be cancelled. If cancelled, also cleans up the action.
            /// </summary>
            public override void TryCancel()
            {
                if (Interlocked.CompareExchange(ref state, ActionContext.STATE_CANCELLED, ActionContext.STATE_PENDING) == ActionContext.STATE_PENDING)
                {
                    completionSource.TrySetCanceled(Token);
                    Dispose();
                }
            }

            /// <summary>
            /// Disposes of resources and cleans up references to allow quicker GC and to avoid any cyclical references.
            /// </summary>
            void Dispose()
            {
                Token = default;

                Cancellation.Dispose();
                Cancellation = default;

                action = null;
                completionSource = null;
            }

            public Task<T> Task => completionSource.Task;
        }

        abstract class ActionContext
        {
            public const int STATE_PENDING = 0;
            public const int STATE_CANCELLED = 1;
            public const int STATE_RUNNING = 2;

            public CancellationTokenRegistration Cancellation;
            protected CancellationToken Token;
            protected int state = STATE_PENDING;

            protected ActionContext(CancellationToken cancellationToken)
            {
                Token = cancellationToken;
            }

            public bool TryStart()
            {
                return Interlocked.CompareExchange(ref state, ActionContext.STATE_RUNNING, ActionContext.STATE_PENDING) == ActionContext.STATE_PENDING;
            }

            public abstract Task Run();
            public abstract void TryCancel();
        }
    }
}
