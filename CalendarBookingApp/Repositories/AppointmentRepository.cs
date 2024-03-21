using CalendarBookingApp.Data;
using CalendarBookingApp.Entities;
using Microsoft.EntityFrameworkCore;

namespace CalendarBookingApp.Repositories
{
    public class AppointmentRepository : IAppointmentRepository
    {
        private readonly AppointmentContext _context;

        public AppointmentRepository(AppointmentContext context)
        {
            _context = context;
        }

        public async Task Add(Appointment appointment)
        {
            _context.Appointments!.Add(appointment);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(Appointment appointment)
        {
            _context.Appointments!.Remove(appointment);
            await _context.SaveChangesAsync();
        }

        public async Task<Appointment?> FindByDateTime(DateTime date, TimeSpan startTime)
        {
            return await _context.Appointments!
                .FirstOrDefaultAsync(a => a.AppointmentDate.Date == date.Date && a.StartTime == startTime);
        }

        public async Task<bool> IsTimeSlotAvailable(DateTime date, TimeSpan startTime, TimeSpan endTime)
        {
            return !await _context.Appointments!
                .AnyAsync(a => a.AppointmentDate.Date == date.Date &&
                            ((a.StartTime <= startTime && a.EndTime > startTime) ||
                                (a.StartTime < endTime && a.EndTime >= endTime)));
        }

        public async Task<TimeSpan?> FindNextAvailableSlot(DateTime date, TimeSpan startTime)
        {
            // Adjust the start time based on the current time for today's date
            if (date.Date == DateTime.Today && DateTime.Now.TimeOfDay > startTime)
            {
                startTime = DateTime.Now.TimeOfDay;
            }

            // Round up to the next 30-minute slot if we're starting from the current time
            if (date.Date == DateTime.Today)
            {
                int minutes = startTime.Minutes >= 30 ? 60 - startTime.Minutes : 30 - startTime.Minutes;
                startTime = startTime.Add(TimeSpan.FromMinutes(minutes));
            }

            var appointments = await _context.Appointments!
                .Where(a => a.AppointmentDate.Date == date.Date)
                .OrderBy(a => a.StartTime)
                .ToListAsync();

            TimeSpan start = startTime < new TimeSpan(9, 0, 0) ? new TimeSpan(9, 0, 0) : startTime;

            foreach (var appointment in appointments)
            {
                if (start < appointment.StartTime)
                {
                    // Found a free slot before the current appointment.
                    return start;
                }
                // Move to the end of the current appointment if it ends later than the current start time.
                if (appointment.EndTime > start)
                {
                    start = appointment.EndTime;
                }
            }

            // Check if there's a slot available at the end of the day.
            if (start < new TimeSpan(17, 0, 0))
            {
                return start;
            }

            // No slot available.
            return null;
        }
    }
}
