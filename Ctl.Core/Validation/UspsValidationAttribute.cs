using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Ctl.Validation
{
    /// <summary>
    /// Validates a field or property depending on another country code member.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false)]
    public class UspsValidationAttribute : ValidationAttribute
    {
        /// <summary>
        /// The name of a country code member read to see what country's states to validate.
        /// </summary>
        public string CountryCodeMember { get; set; }

        protected UspsValidationAttribute(string errorMessage)
            : base(errorMessage)
        {
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (IsValid(value))
            {
                return ValidationResult.Success;
            }

            if (CountryCodeMember != null && validationContext.ObjectInstance != null)
            {
                Type type = validationContext.ObjectInstance.GetType();

                string countryCode = (type.GetProperty(CountryCodeMember)?.GetValue(validationContext.ObjectInstance)
                    ?? type.GetField(CountryCodeMember)?.GetValue(validationContext.ObjectInstance)) as string;

                if (!string.IsNullOrEmpty(countryCode) && !string.Equals(countryCode, "US", StringComparison.OrdinalIgnoreCase))
                {
                    return ValidationResult.Success;
                }
            }

            return new ValidationResult(FormatErrorMessage(validationContext.DisplayName), validationContext.MemberName != null ? new[] { validationContext.MemberName } : null);
        }
    }
}
