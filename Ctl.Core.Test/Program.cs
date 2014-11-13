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
                "",
                "  ",
                "foo",
                "foo ",
                "foo  ",
                " foo",
                "  foo",
                " foo ",
                "  foo  ",
                "foo bar",
                "foo  bar",
                " foo  bar baz ",
                " foo bar  baz "
            };

            foreach (var s in strings)
            {
                Console.WriteLine("\"{0}\", \"{1}\"", s, Normalize(s));
            }
        }

        public static string Normalize(string s)
        {
            return string.Join(" ", s.Tokenize((str, i) => char.IsWhiteSpace(str, i)));
        }
    }
}
