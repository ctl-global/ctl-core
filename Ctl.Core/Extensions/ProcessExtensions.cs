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
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

#if NETSTANDARD2_0 || NET451

namespace Ctl.Extensions
{
    /// <summary>
    /// Provides extension methods for System.Diagnostics.Process.
    /// </summary>
    public static class ProcessExtensions
    {
        /// <summary>
        /// Asynchronously waits for a process to exit.
        /// </summary>
        /// <param name="p">The process to wait for.</param>
        /// <returns>A task representing the wait operation.</returns>
        public static Task WaitForExitAsync(this Process p)
        {
            if (p == null) throw new ArgumentNullException("p");

            return WaitForExitAsync(p, CancellationToken.None);
        }
        /// <summary>
        /// Asynchronously waits for a process to exit.
        /// </summary>
        /// <param name="p">The process to wait for.</param>
        /// <param name="token">A token used to cancel the wait operation.</param>
        /// <returns>A task representing the wait operation.</returns>
        public static async Task WaitForExitAsync(this Process p, CancellationToken token)
        {
            if (p == null) throw new ArgumentNullException("p");

            TaskCompletionSource<int> tcs = new TaskCompletionSource<int>();

            EventHandler eh = (s, e) => tcs.TrySetResult(0);

            p.Exited += eh;

            try
            {
                p.EnableRaisingEvents = true;

                if (p.HasExited)
                {
                    tcs.TrySetResult(0);
                }

                using (token.Register(o => ((TaskCompletionSource<int>)o).TrySetCanceled(), tcs, false))
                {
                    await tcs.Task.ConfigureAwait(false);
                }
            }
            finally
            {
                p.Exited -= eh;
            }
        }
    }
}

#endif