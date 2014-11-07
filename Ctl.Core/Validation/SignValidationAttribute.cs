/*
    Copyright (c) 2014, CTL Global, Inc.
    Copyright (c) 2013, iD Commerce + Logistics
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
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ctl.Validation
{
    /// <summary>
    /// A base class for attributes validating the signedness of a value.
    /// </summary>
    public class SignValidationAttribute : ValidationAttribute
    {
        bool allowStrings;

        internal SignValidationAttribute(string errorMessage, bool allowStrings)
            : base(errorMessage)
        {
            this.allowStrings = allowStrings;
        }

        protected int? GetSign(object value)
        {
            return GetSign(value, allowStrings);
        }

        protected static int? GetSign(object value, bool allowStrings)
        {
            switch (Convert.GetTypeCode(value))
            {
                case TypeCode.Byte: return Convert.ToByte(value) > 0 ? 1 : 0;
                case TypeCode.Char: return Convert.ToChar(value) > 0 ? 1 : 0;
                case TypeCode.Decimal: return Math.Sign(Convert.ToDecimal(value));
                case TypeCode.Double: return Math.Sign(Convert.ToDouble(value));
                case TypeCode.Int16: return Math.Sign(Convert.ToInt16(value));
                case TypeCode.Int32: return Math.Sign(Convert.ToInt32(value));
                case TypeCode.Int64: return Math.Sign(Convert.ToInt64(value));
                case TypeCode.SByte: return Math.Sign(Convert.ToSByte(value));
                case TypeCode.Single: return Math.Sign(Convert.ToSingle(value));
                case TypeCode.UInt16: return Convert.ToUInt16(value) > 0 ? 1 : 0;
                case TypeCode.UInt32: return Convert.ToUInt32(value) > 0 ? 1 : 0;
                case TypeCode.UInt64: return Convert.ToUInt64(value) > 0 ? 1 : 0;
                case TypeCode.String:
                    double d;

                    if (allowStrings && double.TryParse(Convert.ToString(value), NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite | NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands | NumberStyles.AllowExponent, NumberFormatInfo.InvariantInfo, out d))
                    {
                        return Math.Sign(d);
                    }

                    return (int?)null;
                default: return (int?)null;
            }
        }
    }

    /// <summary>
    /// Validates that a numeric value is positive.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class PositiveAttribute : SignValidationAttribute
    {
        public PositiveAttribute(bool allowStrings = false) : base("The field {0} must be greater than zero.", allowStrings) { }
        public override bool IsValid(object value) { return (GetSign(value) ?? -1) > 0; }
    }

    /// <summary>
    /// Validates that a numeric value is non-negative.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class NonNegativeAttribute : SignValidationAttribute
    {
        public NonNegativeAttribute(bool allowStrings) : base("The field {0} must be greater than or equal to zero.", allowStrings) { }
        public override bool IsValid(object value) { return (GetSign(value) ?? -1) >= 0; }
    }
}
