using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ctl.Extensions;
using Ctl.Validation;
using System.Security.Cryptography;

namespace Ctl.Core.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Random rng = new Random();
            List<string> strings = new List<string>();

            for (int i = 0; i < 10000; ++i)
            {
                strings.Add(rng.NextString(16));
            }

            Console.WriteLine("ComparableString:");
            Benchmark.Run(strings.Count, () =>
            {
                IEnumerable<string> e = strings.OrderBy(x => x.WithComparer(StringComparer.OrdinalIgnoreCase));
                foreach (var s in e)
                {
                    // do nothing.
                }
            });

            Console.WriteLine("OrderBy:");
            Benchmark.Run(strings.Count, () =>
            {
                IEnumerable<string> e = strings.OrderBy(x => x, StringComparer.OrdinalIgnoreCase);
                foreach (var s in e)
                {
                    // do nothing.
                }
            });
        }
    }
}
