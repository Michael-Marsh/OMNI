using System;

namespace OMNI.Extensions
{
    public static class DateTimeExtensions
    {
        public static int LastDayOfMonth(this DateTime date)
        {
            return date.AddDays(1 - (date.Day)).AddMonths(1).AddDays(-1).Day;
        }
    }
}
