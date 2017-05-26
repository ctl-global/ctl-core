/*
    Copyright (c) 2014, CTL Global, Inc.
    Copyright (c) 2014, iD Commerce + Logistics
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
    /// Combines multiple streams into a single stream. Writes are broadcast to all streams.
    /// </summary>
    public class CombiningStream : Stream
    {
        readonly Stream[] streams;
        bool? canSeek, canTimeout;

        public override bool CanRead { get { return false; } }
        public override bool CanWrite { get { return true; } }

        public override bool CanSeek
        {
            get
            {
                if (canSeek == null) canSeek = streams.All(x => x.CanSeek);
                return canSeek.Value;
            }
        }

        public override bool CanTimeout
        {
            get
            {
                if (canTimeout == null) canTimeout = streams.All(x => x.CanTimeout);
                return canTimeout.Value;
            }
        }

        public override long Length { get { return streams.First().Length; } }

        public override long Position
        {
            get
            {
                return streams.First().Position;
            }
            set
            {
                foreach (var s in streams)
                {
                    s.Position = value;
                }
            }
        }

        public override int WriteTimeout
        {
            get
            {
                return streams.First().WriteTimeout;
            }
            set
            {
                foreach (var s in streams)
                {
                    s.WriteTimeout = value;
                }
            }
        }

        /// <summary>
        /// Initializes a new CombiningStream.
        /// </summary>
        /// <param name="streams">The streams to combine.</param>
        public CombiningStream(params Stream[] streams)
            : this(streams.AsEnumerable())
        {
        }

        /// <summary>
        /// Initializes a new CombiningStream.
        /// </summary>
        /// <param name="streams">The streams to combine.</param>
        public CombiningStream(IEnumerable<Stream> streams)
        {
            if (streams == null) throw new ArgumentNullException("streams");

            this.streams = streams.ToArray();

            if (this.streams.Any(x => x == null))
            {
                throw new ArgumentException("Can not combine null streams.", "streams");
            }
        }

#if NET451

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return WriteAsync(buffer, offset, count).ContinueWith(callback, state);
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            if (asyncResult == null) throw new ArgumentNullException("asyncResult");
            ((Task)asyncResult).Wait();
        }

#endif

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return Task.WhenAll(from s in streams
                                select s.WriteAsync(buffer, offset, count, cancellationToken));
        }

        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            return Task.WhenAll(from s in streams
                                select s.FlushAsync(cancellationToken));
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            foreach (var s in streams)
            {
                s.Write(buffer, offset, count);
            }
        }

        public override void WriteByte(byte value)
        {
            foreach (var s in streams)
            {
                s.WriteByte(value);
            }
        }

        public override void Flush()
        {
            foreach (var s in streams)
            {
                s.Flush();
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            long pos = 0;

            foreach (var s in streams)
            {
                pos = s.Seek(offset, origin);
            }

            return pos;
        }

        public override void SetLength(long value)
        {
            foreach (var s in streams)
            {
                s.SetLength(value);
            }
        }

        protected override void Dispose(bool disposing)
        {
            foreach (var s in streams)
            {
                s.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
