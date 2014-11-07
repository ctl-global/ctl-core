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
using System.Threading.Tasks;

namespace Ctl.Extensions
{
    /// <summary>
    /// Provides extension methods for System.IO.TextWriter.
    /// </summary>
    public static class TextWriterExtensions
    {
        /// <summary>
        /// Writes a formatted line to a TextWriter.
        /// </summary>
        /// <param name="tw">The TextWriter to write to.</param>
        /// <param name="format">The format string.</param>
        /// <param name="args">Arguments for the format string.</param>
        /// <returns>A task representing the async operation.</returns>
        public static Task WriteLineAsync(this TextWriter tw, string format, params object[] args)
        {
            if (tw == null) throw new ArgumentNullException("tw");
            if (format == null) throw new ArgumentNullException("format");
            if (args == null) throw new ArgumentNullException("args");

            return tw.WriteLineAsync(string.Format(tw.FormatProvider, format, args));
        }

        /// <summary>
        /// Merges a collection of strings, with a separator, into a TextWriter
        /// </summary>
        /// <param name="strings">A collection of strings to merge.</param>
        /// <param name="separator">A separate to put between each string.</param>
        /// <param name="writer">The writer to write to.</param>
        public static void WriteJoined(this TextWriter writer, string separator, IEnumerable<string> strings)
        {
            if (strings == null) throw new ArgumentNullException("strings");
            if (separator == null) throw new ArgumentNullException("separator");
            if (writer == null) throw new ArgumentNullException("writer");

            bool first = true;

            foreach (string s in strings)
            {
                if (s == null)
                {
                    throw new ArgumentNullException("strings", "WriteJoined must not be passed null strings.");
                }

                if (!first)
                {
                    writer.Write(separator);
                }
                else
                {
                    first = false;
                }

                writer.Write(s);
            }
        }
    }
}
