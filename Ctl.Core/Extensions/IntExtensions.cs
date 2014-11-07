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
using System.Diagnostics.Contracts;

namespace Ctl.Extensions
{
    /// <summary>
    /// Extensions for integer types.
    /// </summary>
    public static class IntExtensions
    {
        /// <summary>
        /// Counts the number of bits set in an integer.
        /// </summary>
        /// <param name="v">The integer to test.</param>
        /// <returns>The number of bits set in the specified integer.</returns>
        [Pure, CLSCompliant(false)]
        public static int CountBitsSet(this uint v)
        {
            v = v - ((v >> 1) & 0x55555555);
            v = (v & 0x33333333) + ((v >> 2) & 0x33333333);
            return (int)(((v + (v >> 4) & 0xF0F0F0F) * 0x1010101) >> 24);
        }

        /// <summary>
        /// Counts the number of bits set in an integer.
        /// </summary>
        /// <param name="v">The integer to test.</param>
        /// <returns>The number of bits set in the specified integer.</returns>
        [Pure]
        public static int CountBitsSet(this int v)
        {
            return CountBitsSet((uint)v);
        }
    }
}
