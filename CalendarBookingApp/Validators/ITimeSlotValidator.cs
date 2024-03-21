namespace CalendarBookingApp.Validators
{
    public interface ITimeSlotValidator
    {
        TimeSlotValidationResult IsValidTimeSlot(DateTime date, TimeSpan startTime, TimeSpan endTime);
        bool IsSecondDayOfThirdWeek(DateTime date);
    }
}