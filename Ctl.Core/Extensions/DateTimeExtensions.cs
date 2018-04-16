using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ctl.Extensions
{
    /// <summary>
    /// Provides extension methods for the DateTime type.
    /// </summary>
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Adds days to a date.
        /// </summary>
        /// <param name="fromDate">The date to add days to.</param>
        /// <param name="days">The number of days to add.</param>
        /// <returns>A new date with the days added.</returns>
        public static DateTime AddDays(this DateTime fromDate, int days)
        {
            return fromDate.AddTicks(TimeSpan.TicksPerDay * days);
        }

        /// <summary>
        /// Determines the difference, in business days, between two dates.
        /// </summary>
        /// <param name="fromDate">The date to get business days from.</param>
        /// <param name="toDate">The date to get business days to.</param>
        /// <returns>A TimeSpan representing the amount of business days between the two dates.</returns>
        public static TimeSpan GetBusinessDays(this DateTime fromDate, DateTime toDate)
            => GetBusinessDays(fromDate, toDate, null);

        /// <summary>
        /// Determines the difference, in business days, between two dates.
        /// </summary>
        /// <param name="fromDate">The date to get business days from.</param>
        /// <param name="toDate">The date to get business days to.</param>
        /// <param name="holidayProvider">A holiday provider used to exclude holidays from business days. If null, only Saturday and Sunday are assumed off.</param>
        /// <returns>A TimeSpan representing the amount of business days between the two dates.</returns>
        public static TimeSpan GetBusinessDays(this DateTime fromDate, DateTime toDate, IHolidayProvider holidayProvider)
        {
            if (toDate < fromDate)
            {
                return -GetBusinessDays(toDate, fromDate);
            }

            fromDate = ClampBackward(fromDate, holidayProvider);
            toDate = ClampForward(toDate, holidayProvider);

            long ticks = toDate.Ticks - fromDate.Ticks;
            long weeks;

            weeks = ticks / (TimeSpan.TicksPerDay * 7);
            ticks = ticks % (TimeSpan.TicksPerDay * 7) + (TimeSpan.TicksPerDay * 5 * weeks);

            if (toDate.DayOfWeek < fromDate.DayOfWeek)
            {
                ticks -= TimeSpan.TicksPerDay * 2;
            }

            if (holidayProvider != null)
            {
                ticks -= holidayProvider.CountHolidays(fromDate, toDate) * TimeSpan.TicksPerDay;
            }

            return new TimeSpan(ticks);
        }

        /// <summary>
        /// Adds business days to a date.
        /// </summary>
        /// <param name="date">The date to add to.</param>
        /// <param name="businessDays">The amount of business days to add.</param>
        /// <returns>A new date with the business days added.</returns>
        public static DateTime AddBusinessDays(this DateTime date, int businessDays) =>
            AddBusinessDays(date, businessDays, null);

        /// <summary>
        /// Adds business days to a date.
        /// </summary>
        /// <param name="date">The date to add to.</param>
        /// <param name="businessDays">The amount of business days to add.</param>
        /// <param name="holidayProvider">A holiday provider used to exclude holidays from business days. If null, only Saturday and Sunday are assumed off.</param>
        /// <returns>A new date with the business days added.</returns>
        public static DateTime AddBusinessDays(this DateTime date, int businessDays, IHolidayProvider holidayProvider)
        {
            return AddBusinessDays(date, new TimeSpan(TimeSpan.TicksPerDay * businessDays), holidayProvider);
        }

        /// <summary>
        /// Adds business days to a date.
        /// </summary>
        /// <param name="date">The date to add to.</param>
        /// <param name="businessDays">The amount of business days to add.</param>
        /// <returns>A new date with the business days added.</returns>
        public static DateTime AddBusinessDays(this DateTime date, TimeSpan businessDays) =>
            AddBusinessDays(date, businessDays, null);

        /// <summary>
        /// Adds business days to a date.
        /// </summary>
        /// <param name="date">The date to add to.</param>
        /// <param name="businessDays">The amount of business days to add.</param>
        /// <param name="holidayProvider">A holiday provider used to exclude holidays from business days. If null, only Saturday and Sunday are assumed off.</param>
        /// <returns>A new date with the business days added.</returns>
        public static DateTime AddBusinessDays(this DateTime date, TimeSpan businessDays, IHolidayProvider holidayProvider)
        {
            if (businessDays == TimeSpan.Zero)
            {
                return date;
            }

            long weeks = businessDays.Ticks / (TimeSpan.TicksPerDay * 5);
            long ticks = businessDays.Ticks % (TimeSpan.TicksPerDay * 5);

            if (weeks != 0)
            {
                DateTime newDate = date.AddTicks(TimeSpan.TicksPerDay * 7 * weeks);

                if (holidayProvider != null)
                {
                    int holidays = holidayProvider.CountHolidays(date < newDate ? date : newDate, date < newDate ? newDate : date);

                    if (businessDays < TimeSpan.Zero)
                        holidays = -holidays;

                    ticks += holidays * TimeSpan.TicksPerDay;
                }

                date = newDate;
            }

            if (businessDays > TimeSpan.Zero)
            {
                do
                {
                    long ticksToAdd = Math.Min(ticks, TimeSpan.TicksPerDay);
                    date = ClampForward(date.AddTicks(ticksToAdd), holidayProvider);
                    ticks -= ticksToAdd;
                }
                while (ticks != 0);
            }
            else
            {
                // negative days.
                do
                {
                    long ticksToAdd = Math.Max(ticks, -TimeSpan.TicksPerDay);
                    date = ClampBackward(date.AddTicks(ticksToAdd), holidayProvider);
                    ticks -= ticksToAdd;
                }
                while (ticks != 0);
            }

            return date;
        }

        /// <summary>
        /// Finds the nearest business day.
        /// </summary>
        /// <param name="date">The date to start with.</param>
        /// <param name="holidayProvider">A provider used to skip holidays.</param>
        /// <param name="forward">If true, the date will be moved forward to the next business day. If false, the date will be moved back.</param>
        /// <returns>If <paramref name="date"/> already falls on a business day, it is returned unchanged. Otherwise, the nearest business day.</returns>
        public static DateTime ToBusinessDay(this DateTime date, IHolidayProvider holidayProvider = null, bool forward = true)
        {
            return forward ? ClampForward(date, holidayProvider) : ClampBackward(date, holidayProvider);
        }

        /// <summary>
        /// Clamps Saturday/Sunday to Friday.
        /// </summary>
        static DateTime ClampBackward(DateTime date, IHolidayProvider holidayProvider)
        {
            while (true)
            {
                DayOfWeek dw = date.DayOfWeek;

                if (dw == DayOfWeek.Saturday) date = date.AddTicks(-TimeSpan.TicksPerDay);
                else if (dw == DayOfWeek.Sunday) date = date.AddTicks(-TimeSpan.TicksPerDay * 2);

                if (holidayProvider == null || !holidayProvider.IsHoliday(date))
                    return date;

                date = date.AddTicks(-TimeSpan.TicksPerDay);
            }
        }

        /// <summary>
        /// Clamps Saturday/Sunday to Monday.
        /// </summary>
        static DateTime ClampForward(DateTime date, IHolidayProvider holidayProvider)
        {
            while (true)
            {
                DayOfWeek dw = date.DayOfWeek;

                if (dw == DayOfWeek.Saturday) date = date.AddTicks(TimeSpan.TicksPerDay * 2);
                else if (dw == DayOfWeek.Sunday) date = date.AddTicks(TimeSpan.TicksPerDay);

                if (holidayProvider == null || !holidayProvider.IsHoliday(date))
                    return date;

                date = date.AddTicks(TimeSpan.TicksPerDay);
            }
        }

        /// <summary>
        /// Reinterprets a DateTime as local time.
        /// </summary>
        /// <remarks>
        /// Primarily intended to be used when reading a DateTime from a database, as their Kind is ambiguous.
        /// </remarks>
        public static DateTime AsLocal(this DateTime date)
        {
            return new DateTime(date.Ticks, DateTimeKind.Local);
        }

        /// <summary>
        /// Reinterprets a DateTime as universal time.
        /// </summary>
        /// <remarks>
        /// Primarily intended to be used when reading a DateTime from a database, as their Kind is ambiguous.
        /// </remarks>
        public static DateTime AsUniversal(this DateTime date)
        {
            return new DateTime(date.Ticks, DateTimeKind.Utc);
        }
    }
}
