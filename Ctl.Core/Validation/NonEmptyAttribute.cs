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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ctl.Validation
{
    /// <summary>
    /// Validates that a collection is non-null and has at least one item.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false)]
    public class NonEmptyAttribute : ValidationAttribute
    {
        public NonEmptyAttribute()
            : base("Field {0} is required and must be non-empty.")
        {
        }

        public override bool IsValid(object value)
        {
            // try to treat it as an ICollection first.

            ICollection c = value as ICollection;

            if (c != null)
            {
                return c.Count != 0;
            }

            // otherwise, try as an IEnumerable.

            IEnumerable e = value as IEnumerable;

            if (e != null)
            {
                IEnumerator ee = null;

                try
                {
                    ee = e.GetEnumerator();
                    return ee.MoveNext();
                }
                finally
                {
                    IDisposable disp;
                    if (ee != null && (disp = ee as IDisposable) != null) disp.Dispose();
                }
            }

            // null values are assumed to be an empty/null collection and invalid.

            return false;
        }
    }
}
