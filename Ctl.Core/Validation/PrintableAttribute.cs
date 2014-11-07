using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ctl.Validation
{
    /// <summary>
    /// Validates that a string contains no unprintable (control) characters other than whitespace.
    /// </summary>
    public sealed class PrintableAttribute : ValidationAttribute
    {
        public PrintableAttribute()
            : base("Field {0} must not contain any unprintable (control) characters other than whitespace.")
        {
        }

        public override bool IsValid(object value)
        {
            if (value == null)
            {
                return true;
            }

            string s = value as string;

            if (s == null)
            {
                return false;
            }

            foreach (char ch in s)
            {
                if (char.IsControl(ch) && !char.IsWhiteSpace(ch))
                    return false;
            }

            return true;
        }
    }
}
