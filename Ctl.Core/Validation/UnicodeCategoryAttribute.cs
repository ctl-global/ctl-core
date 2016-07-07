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
    /// Validates that a string consists only of code points within specific unicode categories.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = true)]
    public class UnicodeCategoryAttribute : ValidationAttribute
    {
        UnicodeCategory[] categories;

        [CLSCompliant(false)]
        public UnicodeCategoryAttribute(params UnicodeCategory[] categories)
            : base(GetFormatString(categories))
        {
            this.categories = categories.Distinct().OrderBy(x => x).ToArray();
        }

        static string GetFormatString(UnicodeCategory[] categories)
        {
            categories = categories.Distinct().OrderBy(x => x).ToArray();

            string ret = "Field {0} must consist only of code points within the ";

            if (categories.Length == 1)
            {
                ret += "category ";
            }
            else
            {
                ret += "categories ";
            }

            return ret + string.Join(", ", categories);
        }

        public override bool IsValid(object value)
        {
            if (value == null)
            {
                return true;
            }

            string s = value as string;

            if (value == null)
            {
                return false;
            }

            for (int i = 0; i < s.Length; )
            {
                UnicodeCategory cat = CharUnicodeInfo.GetUnicodeCategory(s, i);

                if (Array.BinarySearch(categories, cat) < 0)
                {
                    return false;
                }

                i += char.IsSurrogatePair(s, i) ? 2 : 1;
            }

            return true;
        }
    }
}
