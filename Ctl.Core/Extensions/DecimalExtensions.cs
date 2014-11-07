using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ctl.Extensions
{
    public static class DecimalExtensions
    {
        /// <summary>
        /// Normalizes a decimal, removing any excess zeros after the decimal point, i.e. from "1.5000" to "1.5".
        /// </summary>
        /// <param name="d">The decimal to normalize.</param>
        /// <returns>A normalized decimal.</returns>
        public static decimal Normalize(this decimal d)
        {
            // A trick courtesy of http://stackoverflow.com/a/7983330/209199
            return d / 1.000000000000000000000000000000000m;
        }
    }
}
