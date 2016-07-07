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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ctl.Extensions;

namespace Ctl
{
    /// <summary>
    /// Creates a proxy stream. When written to, the data can be read from another stream.
    /// </summary>
    class ProxyStream : Stream
    {
        readonly ProxyStreamReader reader = new ProxyStreamReader();

        /// <summary>
        /// The read-only stream which receives data written to this one.
        /// </summary>
        public Stream ReadStream
        {
            get { return reader; }
        }

        public ProxyStream()
        {
        }

        public override bool CanRead
        {
            get { return false; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return true; }
        }

        public override void Flush()
        {
        }

        public override long Length
        {
            get { throw new NotImplementedException(); }
        }

        public override long Position
        {
            get
            {
                return reader.WritePosition;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            reader.WriteAsyncImpl(buffer, offset, count, CancellationToken.None).Wait();
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return reader.WriteAsyncImpl(buffer, offset, count, cancellationToken);
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return reader.WriteAsyncImpl(buffer, offset, count, CancellationToken.None).ContinueWith(callback, state);
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            ((Task)asyncResult).Wait();
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            reader.SetCompletedImpl();
        }
    }

    sealed class ProxyStreamReader : Stream
    {
        object gate = new object();

        TaskCompletionSource<int> readTask, writeTask;
        CancellationTokenRegistration readCancel, writeCancel;

        byte[] readBuf, writeBuf;
        int readOffset, readCount, writeOffset, writeCount, writeTotalCount;

        long lifetimeReadCount, lifetimeWriteCount;
        bool isCompleted;

        void DoWriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken, out TaskCompletionSource<int> taskToComplete, out int bytesRead, out Task taskToReturn)
        {
            if (isCompleted)
            {
                throw new ObjectDisposedException("ProxyStream");
            }

            if (writeTask != null)
            {
                throw new InvalidOperationException("Concurrent operations are not supported. There is already an outstanding write.");
            }

            taskToComplete = null;
            bytesRead = 0;

            if (count == 0)
            {
                taskToReturn = Task.FromResult(false);
                return;
            }

            writeTotalCount = count;

            if (readTask != null)
            {
                // there's an outstanding read. write to that.

                int len = Math.Min(count, readCount);
                Buffer.BlockCopy(buffer, offset, readBuf, readOffset, len);

                // read is done, clean up state and complete the read task.

                taskToComplete = readTask;
                bytesRead = len;

                readBuf = null;
                readTask = null;

                readCancel.Dispose();
                readCancel = default(CancellationTokenRegistration);

                offset += len;
                count -= len;

                lifetimeReadCount += len;

                if (count == 0)
                {
                    // write was fulfilled synchronously.

                    lifetimeWriteCount += writeTotalCount;
                    taskToReturn = Task.FromResult(false);
                    return;
                }
            }

            // no outstanding reads, create a task for the write.

            writeTask = new TaskCompletionSource<int>();
            writeCancel = RegisterCancel(writeTask, cancellationToken);

            writeBuf = buffer;
            writeOffset = offset;
            writeCount = count;

            taskToReturn = writeTask.Task;
        }

        void DoReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken, out TaskCompletionSource<int> taskToComplete, out Task<int> taskToReturn)
        {
            if (readTask != null)
            {
                throw new InvalidOperationException("Concurrent operations are not supported. There is already an outstanding read.");
            }

            taskToComplete = null;

            if (count == 0)
            {
                taskToReturn = Task.FromResult(0);
                return;
            }

            if (writeTask != null)
            {
                // there's an outstanding write. take from that.

                int len = Math.Min(count, writeCount);

                Buffer.BlockCopy(writeBuf, writeOffset, buffer, offset, len);
                writeOffset += len;
                writeCount -= len;

                lifetimeReadCount += len;

                if (writeCount == 0)
                {
                    // write is done, clean up state and complete the write task.

                    taskToComplete = writeTask;

                    writeBuf = null;
                    writeTask = null;

                    writeCancel.Dispose();
                    writeCancel = default(CancellationTokenRegistration);

                    lifetimeWriteCount += writeTotalCount;
                }

                // read was fulfilled synchronously.

                taskToReturn = Task.FromResult(len);
                return;
            }

            if (isCompleted)
            {
                taskToReturn = Task.FromResult(0);
                return;
            }

            // no outstanding writes, create a task for the read.

            readTask = new TaskCompletionSource<int>();
            readCancel = RegisterCancel(readTask, cancellationToken);

            readBuf = buffer;
            readOffset = offset;
            readCount = count;

            taskToReturn = readTask.Task;
        }

