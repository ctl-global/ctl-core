using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ctl
{
    /// <summary>
    /// A semaphore that schedules work to prevent starvation between multiple domains of a workflow.
    /// </summary>
    public sealed class ScheduledSemaphore : IIntrusiveList<ScheduledSemaphore.WaitList>
    {
        readonly Dictionary<string, WaitList> tasks = new Dictionary<string, WaitList>();
        int releaseCount;

        /// <summary>
        /// Initializes the semaphore, optionally making it available for work immediately.
        /// </summary>
        /// <param name="releaseCount">The initial release count of the semaphore. Must be 0 or greater.</param>
        public ScheduledSemaphore(int releaseCount)
        {
            this.releaseCount = releaseCount >= 0 ? releaseCount : throw new ArgumentOutOfRangeException(nameof(releaseCount));
        }

        WaitList IIntrusiveList<WaitList>.First { get; set; }
        WaitList IIntrusiveList<WaitList>.Last { get; set; }

        /// <summary>
        /// Releases one or more caller waiting on the semaphore.
        /// </summary>
        /// <param name="releaseCount">The number of callers to release.</param>
        public void Release(int releaseCount = 1)
        {
            if (releaseCount < 1) throw new ArgumentOutOfRangeException(nameof(releaseCount));

            lock (tasks)
            {
                while (releaseCount != 0 && TryReleaseOneUnsafe())
                {
                    --releaseCount;
                }

                if (releaseCount != 0)
                {
                    this.releaseCount += releaseCount;
                }
            }
        }

        /// <summary>
        /// Releases one waiter. Is unsafe in that it requires the lock to be held.
        /// </summary>
        bool TryReleaseOneUnsafe()
        {
            Debug.Assert(Monitor.IsEntered(tasks) == true);

            WaitList domainList = IntrusiveList.Front(this);

            if (domainList == null)
            {
                return false;
            }

            WaitTask task = IntrusiveList.Front(domainList);

            bool removed = TryRemoveUnsafe(domainList, task);
            Debug.Assert(removed == true);

            if (!IntrusiveList.IsEmpty(domainList))
            {
                IntrusiveList.Remove(this, domainList);
                IntrusiveList.AddToEnd(this, domainList);
            }

            bool set = task.TrySetResult(true);
            Debug.Assert(set == true);

            return true;
        }

        /// <summary>
        /// Waits for the semaphore to be released, optionally within a timeout.
        /// </summary>
        /// <param name="domain">The domain of this wait operation. If multiple domains are waiting, work is split fairly between them.</param>
        /// <param name="timeout">
        /// The number of milliseconds to wait for the semaphore to be released.
        /// If -1, wait indefinitely.
        /// If 0, the operation will return without waiting if the semaphore can not be acquired immediately.</param>
        /// <param name="cancellationToken">The cancellation token for this operation.</param>
        /// <returns>If the wait was acquired, true. Otherwise if a timeout occurred, false.</returns>
        public Task<bool> WaitAsync(string domain, int timeout = -1, CancellationToken cancellationToken = default)
        {
            if (domain == null) return Task.FromException<bool>(new ArgumentNullException(nameof(domain)));
            if (timeout < -1) return Task.FromException<bool>(new ArgumentOutOfRangeException(nameof(timeout)));
            if (cancellationToken.IsCancellationRequested) return Task.FromCanceled<bool>(cancellationToken);

            WaitList domainList;
            WaitTask node;

            lock (tasks)
            {
                // return synchronously if possible.

                if (releaseCount > 0)
                {
                    --releaseCount;
                    return Task.FromResult(true);
                }

                if (timeout == 0)
                {
                    return Task.FromResult(false);
                }

                // otherwise, get the domain to queue up into.

                if (!tasks.TryGetValue(domain, out domainList))
                {
                    domainList = new WaitList(domain);
                    tasks.Add(domain, domainList);

                    IntrusiveList.AddToEnd(this, domainList);
                }

                // and queue into the domain.

                node = new WaitTask();
                IntrusiveList.AddToEnd(domainList, node);
            }

            return cancellationToken.CanBeCanceled == false && timeout == -1 ? node.Task : WaitCancelableAsync(domainList, node, timeout, cancellationToken);
        }

        /// <summary>
        /// Wraps a basic wait operation with a timeout and a cancellation token.
        /// </summary>
        async Task<bool> WaitCancelableAsync(WaitList domainList, WaitTask node, int timeout, CancellationToken cancellationToken)
        {
            using (var cts = cancellationToken.CanBeCanceled ?
                CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, default) :
                new CancellationTokenSource())
            {
                var task = await Task.WhenAny(node.Task, Task.Delay(timeout, cts.Token)).ConfigureAwait(false);

                if (task == node.Task)
                {
                    cts.Cancel();
                    return true;
                }
            }

            lock (tasks)
            {
                if (TryRemoveUnsafe(domainList, node))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    return false;
                }
            }

            return await node.Task.ConfigureAwait(false);
        }

        /// <summary>
        /// Removes one waiting task from the queue, cleaning up the larger data structure if the task is the last one for that domain.
        /// Is unsafe in that it requires the lock to be held.
        /// </summary>
        bool TryRemoveUnsafe(WaitList list, WaitTask task)
        {
            Debug.Assert(Monitor.IsEntered(tasks) == true);

            if (!IntrusiveList.Remove(list, task))
            {
                return false;
            }

            if (IntrusiveList.IsEmpty(list))
            {
                tasks.Remove(list.Domain);
                IntrusiveList.Remove(this, list);
            }

            return true;
        }

        /// <summary>
        /// A wait list for a single domain.
        /// </summary>
        sealed class WaitList : IIntrusiveList<WaitTask>, IIntrusiveNode<WaitList>
        {
            public string Domain { get; }

            WaitTask IIntrusiveList<WaitTask>.First { get; set; }
            WaitTask IIntrusiveList<WaitTask>.Last { get; set; }

            WaitList IIntrusiveNode<WaitList>.Prev { get; set; }
            WaitList IIntrusiveNode<WaitList>.Next { get; set; }
            bool IIntrusiveNode<WaitList>.IsInList { get; set; }

            public WaitList(string domain)
            {
                this.Domain = domain;
            }
        }

        /// <summary>
        /// A single wait.
        /// </summary>
        sealed class WaitTask : TaskCompletionSource<bool>, IIntrusiveNode<WaitTask>
        {
            WaitTask IIntrusiveNode<WaitTask>.Prev { get; set; }
            WaitTask IIntrusiveNode<WaitTask>.Next { get; set; }
            bool IIntrusiveNode<WaitTask>.IsInList { get; set; }

            public WaitTask()
                : base(TaskCreationOptions.RunContinuationsAsynchronously)
            {
            }
        }
    }
}
