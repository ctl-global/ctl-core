using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ctl.Extensions;

namespace Ctl
{
    /// <summary>
    /// Compares strings as if they had normalized whitespace.
    /// </summary>
    public sealed class NormalizingComparer : IComparer<string>, IEqualityComparer<string>
    {
        readonly StringComparer baseComparer;

        /// <summary>
        /// Initializes a normalizing comparer using a base comparer.
        /// </summary>
        /// <param name="baseComparer">The base comparer to use.</param>
        public NormalizingComparer(StringComparer baseComparer)
        {
            this.baseComparer = baseComparer ?? StringComparer.Ordinal;
        }

        public bool Equals(string x, string y)
        {
            return baseComparer.Equals(x.NormalizeWhitespace(), y.NormalizeWhitespace());
        }

        public int GetHashCode(string obj)
        {
            return baseComparer.GetHashCode(obj.NormalizeWhitespace());
        }

        public int Compare(string x, string y)
        {
            return baseComparer.Compare(x.NormalizeWhitespace(), y.NormalizeWhitespace());
        }
    }
}
