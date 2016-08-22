/*
    Copyright (c) 2016, CTL Global, Inc.
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
    /// <summary>
    /// Implements a thread-safe static Random.
    /// </summary>
    public sealed class StaticRandom : Random
    {
        [ThreadStatic]
        static Random rng;

        static int seed = Environment.TickCount;

        /// <summary>
        /// Gets a Random instance that is thread-safe.
        /// </summary>
        public static Random Instance { get; } = new StaticRandom();

        /// <summary>
        /// Get this thread's instance. The instance itself is not thread-safe and thus shouldn't be given between threads.
        /// </summary>
        /// <remarks>
        /// This property is intended to eliminate TLS lookups in high-perf situations where an instance will be used several times in the same thread.
        /// </remarks>
        public static Random LocalInstance
        {
            get
            {
                if (rng == null)
                {
                    rng = new Random(Interlocked.Increment(ref seed));
                }

                return rng;
            }
        }

        private StaticRandom()
        {
        }

        public override int Next() => LocalInstance.Next();
        public override int Next(int maxValue) => LocalInstance.Next(maxValue);
        public override int Next(int minValue, int maxValue) => LocalInstance.Next(minValue, maxValue);
        public override void NextBytes(byte[] buffer) => LocalInstance.NextBytes(buffer);
        public override double NextDouble() => LocalInstance.NextDouble();
    }
}
