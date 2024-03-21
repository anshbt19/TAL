using CalendarBookingApp.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CalendarBookingApp.Repositories
{
    public interface IAppointmentRepository
    {
        Task<bool> IsTimeSlotAvailable(DateTime date, TimeSpan startTime, TimeSpan endTime);
        Task Add(Appointment appointment);
        Task Delete(Appointment appointment);
        Task<Appointment?> FindByDateTime(DateTime date, TimeSpan startTime);
        Task<TimeSpan?> FindNextAvailableSlot(DateTime date, TimeSpan startTime);
    }
}
