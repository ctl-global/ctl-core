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
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ctl
{
    /// <summary>
    /// Poor-man's benchmarking.
    /// </summary>
    public static class Benchmark
    {
        /// <summary>
        /// Benchmarks an action.
        /// </summary>
        /// <param name="bundleSize">The bundle size of each item.</param>
        /// <param name="action">The code to benchmark a bundle.</param>
        public static void Run(int bundleSize, Action action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            if (bundleSize <= 0) throw new ArgumentOutOfRangeException(nameof(bundleSize));

            // first, warm up and estimate the number of runs to execute a full second.

            Console.WriteLine("Warming up...");

            long startTicks, endTicks, freq;
            int runs = 0;

            freq = Stopwatch.Frequency;

            Thread.MemoryBarrier();
            startTicks = Stopwatch.GetTimestamp();
            Thread.MemoryBarrier();

            do
            {
                action();

                Thread.MemoryBarrier();
                endTicks = Stopwatch.GetTimestamp();
                Thread.MemoryBarrier();
                ++runs;
            }
            while ((endTicks - startTicks) < freq);

            double itemsPerSec = checked(freq * runs * bundleSize) / (double)(endTicks - startTicks);
            Console.WriteLine($"Warm up completed, using runs of {runs} with estimated {itemsPerSec} items per second.");

            // next, continue to execute that many runs until we get a number of successive runs that can't improve on the time.

            Console.Write("Benchmarking");

            long bestTime = long.MaxValue;
            int tries = 0;

            do
            {
                Thread.MemoryBarrier();
                startTicks = Stopwatch.GetTimestamp();
                Thread.MemoryBarrier();

                for (int i = 0; i < runs; ++i)
                {
                    action();
                }

                Thread.MemoryBarrier();
                endTicks = Stopwatch.GetTimestamp();
                Thread.MemoryBarrier();

                long curTime = endTicks - startTicks;

                if (curTime < bestTime)
                {
                    bestTime = curTime;
                    tries = 1;
                    Console.Write('+');
                }
                else
                {
                    ++tries;
                    Console.Write('.');
                }
            }
            while (tries <= 5);

            Console.WriteLine();

            itemsPerSec = checked(freq * runs * bundleSize) / (double)bestTime;
            Console.WriteLine($"Final time: {itemsPerSec} items per second.");
        }
    }
}
