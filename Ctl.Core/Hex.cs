using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ctl
{
    /// <summary>
    /// A utility class to convert bytes to and from a hex string representation.
    /// </summary>
    public static class Hex
    {
        /// <summary>
        /// Transforms a byte array into a hex string.
        /// </summary>
        /// <param name="data">The byte array to transform.</param>
        /// <returns>A hex string.</returns>
        public static string GetString(byte[] data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            StringBuilder sb = new StringBuilder(data.Length * 2);

            for (int i = 0; i < data.Length; ++i)
            {
                byte val = data[i];

                sb.Append(ToHexPart(val >> 4));
                sb.Append(ToHexPart(val & 0xF));
            }

            return sb.ToString();
        }

        static char ToHexPart(int val)
        {
            return (char)(val + (val <= 9 ? '0' : 'A' - 10));
        }

        /// <summary>
        /// Transforms a hex string into a byte array.
        /// </summary>
        /// <param name="str">The string to transform.</param>
        /// <returns>A byte array.</returns>
        /// <remarks>
        /// This will result in garbage data if <paramref name="str"/> is not hex.
        /// </remarks>
        public static byte[] GetBytes(string str)
        {
            if (str == null) throw new ArgumentNullException(nameof(str));
            if ((str.Length & 1) != 0) throw new ArgumentOutOfRangeException(nameof(str), str.Length, "String length must be a multiple of two.");

            byte[] data = new byte[str.Length / 2];

            for (int i = 0; i < data.Length; ++i)
            {
                data[i] = (byte)((ToBytePart(str[i * 2]) << 4) | ToBytePart(str[i * 2 + 1]));
            }

            return data;
        }

        static int ToBytePart(char ch)
        {
            // note: this will give random results on invalid input.
            return ch - (ch < 'A' ? '0' : 'A' - 10);
        }
    }
}
