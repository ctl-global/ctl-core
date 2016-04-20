using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ctl
{
    /// <summary>
    /// Reports progress as a percentage.
    /// </summary>
    public sealed class PercentProgress
    {
        readonly IProgress<double> progress;
        readonly double rmax;
        readonly int max;
        int cur;

        /// <summary>
        /// Creates a new progress reporter.
        /// </summary>
        /// <param name="max">The total number of steps to report over.</param>
        /// <param name="progress">The progress to report to.</param>
        public PercentProgress(int max, IProgress<double> progress)
        {
            if (max <= 0) throw new ArgumentOutOfRangeException(nameof(max), "Maximum value must be greater than zero.");

            this.progress = progress;
            this.max = max;
            this.rmax = 1.0 / max;
        }

        /// <summary>
        /// Increments the count, reporting progress.
        /// </summary>
        /// <param name="count">The number of steps to increase progress by.</param>
        public void Step(int count = 1)
        {
            int newCur = Math.Min(checked(cur + count), max);

            if (newCur != cur)
            {
                cur = newCur;
                progress?.Report(Math.Min(Math.Max(0.0, newCur * rmax), 1.0));
            }
        }
    }
}
