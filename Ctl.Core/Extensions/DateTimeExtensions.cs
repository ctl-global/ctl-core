using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ctl.Extensions
{
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Determines the difference, in business days, between two dates.
        /// </summary>
        /// <param name="fromDate">The date to get business days from.</param>
        /// <param name="toDate">The date to get business days to.</param>
        /// <returns>A TimeSpan representing the amount of business days between the two dates.</returns>
        public static TimeSpan GetBusinessDays(this DateTime fromDate, DateTime toDate)
        {
            if (toDate < fromDate)
            {
                return -GetBusinessDays(toDate, fromDate);
            }

            fromDate = ClampBackward(fromDate);
            toDate = ClampForward(toDate);

            long ticks = toDate.Ticks - fromDate.Ticks;

            long weeks;
            weeks = Math.DivRem(ticks, TimeSpan.TicksPerDay * 7, out ticks);

            ticks += TimeSpan.TicksPerDay * 5 * weeks;

            if (toDate.DayOfWeek < fromDate.DayOfWeek)
            {
                ticks -= TimeSpan.TicksPerDay * 2;
            }

            return new TimeSpan(ticks);
        }

        /// <summary>
        /// Adds business days to a date.
        /// </summary>
        /// <param name="date">The date to add to.</param>
        /// <param name="businessDays">The amount of business days to add.</param>
        /// <returns>A new date with the business days added.</returns>
        public static DateTime AddBusinessDays(this DateTime date, TimeSpan businessDays)
        {
            if (businessDays == TimeSpan.Zero)
            {
                return date;
            }

            long weeks, ticks;
            weeks = Math.DivRem(businessDays.Ticks, TimeSpan.TicksPerDay * 5, out ticks);

            if (weeks != 0)
            {
                date = date.AddTicks(TimeSpan.TicksPerDay * 7 * weeks);
            }

            if (businessDays > TimeSpan.Zero)
            {
                if (ticks == 0)
                {
                    date = ClampForward(date);
                }
                else
                {
                    do
                    {
                        long ticksToAdd = Math.Min(ticks, TimeSpan.TicksPerDay);
                        date = ClampForward(date.AddTicks(ticksToAdd));
                        ticks -= ticksToAdd;
                    }
                    while (ticks != 0);
                }
            }
            else
            {
                // negative days.
                if (ticks == 0)
                {
                    date = ClampBackward(date);
                }
                else
                {
                    do
                    {
                        long ticksToAdd = Math.Max(ticks, -TimeSpan.TicksPerDay);
                        date = ClampBackward(date.AddTicks(ticksToAdd));
                        ticks -= ticksToAdd;
                    }
                    while (ticks != 0);
                }
            }

            return date;
        }

        /// <summary>
        /// Clamps Saturday/Sunday to Friday.
        /// </summary>
        static DateTime ClampBackward(DateTime date)
        {
            DayOfWeek dw = date.DayOfWeek;

            return
                dw == DayOfWeek.Saturday ? date.AddTicks(-TimeSpan.TicksPerDay) :
                dw == DayOfWeek.Sunday ? date.AddTicks(-TimeSpan.TicksPerDay * 2) :
                date;
        }

        /// <summary>
        /// Clamps Saturday/Sunday to Monday.
        /// </summary>
        static DateTime ClampForward(DateTime date)
        {
            DayOfWeek dw = date.DayOfWeek;

            return
                dw == DayOfWeek.Saturday ? date.AddTicks(TimeSpan.TicksPerDay * 2) :
                dw == DayOfWeek.Sunday ? date.AddTicks(TimeSpan.TicksPerDay) :
                date;
        }
    }
}
