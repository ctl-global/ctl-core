using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ctl
{
    /// <summary>
    /// Provides functionality for working with holidays.
    /// </summary>
    public interface IHolidayProvider
    {
        /// <summary>
        /// Tests if a date is a holiday.
        /// </summary>
        /// <param name="date">The date to test.</param>
        /// <returns>If the date is a holiday, true. Otherwise, false.</returns>
        bool IsHoliday(DateTime date);

        /// <summary>
        /// Gets a collection of holidays within a time period.
        /// </summary>
        /// <param name="fromDate">The date to calculate from, inclusive.</param>
        /// <param name="toDate">The date to calculate to, inclusive.</param>
        /// <returns>A collection of holidays between <paramref name="fromDate"/> and <paramref name="toDate"/>.</returns>
        IEnumerable<DateTime> EnumerateHolidays(DateTime fromDate, DateTime toDate);

        /// <summary>
        /// Counts the number of holidays within a time period.
        /// </summary>
        /// <param name="fromDate">The date to calculate from, inclusive.</param>
        /// <param name="toDate">The date to calculate to, inclusive.</param>
        /// <returns>The number of holidays between <paramref name="fromDate"/> and <paramref name="toDate"/>.</returns>
        int CountHolidays(DateTime fromDate, DateTime toDate);
    }
}