        static CancellationTokenRegistration RegisterCancel(TaskCompletionSource<int> tcs, CancellationToken cancellationToken)
        {
            return cancellationToken.Register(o => ((TaskCompletionSource<int>)o).TrySetCanceled(), tcs, false);
        }

        internal ProxyStreamReader()
        {
        }

        internal void SetCompletedImpl()
        {
            TaskCompletionSource<int> task = null;

            lock (gate)
            {
                if (isCompleted)
                {
                    return;
                }

                if (writeTask != null)
                {
                    throw new InvalidOperationException("ProxyStream can not be closed while there is an outstanding write.");
                }

                isCompleted = true;

                task = readTask;

                readCancel.Dispose();
                readCancel = default(CancellationTokenRegistration);
                readTask = null;
            }

            if (task != null)
            {
                // end of stream.
                task.TrySetResult(0);
            }
        }

        internal Task WriteAsyncImpl(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (buffer == null) throw new ArgumentNullException("buffer");
            if (offset < 0) throw new ArgumentOutOfRangeException("offset", "offset must not be negative.");
            if (count < 0) throw new ArgumentOutOfRangeException("count", "count must not be negative.");
            if (offset + count > buffer.Length) throw new ArgumentOutOfRangeException("buffer", "buffer is not large enough to satisfy count.");

            TaskCompletionSource<int> taskToComplete;
            int bytesRead;
            Task taskToReturn;

            lock (gate)
            {
                DoWriteAsync(buffer, offset, count, cancellationToken, out taskToComplete, out bytesRead, out taskToReturn);
            }

            if (taskToComplete != null)
            {
                taskToComplete.TrySetResult(bytesRead);
            }

            return taskToReturn;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return ReadAsync(buffer, offset, count, CancellationToken.None).Result;
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return ReadAsync(buffer, offset, count, CancellationToken.None).ContinueWith(callback, state);
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            return ((Task<int>)asyncResult).Result;
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (buffer == null) throw new ArgumentNullException("buffer");
            if (offset < 0) throw new ArgumentOutOfRangeException("offset", "offset must not be negative.");
            if (count < 0) throw new ArgumentOutOfRangeException("count", "count must not be negative.");
            if (offset + count > buffer.Length) throw new ArgumentOutOfRangeException("buffer", "buffer is not large enough to satisfy count.");

            Task<int> ret;
            TaskCompletionSource<int> tcs;

            lock (gate)
            {
                DoReadAsync(buffer, offset, count, cancellationToken, out tcs, out ret);
            }

            if (tcs != null)
            {
                tcs.TrySetResult(0);
            }

            return ret;
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            TaskCompletionSource<int> t1, t2;

            lock (gate)
            {
                isCompleted = true;

                t1 = readTask;

                readCancel.Dispose();
                readCancel = default(CancellationTokenRegistration);
                readTask = null;

                t2 = writeTask;

                writeCancel.Dispose();
                writeCancel = default(CancellationTokenRegistration);
                writeTask = null;
            }

            if (t1 != null || t2 != null)
            {
                ObjectDisposedException ex = new ObjectDisposedException("ProxyStream");

                if (t1 != null) t1.TrySetException(ex);
                if (t2 != null) t2.TrySetException(ex);
            }
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override long Length
        {
            get { throw new NotImplementedException(); }
        }

        public override long Position
        {
            get
            {
                return lifetimeReadCount;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        internal long WritePosition
        {
            get { return lifetimeWriteCount; }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }
}
