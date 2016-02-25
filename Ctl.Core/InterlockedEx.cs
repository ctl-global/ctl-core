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
    /// Provides lock-free operations for primitive types.
    /// </summary>
    public static class InterlockedEx
    {
        /// <summary>
        /// Performs an atomic <c><paramref name="minVal"/> = Math.Min(<paramref name="minVal"/>, <paramref name="val"/>)</c>.
        /// </summary>
        /// <param name="minVal">The value to assign a minimum to.</param>
        /// <param name="val">The new value to assign if less than the current value.</param>
        /// <returns>The new minimum value.</returns>
        public static int ExchangeMin(ref int minVal, int val)
        {
            int oldcmp, curval;

            curval = Volatile.Read(ref minVal);

            do
            {
                if (curval <= val) return curval;
                oldcmp = curval;
            } while ((curval = Interlocked.CompareExchange(ref minVal, val, oldcmp)) != oldcmp);

            return val;
        }

        /// <summary>
        /// Performs an atomic <c><paramref name="maxVal"/> = Math.Max(<paramref name="maxVal"/>, <paramref name="val"/>)</c>.
        /// </summary>
        /// <param name="maxVal">The value to assign a maximum to.</param>
        /// <param name="val">The new value to assign if more than the current value.</param>
        /// <returns>The new maximum value.</returns>
        public static int ExchangeMax(ref int maxVal, int val)
        {
            int oldcmp, curval;

            curval = Volatile.Read(ref maxVal);

            do
            {
                if (curval >= val) return curval;
                oldcmp = curval;
            } while ((curval = Interlocked.CompareExchange(ref maxVal, val, oldcmp)) != oldcmp);

            return val;
        }
    }
}
