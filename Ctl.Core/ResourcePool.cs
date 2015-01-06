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

        /// <summary>
        /// Initializes a resource pool for a disposable type.
        /// </summary>
        /// <param name="getItemFunc">A function that returns the pooled objects. Must be safe to call concurrently.</param>
        public ResourcePool(Func<T> getItemFunc)
        {
            if (getItemFunc == null) throw new ArgumentNullException("getItemFunc");

            this.getItemFunc = getItemFunc;
        }

        /// <summary>
        /// Initializes a resource pool for a disposable type.
        /// </summary>
        /// <param name="getItemFunc">A function that returns the pooled objects. Must be safe to call concurrently.</param>
        /// <param name="initFunc">A function used to initialize the object. Must be safe to call concurrently.</param>
        public ResourcePool(Func<T> getItemFunc, Action<T> initFunc)
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
        }

        /// <summary>
        /// Initializes a resource pool for a disposable type.
        /// </summary>
        /// <param name="getItemFunc">A function that returns the pooled objects. Must be safe to call concurrently.</param>
        /// <param name="initFunc">A function used to initialize the object. Must be safe to call concurrently.</param>
        public ResourcePool(Func<T> getItemFunc, Func<T, CancellationToken, Task> initFunc)
            : this(ct => Task.FromResult(getItemFunc()), initFunc)
        {
            if (getItemFunc == null) throw new ArgumentNullException("getItemFunc");
        }

        /// <summary>
        /// Initializes a resource pool for a disposable type.
        /// </summary>
        /// <param name="getItemFunc">A function that returns the pooled objects. Must be safe to call concurrently.</param>
        public ResourcePool(Func<CancellationToken, Task<T>> getItemFunc)
        {
            if (getItemFunc == null) throw new ArgumentNullException("getItemFunc");

            this.getItemFuncAsync = getItemFunc;
        }

        /// <summary>
        /// Initializes a resource pool for a disposable type.
        /// </summary>
        /// <param name="getItemFunc">A function that returns the pooled objects. Must be safe to call concurrently.</param>
        /// <param name="initFunc">A function used to initialize the object. Must be safe to call concurrently.</param>
        public ResourcePool(Func<CancellationToken, Task<T>> getItemFunc, Func<T, CancellationToken, Task> initFunc)
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

                    if (bag != null)
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
