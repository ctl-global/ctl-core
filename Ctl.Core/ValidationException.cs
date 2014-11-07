using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ctl.Core
{
    public class ValidationException : Exception
    {
        public object Object { get; set; }
        public IEnumerable<ValidationResult> Results { get; set; }

        public ValidationException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }
}
