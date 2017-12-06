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
using System.Globalization;
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
        /// Trims leading and trailing whitespace from a string, returning null if the result is empty.
        /// </summary>
        /// <param name="s">A string to trim.</param>
        public static string TrimToNull(this string s)
        {
            return string.IsNullOrWhiteSpace(s) ? null : s.Trim();
        }

        /// <summary>
        /// Trims leading and trailing whitespace from a string, returning null if the result is empty.
        /// If the string is over <paramref name="length"/> characters long, it is truncated.
        /// </summary>
        /// <param name="s">A string to truncate.</param>
        /// <param name="length">The maximum length of the string.</param>
        /// <returns>A new string which has at most <paramref name="length"/> characters.</returns>
        public static string TruncateToLength(this string s, int length)
        {
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length), nameof(length) + " must be greater or equal to zero.");
            }

            if (string.IsNullOrWhiteSpace(s))
            {
                return null;
            }

            s = s.Trim();

            if (s.Length > length)
            {
                s = s.Substring(0, length);
            }

            return s;
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

#if !NETSTANDARD1_4
        /// <summary>
        /// Converts ligatures and diacritics into their ASCII equivalents.
        /// </summary>
        /// <param name="s">The string to convert.</param>
        /// <returns>A string with as many non-ASCII characters converted as possible</returns>
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
        public static string StripDiacritics(this string s)
        {
            if (s == null) throw new ArgumentNullException("s");

            s = s.Normalize(NormalizationForm.FormD);

            StringBuilder sb = new StringBuilder(s.Length);

            for (int i = 0; i < s.Length; ++i)
            {
                if (!Unicode.IsCombiningCharacter(s, i))
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
#endif

        /// <summary>
        /// Tests if one string contains another using a specific comparison type.
        /// </summary>
        /// <param name="haystack">The string to search.</param>
        /// <param name="needle">The string to search for.</param>
        /// <param name="comparisonType">The type of comparison to perform.</param>
        /// <returns>If <paramref name="haystack"/> contains <paramref name="needle"/>, true. Otherwise, false.</returns>
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
        /// Removes whitespace from beginning and end of the string, and folds multiple spaces between tokens into single spaces.
        /// </summary>
        /// <param name="s">The string to normalize.</param>
        /// <returns>If non-null, a string with normalized whitespace. Otherwise, an empty string.</returns>
        /// <remarks>This can be thought of as how HTML treats display of whitespace.</remarks>
        public static string NormalizeWhitespace(this string s)
        {
            return s != null ? string.Join(" ", Tokenize(s, (str, i) => char.IsWhiteSpace(str, i))) : string.Empty;
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

            for (int i = 0; i < s.Length; i += Unicode.GetCodePointLength(s, i))
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

        /// <summary>
        /// Truncates a string to a specific number of grapheme clusters (actual visual glyphs).
        /// Special care is taken to not break surrogate pairs and combining characters.
        /// This may result in a string that takes more than <paramref name="length"/> code units of storage.
        /// </summary>
        /// <param name="str">The string to truncate.</param>
        /// <param name="length">The number of grapheme clusters to truncate to.</param>
        /// <param name="ellipsis">If true, an ellipsis will be appended in the event of truncation.</param>
        /// <returns>A truncated string, or the original string if no truncation was necessary.</returns>
        public static string VisualTruncate(this string str, int length, bool ellipsis = false)
        {
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length), "Length must not be negative.");
            }

            if (str == null || str.Length <= length)
            {
                return str;
            }

            for (int i = 0; i < str.Length;)
            {
                int len = Unicode.GetGraphemeClusterLength(str, i);

                if (i + len + (ellipsis ? 1 : 0) > length)
                {
                    string s = str.Substring(0, i);

                    return ellipsis ? (s + "…") : s;
                }

                i += len;
            }

            return str;
        }

        /// <summary>
        /// Splits a string into lines of a specified maximum character length, breaking on whitespace.
        /// </summary>
        /// <param name="str">The string to split.</param>
        /// <param name="lineLength">The maximum character length of each line.</param>
        /// <param name="ellipsis">
        ///    If true, words that are too large for a single line will be split and have two ellipsis inserted between the parts. so "longword" becomes "long...\n...word".
        ///    If false, an exception is thrown.
        ///    Exceptions may be thrown regardless if a single grapheme cluster is too large to fit into a line.
        /// </param>
        /// <returns></returns>
        public static IEnumerable<string> SplitToLines(this string str, int lineLength, bool ellipsis = true)
        {
            if (str == null) throw new ArgumentNullException(nameof(str));
            if (lineLength < (ellipsis ? 3 : 1)) throw new ArgumentOutOfRangeException(nameof(lineLength), $"{nameof(lineLength)} must be greater or equal to {(ellipsis ? 3 : 1)}.");

            StringBuilder sb = new StringBuilder();

            foreach (var tok in Tokenize(str, (s, idx) => Char.IsWhiteSpace(s, idx)))
            {
                int newLen = sb.Length + (sb.Length != 0 ? 1 : 0) + tok.Length;

                if (newLen <= lineLength)
                {
                    // append to existing line.

                    if (sb.Length != 0)
                    {
                        sb.Append(' ');
                    }

                    sb.Append(tok);
                    continue;
                }

                if(sb.Length > 0)
                {
                    // not enough space, yield existing line first.
                    yield return sb.ToString();
                    sb.Clear();
                }

                if (tok.Length < (lineLength - 1))
                {
                    // have enough space now, append to existing line.
                    sb.Append(tok);
                    continue;
                }

                if (tok.Length >= (lineLength - 1) && tok.Length <= lineLength)
                {
                    // the token takes up an entire line -- just yield it directly.
                    yield return tok;
                    continue;
                }

                // the token is too large for a line. it will need to be split, with ellipsis inserted.
                // this will split e.g. "foobarbaz" into:
                // foo…
                // …bar…
                // …baz

                if (!ellipsis) throw new Exception($"The given string has a {tok.Length:N0} character token that will not fit into the {lineLength:N0} character maximum line length.");

                for (int i = 0; i < tok.Length;)
                {
                    int partLen = 0;
                    int clusterLen = 0;

                    for (int j = i; j < tok.Length; j += clusterLen)
                    {
                        clusterLen = Unicode.GetGraphemeClusterLength(tok, j);
                        int newPartLen = partLen + clusterLen;

                        if (newPartLen > (lineLength - 2))
                        {
                            break;
                        }

                        partLen = newPartLen;
                    }

                    if (partLen == 0)
                    {
                        throw new Exception($"The given string has a {tok.Length:N0} character token with a {clusterLen:N0} character grapheme cluster that will not fit into the {lineLength:N0} character maximum line length.");
                    }

                    string tokSplit = tok.Substring(i, partLen);

                    if (i != 0)
                    {
                        // if this isn't the first token, prepend an ellipsis.
                        tokSplit = "…" + tokSplit;
                    }

                    if ((i + partLen) != tok.Length)
                    {
                        // if this isn't the last token, append an ellipsis.
                        tokSplit = tokSplit + "…";
                    }

                    yield return tokSplit;
                    i += partLen;
                }
            }

            if (sb.Length != 0)
            {
                yield return sb.ToString();
            }
        }

        /// <summary>
        /// Capitalizes the first letter of a string using the current culture, and lowercase the rest of it.
        /// </summary>
        /// <param name="s">The string to capitalize.</param>
        /// <returns>A capitalized string.</returns>
        public static string CapitalizeWord(this string s)
        {
            return CapitalizeWord(s, CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Capitalizes the first letter of a string using the invariant culture, and lowercase the rest of it.
        /// </summary>
        /// <param name="s">The string to capitalize.</param>
        /// <returns>A capitalized string.</returns>
        public static string CapitalizeWordInvariant(this string s)
        {
            return CapitalizeWord(s, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Capitalizes the first letter of a string, and lowercase the rest of it.
        /// </summary>
        /// <param name="s">The string to capitalize.</param>
        /// <param name="culture">The culture to use.</param>
        /// <returns>A capitalized string.</returns>
        public static string CapitalizeWord(this string s, CultureInfo culture)
        {
            if (string.IsNullOrEmpty(s)) return s;

            int takeLen = Unicode.GetGraphemeClusterLength(s, 0);

            TextInfo textInfo = culture.TextInfo;
            return textInfo.ToUpper(s.Substring(0, takeLen)) + textInfo.ToLower(s.Substring(takeLen));
        }

        /// <summary>
        /// Tests if a string is upper-case (contains at least one upper-case letter and has no lower-case letters.
        /// </summary>
        /// <param name="s">The string to test.</param>
        /// <returns>If the string is upper-case, true.</returns>
        public static bool IsUpper(this string s)
        {
            if (string.IsNullOrEmpty(s)) return false;

            bool hasUpper = false;

            for (int i = 0; i < s.Length; i += Unicode.GetCodePointLength(s, i))
            {
                if (char.IsLower(s, i))
                {
                    return false;
                }

                if (!hasUpper && char.IsUpper(s, i))
                {
                    hasUpper = true;
                }
            }

            return hasUpper;
        }

        /// <summary>
        /// Tests if a string is lower-case (contains at least one lower-case letter and has no upper-case letters.
        /// </summary>
        /// <param name="s">The string to test.</param>
        /// <returns>If the string is lower-case, true.</returns>
        public static bool IsLower(this string s)
        {
            if (string.IsNullOrEmpty(s)) return false;

            bool hasLower = false;

            for (int i = 0; i < s.Length; i += Unicode.GetCodePointLength(s, i))
            {
                if (char.IsUpper(s, i))
                {
                    return false;
                }

                if (!hasLower && char.IsLower(s, i))
                {
                    hasLower = true;
                }
            }

            return hasLower;
        }

        /// <summary>
        /// Tests if a string is capitalized (that is, the first letter is upper-case and subsequent letters are lower-case.)
        /// </summary>
        /// <param name="s">The string to test.</param>
        /// <returns>If the string is capitalized, true.</returns>
        public static bool IsCapitalized(this string s)
        {
            if (string.IsNullOrEmpty(s)) return false;

            if (!char.IsUpper(s, 0))
            {
                return false;
            }

            for (int i = Unicode.GetCodePointLength(s, 0); i < s.Length; i += Unicode.GetCodePointLength(s, i))
            {
                if (char.IsUpper(s, i))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Splits the string out into a collection of grapheme clusters.
        /// </summary>
        /// <param name="str">The string to get as grapheme clusters.</param>
        /// <returns>A collection of grapheme clusters.</returns>
        public static IEnumerable<StringSegment> AsGraphemeClusters(this string str)
        {
            if (str == null) throw new ArgumentNullException(nameof(str));

            for (int i = 0; i < str.Length;)
            {
                int len = Unicode.GetGraphemeClusterLength(str, i);
                yield return new StringSegment(str, i, len);

                i += len;
            }
        }
    }
}
