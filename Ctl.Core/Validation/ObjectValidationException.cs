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
