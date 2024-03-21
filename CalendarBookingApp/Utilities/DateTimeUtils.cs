using System.Globalization;

namespace CalendarBookingApp.Utilities
{
    public static class DateTimeUtils
    {
        public static bool TryParseDateAndTime(string dateInput, string timeInput, out DateTime date, out TimeSpan time)
        {
            date = DateTime.MinValue;
            time = TimeSpan.Zero;

            if (!DateTime.TryParseExact(dateInput, "dd/MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
            {
                return false;
            }

            if (!TimeSpan.TryParseExact(timeInput, "hh\\:mm", CultureInfo.InvariantCulture, TimeSpanStyles.None, out time))
            {
                return false;
            }

            return true;
        }

        public static bool TryParseDate(string dateInput, out DateTime date)
        {
            date = DateTime.MinValue;

            if (!DateTime.TryParseExact(dateInput, "dd/MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
            {
                return false;
            }

            return true;
        }

        public static bool TryParseTime(string timeInput, out TimeSpan time)
        {
            time = TimeSpan.Zero;

            if (!TimeSpan.TryParseExact(timeInput, "hh\\:mm", CultureInfo.InvariantCulture, out time))
            {
                return false;
            }

            return true;
        }
    }
}

