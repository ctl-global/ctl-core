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

using Ctl.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Ctl
{
    /// <summary>
    /// Utility methods to validate objects using data annotations.
    /// </summary>
    public static class ValidatorEx
    {
        /// <summary>
        /// Validates an object to conform to data annotations.
        /// </summary>
        /// <param name="o">The object to validate.</param>
        /// <exception cref="Ctl.ObjectValidationException">Thrown when <paramref name="o"/> does not conform to data annotations.</exception>
        public static void Validate(object o)
        {
            List<ValidationResult> results = new List<ValidationResult>();
            if (!TryValidate(o, results))
            {
                throw new ObjectValidationException(results, o);
            }
        }

        /// <summary>
        /// Validates an object to conform to data annotations.
        /// </summary>
        /// <param name="o">The object to validate.</param>
        /// <param name="results">The validation results.</param>
        /// <param name="recurseComplexTypes">If true, any members with a type annotated with ComplexTypeAttribute will also be checked.</param>
        /// <returns>If the object is valid, true. Otherwise, false.</returns>
        public static bool TryValidate(object o, ICollection<ValidationResult> results, bool recurseComplexTypes = true)
        {
            if (results == null) throw new ArgumentNullException("results");

            if (o == null)
            {
                return true;
            }

            return TryValidateSubFull(o, string.Empty, results, recurseComplexTypes);
        }

        static Regex re = new Regex("The (((?<f>\\S+) field)|(field (?<f>\\S+)))");

        /// <summary>
        /// Validates an object to conform to data annotations, maintaining an object path.
        /// </summary>
        /// <param name="o">The object to validate.</param>
        /// <param name="path">The path taken to get to this object.</param>
        /// <param name="results">The validation results.</param>
        /// <param name="recurseComplexTypes">If true, any members with a type annotated with ComplexTypeAttribute will also be checked.</param>
        /// <returns>If the object is valid, true. Otherwise, false.</returns>
        static bool TryValidateSubFull(object o, string path, ICollection<ValidationResult> results, bool recurseComplexTypes)
        {
            List<ValidationResult> subresults = new List<ValidationResult>();

            // first, validate this object.

            bool isOkay = Validator.TryValidateObject(o, new ValidationContext(o), subresults, true);
            string repstr = "The " + path + (path.Length > 0 ? "." : string.Empty) + "${f} field";
            foreach (ValidationResult res in subresults)
            {
                results.Add(new ValidationResult(re.Replace(res.ErrorMessage, repstr), res.MemberNames.Select(x => path + "." + x)));
            }

            if (!recurseComplexTypes)
            {
                return isOkay;
            }

            // next, recurse into its members. this works for properties or fields.

            foreach (MemberInfo mi in o.GetType().GetMembers())
            {
                PropertyInfo pi = mi as PropertyInfo;
                object so = null;

                if (pi != null && pi.GetGetMethod() != null)
                {
                    so = pi.GetGetMethod().Invoke(o, null);
                }

                FieldInfo fi = mi as FieldInfo;

                if (fi != null)
                {
                    so = fi.GetValue(o);
                }

                if (so != null)
                {
                    // Check to see if the member's object is a collection or complex type.

                    ICollection c = so as ICollection;

                    if (c != null)
                    {
                        int idx = 0;
                        foreach (object sao in c)
                        {
                            if (sao.GetType().GetCustomAttribute<ComplexTypeAttribute>() != null)
                            {
                                isOkay = isOkay && TryValidateSubFull(so, path + mi.Name + "[" + idx.ToString() + "]", results, recurseComplexTypes);
                            }

                            ++idx;
                        }
                    }
                    else if (so.GetType().GetCustomAttribute<ComplexTypeAttribute>() != null)
                    {
                        isOkay = isOkay && TryValidateSubFull(so, path + mi.Name, results, recurseComplexTypes);
                    }
                }
            }

            return isOkay;
        }
    }

    /// <summary>
    /// An exception thrown when an object fails validation.
    /// </summary>
    public class ObjectValidationException : ValidationException
    {
        /// <summary>
        /// The reasons for the object failing validation.
        /// </summary>
        public IEnumerable<ValidationResult> Results { get; private set; }

        /// <summary>
        /// Creates a new instance of ObjectValidationException.
        /// </summary>
        /// <param name="results">Specific reasons for validation failure.</param>
        /// <param name="value">The value which caused validation to fail.</param>
        public ObjectValidationException(IEnumerable<ValidationResult> results, object value)
            : this(CreateMessage(results), results, value)
        {
        }

        static string CreateMessage(IEnumerable<ValidationResult> results)
        {
            if (results == null) throw new ArgumentNullException("results");

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("The object is not valid:");

            foreach (var res in results)
            {
                sb.AppendLine(string.Join(",", res.MemberNames) + ": " + res.ErrorMessage);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Creates a new instance of ObjectValidationException.
        /// </summary>
        /// <param name="message">A message describing the validation failure.</param>
        /// <param name="results">Specific reasons for validation failure.</param>
        /// <param name="value">The value which caused validation to fail.</param>
        public ObjectValidationException(string message, IEnumerable<ValidationResult> results, object value)
            : base(message, null, value)
        {
            if (message == null) throw new ArgumentNullException("message");
            if (results == null) throw new ArgumentNullException("results");

            Results = results;
        }
    }
}
