using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ctl
{
    /// <summary>
    /// Utility methods for working with URIs.
    /// </summary>
    public static class UriUtils
    {
        /// <summary>
        /// Escapes a formattable string so that each interpolated expression is URL-encoded.
        /// </summary>
        /// <param name="str">The string to format.</param>
        /// <returns>A formatted string with each interpolated expression URL-encoded.</returns>
        public static string Escape(FormattableString str)
        {
            return str.ToString(EscapedUriFormatProvider.Instance);
        }

        sealed class EscapedUriFormatProvider : IFormatProvider, ICustomFormatter
        {
            public static EscapedUriFormatProvider Instance { get; } = new EscapedUriFormatProvider();

            public string Format(string format, object arg, IFormatProvider formatProvider)
            {
                switch (arg)
                {
                    case null:
                        return string.Empty;
                    case IFormattable fmt:
                        return Uri.EscapeDataString(fmt.ToString(format, CultureInfo.InvariantCulture));
                    case IConvertible cvt:
                        return Uri.EscapeDataString(cvt.ToString(CultureInfo.InvariantCulture));
                    default:
                        return Uri.EscapeDataString(arg.ToString());
                }
            }

            public object GetFormat(Type formatType)
            {
                return formatType == typeof(ICustomFormatter) ? this : null;
            }
        }
    }
}
