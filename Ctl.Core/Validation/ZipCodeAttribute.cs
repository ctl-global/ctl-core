﻿/*
    Copyright (c) 2014, CTL Global, Inc.
    Copyright (c) 2014, iD Commerce + Logistics
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
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Ctl.Validation
{
    /// <summary>
    /// Validates that a string is a US ZIP code, formatted as either XXXXX or XXXXX-XXXX.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false)]
    public class ZipCodeAttribute : UspsValidationAttribute
    {
        readonly Regex re;

        /// <summary>
        /// Instantiates a zip code validator.
        /// </summary>
        public ZipCodeAttribute()
            : this(false)
        {
        }

        /// <summary>
        /// Instantiates a zip code validator, optionally relaxing validation rules to handle integer zip codes.
        /// </summary>
        /// <param name="assumeLeadingZeroes">If true, zip codes with less than 5 digits will be assumed to have five digits.</param>
        public ZipCodeAttribute(bool assumeLeadingZeroes)
            : base("Field {0} must be a zip code in format XXXXX or XXXXX-XXXX.")
        {
            re = assumeLeadingZeroes ? reWithLeadingZeros : reStrict;
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

            return re.IsMatch(s);
        }

        static readonly Regex reStrict = new Regex(@"^\d{5}(?:(?:-|‒)?\d{4})?$", RegexOptions.Compiled);
        static readonly Regex reWithLeadingZeros = new Regex(@"^(?:(?:\d{5}(?:(?:-|‒)?\d{4})?)|\d{1,4})$", RegexOptions.Compiled);
    }
}
