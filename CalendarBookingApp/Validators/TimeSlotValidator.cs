
namespace CalendarBookingApp.Validators
{
    public class TimeSlotValidator : ITimeSlotValidator
    {
       public bool IsSecondDayOfThirdWeek(DateTime date)
        {
            // Get the first day of the month
            var firstDayOfMonth = new DateTime(date.Year, date.Month, 1);

            // Find out how many days to add to the first day of the month to get to the first Monday
            int daysUntilFirstMonday = (int)DayOfWeek.Monday - (int)firstDayOfMonth.DayOfWeek;
            if (daysUntilFirstMonday < 0)
            {
                daysUntilFirstMonday += 7; // Ensure it's always positive, counting forward
            }
            var firstMonday = firstDayOfMonth.AddDays(daysUntilFirstMonday);

            // Calculate the second day of the third week
            var secondDayOfThirdWeek = firstMonday.AddDays(15); // Start of third week + 1 day

            // Check if the provided date is the second day of the third week
            return date.Date == secondDayOfThirdWeek.Date;
        }

        public TimeSlotValidationResult IsValidTimeSlot(DateTime date, TimeSpan startTime, TimeSpan endTime)
        {
            // Check if the date is in the past
            if (date.Date < DateTime.Today)
            {
                return TimeSlotValidationResult.Invalid("The date is in the past.");
            }

            // Check if the time is in the past for today's date
            if (date.Date == DateTime.Today && DateTime.Now.TimeOfDay > startTime)
            {
                return TimeSlotValidationResult.Invalid("The time is in the past.");
            }

            // Check if time is within business hours
            if (startTime < new TimeSpan(9, 0, 0) || endTime > new TimeSpan(17, 0, 0))
            {
                return TimeSlotValidationResult.Invalid("The time slot is outside of business hours.");
            }

            // Additional check for the reserved time slot on the second day of the third week of any month
            if (IsSecondDayOfThirdWeek(date) && startTime >= new TimeSpan(16, 0, 0) && startTime < new TimeSpan(17, 0, 0))
            {
                return TimeSlotValidationResult.Invalid("This time slot is reserved and unavailable.");
            }

            return TimeSlotValidationResult.Valid();
        }
    }
}

