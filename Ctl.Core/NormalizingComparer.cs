/*
    Copyright (c) 2015, CTL Global, Inc.
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
using System.Threading.Tasks;
using Ctl.Extensions;

namespace Ctl
{
    /// <summary>
    /// Compares strings as if they had normalized whitespace.
    /// </summary>
    public sealed class NormalizingComparer : IComparer<string>, IEqualityComparer<string>
    {
        readonly StringComparer baseComparer;

        /// <summary>
        /// Initializes a normalizing comparer using a base comparer.
        /// </summary>
        /// <param name="baseComparer">The base comparer to use.</param>
        public NormalizingComparer(StringComparer baseComparer)
        {
            this.baseComparer = baseComparer ?? StringComparer.Ordinal;
        }

        public bool Equals(string x, string y)
        {
            return baseComparer.Equals(x.NormalizeWhitespace(), y.NormalizeWhitespace());
        }

        public int GetHashCode(string obj)
        {
            return baseComparer.GetHashCode(obj.NormalizeWhitespace());
        }

        public int Compare(string x, string y)
        {
            return baseComparer.Compare(x.NormalizeWhitespace(), y.NormalizeWhitespace());
        }
    }
}
