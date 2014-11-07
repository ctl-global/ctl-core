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
using System.Threading;
using System.Threading.Tasks;

namespace Ctl.Extensions
{
    /// <summary>
    /// Provides extension methods for System.IO.TextReader.
    /// </summary>
    public static class TextReaderExtensions
    {
        /// <summary>
        /// Reads lines from the TextReader.
        /// </summary>
        /// <param name="tr">The TextReader to read from.</param>
        /// <returns>A sequence of lines.</returns>
        public static IEnumerable<string> ReadLines(this TextReader tr)
        {
            if (tr == null) throw new ArgumentNullException("tr");

            string line;

            while ((line = tr.ReadLine()) != null)
            {
                yield return line;
            }
        }

        /// <summary>
        /// Reads all lines from the TextReader.
        /// </summary>
        /// <param name="tr">The TextReader to read from.</param>
        /// <returns>A collection of lines.</returns>
        public static IList<string> ReadAllLines(this TextReader tr)
        {
            if (tr == null) throw new ArgumentNullException("tr");

            List<string> lines = new List<string>();
            string line;

            while ((line = tr.ReadLine()) != null)
            {
                lines.Add(line);
            }

            return lines;
        }

        /// <summary>
        /// Reads all lines from the TextReader.
        /// </summary>
        /// <param name="tr">The TextReader to read from.</param>
        /// <param name="token">A cancellation token.</param>
        /// <returns>A collection of lines.</returns>
        public static async Task<IList<string>> ReadAllLinesAsync(this TextReader tr, CancellationToken token)
        {
            if (tr == null) throw new ArgumentNullException("tr");

            token.ThrowIfCancellationRequested();

            List<string> lines = new List<string>();
            string line;

            while ((line = await tr.ReadLineAsync().ConfigureAwait(false)) != null)
            {
                lines.Add(line);
                token.ThrowIfCancellationRequested();
            }

            return lines;
        }
    }
}
