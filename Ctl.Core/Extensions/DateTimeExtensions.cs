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
        /// Adds business days to a date.
        /// </summary>
        /// <param name="date">The date to add to.</param>
        /// <param name="businessDays">The amount of business days to add.</param>
        /// <returns>A new date with the business days added.</returns>
        public static DateTime AddBusinessDays(this DateTime date, int businessDays)
        {
            if (businessDays == 0)
            {
                return date;
            }

            int weeks, days;
            weeks = Math.DivRem(businessDays, 5, out days);

            if (weeks != 0)
            {
                date = date.AddTicks(TimeSpan.TicksPerDay * 7 * weeks);
            }

            if (businessDays > 0)
            {
                if (days == 0)
                {
                    date = ClampForward(date);
                }
                else
                {
                    do date = ClampForward(date.AddTicks(TimeSpan.TicksPerDay));
                    while (--days != 0);
                }
            }
            else
            {
                // negative days.

                if (days == 0)
                {
                    date = ClampBackward(date);
                }
                else
                {
                    do date = ClampBackward(date.AddTicks(-TimeSpan.TicksPerDay));
                    while (++days != 0);
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
