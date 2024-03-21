namespace CalendarBookingApp.Validators
{
    public class TimeSlotValidationResult
    {
        public bool IsValid { get; }
        public string Message { get; }

        private TimeSlotValidationResult(bool isValid, string message)
        {
            IsValid = isValid;
            Message = message;
        }

        public static TimeSlotValidationResult Invalid(string message)
        {
            return new TimeSlotValidationResult(false, message);
        }

        public static TimeSlotValidationResult Valid()
        {
            return new TimeSlotValidationResult(true, string.Empty);
        }
    }
}

