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
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ctl
{
    /// <summary>
    /// A disposable, thread-safe pool of resources.
    /// </summary>
    /// <typeparam name="T">The type to pool.</typeparam>
    public sealed class ResourcePool<T> : IDisposable where T : IDisposable
    {
        ConcurrentBag<T> items = new ConcurrentBag<T>();

        Func<T> getItemFunc;
        Func<CancellationToken, Task<T>> getItemFuncAsync;
        Predicate<T> testViabilityFunc;

        /// <summary>
        /// Initializes a resource pool for a disposable type.
        /// </summary>
        /// <param name="getItemFunc">A function that returns the pooled objects. Must be safe to call concurrently.</param>
        /// <param name="testViabilityFunc">A predicate that tests if an item should be returned to the pool.</param>
        public ResourcePool(Func<T> getItemFunc, Predicate<T> testViabilityFunc = null)
        {
            if (getItemFunc == null) throw new ArgumentNullException("getItemFunc");

            this.getItemFunc = getItemFunc;
            this.testViabilityFunc = testViabilityFunc;
        }

        /// <summary>
        /// Initializes a resource pool for a disposable type.
        /// </summary>
        /// <param name="getItemFunc">A function that returns the pooled objects. Must be safe to call concurrently.</param>
        /// <param name="initFunc">A function used to initialize the object. Must be safe to call concurrently.</param>
        /// <param name="testViabilityFunc">A predicate that tests if an item should be returned to the pool.</param>
        public ResourcePool(Func<T> getItemFunc, Action<T> initFunc, Predicate<T> testViabilityFunc = null)
        {
            if (getItemFunc == null) throw new ArgumentNullException("getItemFunc");
            if (initFunc == null) throw new ArgumentNullException("initFunc");

            this.getItemFunc = () =>
            {
                T item = getItemFunc();
                if (item == null) throw new InvalidOperationException("GetItem() function must not return null.");

                try
                {
                    initFunc(item);
                    return item;
                }
                catch
                {
                    item.Dispose();
                    throw;
                }
            };

            this.testViabilityFunc = testViabilityFunc;
        }

        /// <summary>
        /// Initializes a resource pool for a disposable type.
        /// </summary>
        /// <param name="getItemFunc">A function that returns the pooled objects. Must be safe to call concurrently.</param>
        /// <param name="initFunc">A function used to initialize the object. Must be safe to call concurrently.</param>
        /// <param name="testViabilityFunc">A predicate that tests if an item should be returned to the pool.</param>
        public ResourcePool(Func<T> getItemFunc, Func<T, CancellationToken, Task> initFunc, Predicate<T> testViabilityFunc = null)
            : this(ct => Task.FromResult(getItemFunc()), initFunc, testViabilityFunc)
        {
            if (getItemFunc == null) throw new ArgumentNullException("getItemFunc");
        }

        /// <summary>
        /// Initializes a resource pool for a disposable type.
        /// </summary>
        /// <param name="getItemFunc">A function that returns the pooled objects. Must be safe to call concurrently.</param>
        /// <param name="testViabilityFunc">A predicate that tests if an item should be returned to the pool.</param>
        public ResourcePool(Func<CancellationToken, Task<T>> getItemFunc, Predicate<T> testViabilityFunc = null)
        {
            if (getItemFunc == null) throw new ArgumentNullException("getItemFunc");

            this.getItemFuncAsync = getItemFunc;
            this.testViabilityFunc = testViabilityFunc;
        }

        /// <summary>
        /// Initializes a resource pool for a disposable type.
        /// </summary>
        /// <param name="getItemFunc">A function that returns the pooled objects. Must be safe to call concurrently.</param>
        /// <param name="initFunc">A function used to initialize the object. Must be safe to call concurrently.</param>
        /// <param name="testViabilityFunc">A predicate that tests if an item should be returned to the pool.</param>
        public ResourcePool(Func<CancellationToken, Task<T>> getItemFunc, Func<T, CancellationToken, Task> initFunc, Predicate<T> testViabilityFunc = null)
        {
            if (getItemFunc == null) throw new ArgumentNullException("getItemFunc");
            if (initFunc == null) throw new ArgumentNullException("initFunc");

            this.getItemFuncAsync = async ct =>
            {
                T item = await getItemFunc(ct).ConfigureAwait(false);
                if (item == null) throw new InvalidOperationException("GetItem() function must not return null.");

                try
                {
                    await initFunc(item, ct).ConfigureAwait(false);
                    return item;
                }
                catch
                {
                    item.Dispose();
                    throw;
                }
            };

            this.testViabilityFunc = testViabilityFunc;
        }

        /// <summary>
        /// Disposes of any pooled objects.
        /// </summary>
        public void Dispose()
        {
            ConcurrentBag<T> bag = Interlocked.Exchange(ref items, null);

            if (bag != null)
            {
                DisposeBag(bag);

                getItemFunc = null;
                getItemFuncAsync = null;
            }
        }

        static void DisposeBag(ConcurrentBag<T> bag)
        {
            T item;

            while (bag.TryTake(out item))
            {
                item.Dispose();
            }
        }

        /// <summary>
        /// Gets a pooled resource synchronously.
        /// </summary>
        /// <returns>A pooled resource.</returns>
        public PooledResource Get()
        {
            if (items == null) throw new ObjectDisposedException("ResourcePool");
            if (getItemFunc == null) throw new InvalidOperationException("Unable to use synchronous Get() when ResourcePool is initialized for async operation.");

            T item;

            if (!items.TryTake(out item))
            {
                item = getItemFunc();

                if (item == null)
                {
                    throw new InvalidOperationException("GetItemFunc() passed to ResourcePool must not return null.");
                }
            }

            return new PooledResource(this, item);
        }

        /// <summary>
        /// Gets a pooled resource asynchronously.
        /// </summary>
        /// <param name="token">A cancellation token.</param>
        /// <returns>A pooled resource.</returns>
        public async Task<PooledResource> GetAsync(CancellationToken token)
        {
            if (items == null) throw new ObjectDisposedException("ResourcePool");
            if (getItemFuncAsync == null) throw new InvalidOperationException("Unable to use asynchronous GetAsync() when ResourcePool is initialized for sync operation.");

            T item;

            if (!items.TryTake(out item))
            {
                item = await getItemFuncAsync(token);

                if (item == null)
                {
                    throw new InvalidOperationException("GetItemFunc() passed to ResourcePool must not return null.");
                }
            }

            return new PooledResource(this, item);
        }

        /// <summary>
        /// Executes an action against a pooled item, then returns the item to the pool.
        /// </summary>
        /// <param name="func">The action to execute.</param>
        public void Execute(Action<T> func)
        {
            if (func == null) throw new ArgumentNullException(nameof(func));

            using (var res = Get())
            {
                func(res.Value);
            }
        }

        /// <summary>
        /// Executes a function against a pooled item, then returns the item to the pool.
        /// </summary>
        /// <param name="func">The function to execute.</param>
        /// <returns>The result of the function.</returns>
        public TResult Execute<TResult>(Func<T, TResult> func)
        {
            if (func == null) throw new ArgumentNullException(nameof(func));

            using (var res = Get())
            {
                return func(res.Value);
            }
        }

        /// <summary>
        /// Executes an action against a pooled item, then returns the item to the pool.
        /// </summary>
        /// <param name="func">The action to execute.</param>
        /// <param name="token">A cancellation token.</param>
        /// <returns>A task representing the async operation.</returns>
        public async Task ExecuteAsync(Func<T, Task> func, CancellationToken token)
        {
            if (func == null) throw new ArgumentNullException(nameof(func));

            using (var res = (getItemFunc != null ? Get() : await GetAsync(token).ConfigureAwait(false)))
            {
                await func(res.Value).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Executes a function against a pooled item, then returns the item to the pool.
        /// </summary>
        /// <param name="func">The function to execute.</param>
        /// <param name="token">A cancellation token.</param>
        /// <returns>The result of the function.</returns>
        public async Task<TResult> ExecuteAsync<TResult>(Func<T, Task<TResult>> func, CancellationToken token)
        {
            if (func == null) throw new ArgumentNullException(nameof(func));

            using (var res = (getItemFunc != null ? Get() : await GetAsync(token).ConfigureAwait(false)))
            {
                return await func(res.Value).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// A handle to a pooled resource.
        /// </summary>
        public struct PooledResource : IDisposable
        {
            ResourcePool<T> pool;
            T value;

            /// <summary>
            /// The pooled object.
            /// </summary>
            public T Value
            {
                get { return value; }
            }

            internal PooledResource(ResourcePool<T> pool, T value)
            {
                Debug.Assert(pool != null);
                Debug.Assert(value != null);

                this.pool = pool;
                this.value = value;
            }

            /// <summary>
            /// Removes the object from the pool, optionally disposing it.
            /// </summary>
            public void RemoveFromPool(bool dispose = true)
            {
                if (pool != null)
                {
                    if (dispose)
                    {
                        value.Dispose();
                    }

                    pool = null;
                    value = default(T);
                }
            }

            /// <summary>
            /// Returns the item to its pool.
            /// </summary>
            public void Dispose()
            {
                if (pool != null)
                {
                    ConcurrentBag<T> bag = Volatile.Read(ref pool.items);

                    if (bag != null && (pool.testViabilityFunc == null || pool.testViabilityFunc(value)))
                    {
                        bag.Add(value);

                        if (Volatile.Read(ref pool.items) == null)
                        {
                            DisposeBag(bag);
                        }
                    }
                    else
                    {
                        value.Dispose();
                    }

                    pool = null;
                    value = default(T);
                }
            }
        }
    }
}
