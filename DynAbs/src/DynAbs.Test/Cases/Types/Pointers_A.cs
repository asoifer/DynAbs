using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs.Test.Cases.Types
{
    class Pointers_A
    {
        static void Main()
        {
            var dt = DateTime.Now;
            var b = NonWindowsSetDate(dt);
            Console.WriteLine(b);
            return;
        }

        internal static unsafe bool NonWindowsSetDate(DateTime dateToUse)
        {
            UnixTm tm = DateTimeToUnixTm(dateToUse);
            return SetDate(&tm) == 0;
        }

        internal static unsafe int SetDate(UnixTm* tm)
        {
            return tm->tm_sec;
        }

        internal static UnixTm DateTimeToUnixTm(DateTime date)
        {
            UnixTm tm;
            tm.tm_sec = date.Second;
            tm.tm_min = date.Minute;
            tm.tm_hour = date.Hour;
            tm.tm_mday = date.Day;
            tm.tm_mon = date.Month - 1; // needs to be 0 indexed
            tm.tm_year = date.Year - 1900; // years since 1900
            tm.tm_wday = 0; // this is ignored by mktime
            tm.tm_yday = 0; // this is also ignored
            tm.tm_isdst = date.IsDaylightSavingTime() ? 1 : 0;
            return tm;
        }

        internal unsafe struct UnixTm
        {
            public int tm_sec;    /* Seconds (0-60) */
            public int tm_min;    /* Minutes (0-59) */
            public int tm_hour;   /* Hours (0-23) */
            public int tm_mday;   /* Day of the month (1-31) */
            public int tm_mon;    /* Month (0-11) */
            public int tm_year;   /* Year - 1900 */
            public int tm_wday;   /* Day of the week (0-6, Sunday = 0) */
            public int tm_yday;   /* Day in the year (0-365, 1 Jan = 0) */
            public int tm_isdst;  /* Daylight saving time */
        }
    }
}
