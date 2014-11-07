﻿/*
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
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Ctl.Extensions
{
    /// <summary>
    /// Provides extension methods for String.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Trims all whitespace from a string, returning null if the result is empty.
        /// </summary>
        /// <param name="s"></param>
        [Pure]
        public static string TrimToNull(this string s)
        {
            return string.IsNullOrWhiteSpace(s) ? null : s.Trim();
        }

        /// <summary>
        /// Splits a string into lines.
        /// </summary>
        /// <param name="s">A string to split.</param>
        /// <returns>A collection of lines.</returns>
        public static IEnumerable<string> GetLines(this string s)
        {
            if (s == null) throw new ArgumentNullException("s");

            using (StringReader sr = new StringReader(s))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    yield return line;
                }
            }
        }

        /// <summary>
        /// Converts ligatures and diacritics into their ASCII equivalents.
        /// </summary>
        /// <param name="s">The string to convert.</param>
        /// <returns>A string with as many non-ASCII characters converted as possible</returns>
        [Pure]
        public static string ToAscii(this string s)
        {
            if (s == null) throw new ArgumentNullException("s");

            if (ligaturesRe == null)
            {
                string str;

                using (Stream stream = typeof(StringExtensions).Assembly.GetManifestResourceStream("Ctl.ligatures.txt"))
                using (StreamReader sr = new StreamReader(stream))
                {
                    str = sr.ReadToEnd();
                }

                var data = str
                    .Split((char[])null, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Split(','))
                    .ToArray();

                ligatures = data.ToDictionary(x => x[1], x => x[0]);
                ligaturesRe = new Regex(string.Join("|", data.Select(x => "(?:" + x[1] + ")")), RegexOptions.Compiled);
            }

            s = StripDiacritics(s);
            s = ligaturesRe.Replace(s, m => ligatures[m.Value]);

            return s;
        }

        volatile static Regex ligaturesRe;
        volatile static Dictionary<string, string> ligatures;

        /// <summary>
        /// Strips any diacritics (accents, etc.) from a string.
        /// </summary>
        /// <param name="s">The string to strip.</param>
        /// <returns>A string with diacritics removed.</returns>
        [Pure]
        public static string StripDiacritics(this string s)
        {
            if (s == null) throw new ArgumentNullException("s");

            s = s.Normalize(NormalizationForm.FormD);

            StringBuilder sb = new StringBuilder(s.Length);

            for (int i = 0; i < s.Length; ++i)
            {
                if (char.GetUnicodeCategory(s, i) != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    if (!char.IsSurrogatePair(s, i))
                    {
                        sb.Append(s[i]);
                    }
                    else
                    {
                        sb.Append(s, i, 2);
                        ++i;
                    }
                }
            }

            return sb.ToString().Normalize();
        }

        /// <summary>
        /// Tests if one string contains another using a specific comparison type.
        /// </summary>
        /// <param name="haystack">The string to search.</param>
        /// <param name="needle">The string to search for.</param>
        /// <param name="comparisonType">The type of comparison to perform.</param>
        /// <returns>If <paramref name="haystack"/> contains <paramref name="needle"/>, true. Otherwise, false.</returns>
        [Pure]
        public static bool Contains(this string haystack, string needle, StringComparison comparisonType)
        {
            if (haystack == null) throw new ArgumentNullException("haystack");
            if (needle == null) throw new ArgumentNullException("needle");

            return haystack.IndexOf(needle, comparisonType) >= 0;
        }

        /// <summary>
        /// Appends a formatted line to a StringBuilder using default culture information.
        /// </summary>
        /// <param name="sb">The StringBuilder to append to.</param>
        /// <param name="format">The format string to use.</param>
        /// <param name="args">Arguments to use with <paramref name="format"/>.</param>
        /// <returns>A reference to the StringBuilder instance after the append operation has completed.</returns>
        public static StringBuilder AppendLine(this StringBuilder sb, string format, params object[] args)
        {
            if (sb == null) throw new ArgumentNullException("sb");
            if (format == null) throw new ArgumentNullException("format");
            if (args == null) throw new ArgumentNullException("args");

            return sb.AppendLine(string.Format(format, args));
        }

        /// <summary>
        /// Appends a formatted line to a StringBuilder using <paramref name="provider"/> for culture-specific formatting.
        /// </summary>
        /// <param name="sb">The StringBuilder to append to.</param>
        /// <param name="provider">A format provider to use for culture-specific formatting.</param>
        /// <param name="format">The format string to use.</param>
        /// <param name="args">Arguments to use with <paramref name="format"/>.</param>
        /// <returns>A reference to the StringBuilder instance after the append operation has completed.</returns>
        public static StringBuilder AppendLine(this StringBuilder sb, IFormatProvider provider, string format, params object[] args)
        {
            if (sb == null) throw new ArgumentNullException("sb");
            if (provider == null) throw new ArgumentNullException("provider");
            if (format == null) throw new ArgumentNullException("format");
            if (args == null) throw new ArgumentNullException("args");

            return sb.AppendLine(string.Format(provider, format, args));
        }

        /// <summary>
        /// Splits a string into tokens.
        /// </summary>
        /// <param name="s">The string to split.</param>
        /// <param name="isSeparator">A function testing if a code point at a position in the input string is a separator.</param>
        /// <returns>A sequence of tokens.</returns>
        public static IEnumerable<string> Tokenize(this string s, Func<string, int, bool> isSeparator = null)
        {
            if (isSeparator == null) isSeparator = (str, i) => !char.IsLetterOrDigit(str, i);

            int startPos = -1;

            for (int i = 0; i < s.Length; i += char.IsSurrogatePair(s, i) ? 2 : 1)
            {
                if (!isSeparator(s, i))
                {
                    if (startPos == -1) startPos = i;
                }
                else if (startPos != -1)
                {
                    yield return s.Substring(startPos, i - startPos);
                    startPos = -1;
                }
            }

            if (startPos != -1)
            {
                yield return s.Substring(startPos);
            }
        }
    }
}
