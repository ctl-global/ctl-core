using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ctl
{
    /// <summary>
    /// Contains utility methods for supporting Unicode.
    /// </summary>
    public static class Unicode
    {
        /// <summary>
        /// Gets the length, in characters, of the indexed code point.
        /// </summary>
        /// <param name="str">The string to inspect.</param>
        /// <param name="idx">An index pointing to the code point to measure.</param>
        /// <returns>A length, in characters, of the code point.</returns>
        public static int GetCodePointLength(string str, int idx) => char.IsSurrogatePair(str, idx) ? 2 : 1;

        /// <summary>
        /// Determines if the codepoint pointed to by <paramref name="idx"/> is a combining character.
        /// </summary>
        /// <param name="str">The string to test.</param>
        /// <param name="idx">The index of the codepoint to test.</param>
        /// <returns>If the indexed codepoint is a combining character, true. Otherwise, false.</returns>
        public static bool IsCombiningCharacter(string str, int idx)
        {
            var cat = CharUnicodeInfo.GetUnicodeCategory(str, idx);
            return cat >= UnicodeCategory.NonSpacingMark && cat <= UnicodeCategory.EnclosingMark;
        }

        /// <summary>
        /// Gets the length, in characters, of the indexed grapheme cluster.
        /// </summary>
        /// <param name="str">The string to inspect.</param>
        /// <param name="idx">An index pointing to the grapheme cluster to measure.</param>
        /// <returns>A length, in characters, of the grapheme cluster.</returns>
        public static int GetGraphemeClusterLength(string str, int idx)
        {
            if (str == null) throw new ArgumentNullException(nameof(str));
            if (idx < 0) throw new ArgumentOutOfRangeException(nameof(idx));

            for (int i = idx; i < str.Length; i += GetCodePointLength(str, i))
            {
                if (!IsCombiningCharacter(str, i) && i != idx)
                {
                    return i - idx;
                }
            }

            return str.Length - idx;
        }
    }
}
