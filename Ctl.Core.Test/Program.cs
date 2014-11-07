using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ctl.Extensions;
using Ctl.Validation;

namespace Ctl.Core.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var strings = new[]
            {
                "12345",
                "12345a",
                "a12345",
                "12345-6789",
                "a12345-6789",
                "12345-6789a"
            };

            foreach (var s in strings)
            {
                Console.WriteLine("{0}: {1}", s, new ZipCodeAttribute().IsValid(s));
            }
        }
    }
}
