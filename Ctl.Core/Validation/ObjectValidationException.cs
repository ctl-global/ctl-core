/*
    Copyright (c) 2015, CTL Global, Inc.
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
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Ctl.Validation
{
    /// <summary>
    /// Thrown when an object fails validation.
    /// </summary>
    [Serializable]
    public class ObjectValidationException : Exception
    {
        public object Object { get; private set; }
        public IEnumerable<ValidationResult> ValidationResults { get; private set; }

        public override string Message
        {
            get
            {
                if (ValidationResults == null)
                {
                    return base.Message;
                }

                return base.Message + Environment.NewLine + string.Join(Environment.NewLine, from r in ValidationResults
                                                                                             select string.Join(", ", r.MemberNames) + ": " + r.ErrorMessage);
            }
        }

        public ObjectValidationException()
            : this("An object failed to validate.")
        {
        }

        public ObjectValidationException(string message)
            : base(message)
        {
        }

        public ObjectValidationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Instatiates a new ObjectValidation Exception.
        /// </summary>
        /// <param name="obj">The object that failed validation.</param>
        /// <param name="validationResults">Validation results describing the failure.</param>
        public ObjectValidationException(object obj, IEnumerable<ValidationResult> validationResults)
            : this("An object failed to validate. See ValidationResults for details.")
        {
            this.Object = obj;
            this.ValidationResults = validationResults;
        }

        /// <summary>
        /// Instantiates a new ObjectValidationException from a serialized instance.
        /// </summary>
        /// <param name="info">Serialization info to deserialize from.</param>
        /// <param name="context">A context to use while deserializing.</param>
        protected ObjectValidationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            var value = info.GetValue("Errors", typeof(List<SerializedValidationResult>));
            ValidationResults = ((List<SerializedValidationResult>)value).Select(x => new ValidationResult(x.ErrorMessage, x.MemberNames)).ToArray();
        }

        /// <summary>
        /// Serializes the ObjectValidationException for remoting.
        /// </summary>
        /// <param name="info">Serialization info to store data to.</param>
        /// <param name="context">A context to use while serializing.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null) throw new ArgumentNullException("info");

            base.GetObjectData(info, context);
            info.AddValue("Errors", ValidationResults.Select(x => new SerializedValidationResult { ErrorMessage = x.ErrorMessage, MemberNames = x.MemberNames.ToList() }).ToList(), typeof(List<SerializedValidationResult>));
        }

        [Serializable]
        sealed class SerializedValidationResult
        {
            public string ErrorMessage { get; set; }
            public List<string> MemberNames { get; set; }
        }
    }
}
