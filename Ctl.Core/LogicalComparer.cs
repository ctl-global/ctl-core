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

namespace Ctl
{
    /// <summary>
    /// Compares strings logically, comparing digits as numbers rather than characters.
    /// This means "2" will sort before "10".
    /// </summary>
    public class LogicalStringComparer : StringComparer
    {
        static readonly StringComparer currentCulture = new LogicalStringComparer(StringComparer.CurrentCulture);
        static readonly StringComparer currentCultureIgnoreCase = new LogicalStringComparer(StringComparer.CurrentCultureIgnoreCase);
        static readonly StringComparer invariantCulture = new LogicalStringComparer(StringComparer.InvariantCulture);
        static readonly StringComparer invariantCultureIgnoreCase = new LogicalStringComparer(StringComparer.InvariantCultureIgnoreCase);
        static readonly StringComparer ordinal = new LogicalStringComparer(StringComparer.Ordinal);
        static readonly StringComparer ordinalIgnoreCase = new LogicalStringComparer(StringComparer.OrdinalIgnoreCase);

        public new static StringComparer CurrentCulture { get { return currentCulture; } }
        public new static StringComparer CurrentCultureIgnoreCase { get { return currentCultureIgnoreCase; } }
        public new static StringComparer InvariantCulture { get { return invariantCulture; } }
        public new static StringComparer InvariantCultureIgnoreCase { get { return invariantCultureIgnoreCase; } }
        public new static StringComparer Ordinal { get { return ordinal; } }
        public new static StringComparer OrdinalIgnoreCase { get { return ordinalIgnoreCase; } }

        readonly StringComparer baseComparer;

        public LogicalStringComparer(StringComparer baseComparer)
        {
            if (baseComparer == null)
            {
                throw new ArgumentNullException("baseComparer");
            }

            this.baseComparer = baseComparer;
        }

        public override int Compare(string x, string y)
        {
            using (var xe = Tokenize(x).GetEnumerator())
            using (var ye = Tokenize(y).GetEnumerator())
            {
                bool xb = xe.MoveNext();
                bool yb = ye.MoveNext();

                while (xb && yb)
                {
                    string xs = xe.Current;
                    string ys = ye.Current;
                    int cmp;

                    if (char.IsDigit(xs[0]) && char.IsDigit(ys[0]))
                    {
                        int len = Math.Max(xs.Length, ys.Length);
                        cmp = string.CompareOrdinal(xs.PadLeft(len, '0'), ys.PadLeft(len, '0'));
                    }
                    else
                    {
                        cmp = baseComparer.Compare(xs, ys);
                    }

                    if (cmp != 0)
                    {
                        return cmp;
                    }

                    xb = xe.MoveNext();
                    yb = ye.MoveNext();
                }

                return (xb ? 1 : 0) - (yb ? 1 : 0);
            }
        }

        public override bool Equals(string x, string y)
        {
            return Compare(x, y) == 0;
        }

        public override int GetHashCode(string obj)
        {
            int h = 0;

            foreach (string s in Tokenize(obj))
            {
                int tokhash = baseComparer.GetHashCode(char.IsDigit(s[0]) ? s.TrimStart('0') : s);

                unchecked
                {
                    h = (h << 5) + 3 + h ^ tokhash;
                }
            }

            return h;
        }

        static IEnumerable<string> Tokenize(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                yield break;
            }

            int startPos = -1;
            bool isPrevDigit = false;

            for (int i = 0; i < s.Length; ++i)
            {
                bool isDigit = char.IsDigit(s[i]);

                if (isPrevDigit != isDigit && startPos != -1)
                {
                    yield return s.Substring(startPos, i - startPos);
                    startPos = i;
                }
                else if (startPos == -1)
                {
                    startPos = i;
                }

                isPrevDigit = isDigit;
            }

            if (startPos != -1)
            {
                yield return s.Substring(startPos);
            }
        }
    }
}
