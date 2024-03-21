using System;

namespace CalendarBookingApp.Exceptions
{
    public class AppointmentException : Exception
    {
        public AppointmentException()
        {
        }

        public AppointmentException(string message)
            : base(message)
        {
        }

        public AppointmentException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
