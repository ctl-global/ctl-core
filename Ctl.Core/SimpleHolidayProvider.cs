using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ctl.Extensions;

namespace Ctl
{
    /// <summary>
    /// A simple holiday provider that runs of a predefined collection of dates.
    /// </summary>
    public sealed class SimpleHolidayProvider : IHolidayProvider
    {
        readonly List<DateTime> dates;

        /// <summary>
        /// Creates a new SimpleHolidayProvider using a collection of dates.
        /// </summary>
        /// <param name="dates">Dates to create the provider with.</param>
        public SimpleHolidayProvider(IEnumerable<DateTime> dates)
        {
            if (dates == null) throw new ArgumentNullException(nameof(dates));

            this.dates = dates.Select(x => x.Date).Distinct().OrderBy(x => x).ToList();
        }

        /// <summary>
        /// Counts the number of holidays within a time period.
        /// </summary>
        /// <param name="fromDate">The date to calculate from, inclusive.</param>
        /// <param name="toDate">The date to calculate to, inclusive.</param>
        /// <returns>The number of holidays between <paramref name="fromDate"/> and <paramref name="toDate"/>.</returns>
        public int CountHolidays(DateTime fromDate, DateTime toDate)
        {
            if (fromDate > toDate)
            {
                return CountHolidays(toDate, fromDate);
            }

            fromDate = fromDate.Date;
            toDate = toDate.Date.AddTicks(TimeSpan.TicksPerDay - 1);

            int fromIdx = dates.LowerBound(fromDate);
            int toIdx = dates.UpperBound(toDate);

            return toIdx - fromIdx;
        }

        /// <summary>
        /// Gets a collection of holidays within a time period.
        /// </summary>
        /// <param name="fromDate">The date to calculate from, inclusive.</param>
        /// <param name="toDate">The date to calculate to, inclusive.</param>
        /// <returns>A collection of holidays between <paramref name="fromDate"/> and <paramref name="toDate"/>.</returns>
        public IEnumerable<DateTime> EnumerateHolidays(DateTime fromDate, DateTime toDate)
        {
            if (fromDate > toDate)
            {
                DateTime tmp = fromDate;

                fromDate = toDate;
                toDate = tmp;
            }

            fromDate = fromDate.Date;
            toDate = toDate.Date.AddTicks(TimeSpan.TicksPerDay - 1);

            int fromIdx = dates.LowerBound(fromDate);
            int toIdx = dates.UpperBound(toDate);

            while (fromIdx != toIdx)
            {
                yield return dates[fromIdx++];
            }
        }

        /// <summary>
        /// Tests if a date is a holiday.
        /// </summary>
        /// <param name="date">The date to test.</param>
        /// <returns>If the date is a holiday, true. Otherwise, false.</returns>
        public bool IsHoliday(DateTime date)
        {
            return dates.BinarySearch(date) >= 0;
        }
    }
}
