using CalendarBookingApp.Repositories;
using CalendarBookingApp.Entities;
using CalendarBookingApp.Validators;

namespace CalendarBookingApp.Services
{
    public class AppointmentService
    {
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly ITimeSlotValidator _timeSlotValidator;

        public AppointmentService(IAppointmentRepository appointmentRepository, ITimeSlotValidator timeSlotValidator)
        {
            _appointmentRepository = appointmentRepository;
            _timeSlotValidator = timeSlotValidator;
        }

        public async Task<ServiceResult> AddAppointment(DateTime date, TimeSpan startTime)
        {
            var endTime = startTime.Add(TimeSpan.FromMinutes(30));
            var validationResult = _timeSlotValidator.IsValidTimeSlot(date, startTime, endTime);

            if (!validationResult.IsValid)
            {
                return new ServiceResult(false, validationResult.Message ?? "Invalid time slot.");
            }

            if (!await _appointmentRepository.IsTimeSlotAvailable(date, startTime, endTime))
            {
                return new ServiceResult(false, "Time slot is not available.");
            }

            var appointment = new Appointment
            {
                AppointmentDate = date,
                StartTime = startTime,
                EndTime = endTime
            };
            try
            {
                await _appointmentRepository.Add(appointment);
                return new ServiceResult(true, $"Appointment added on {date.ToShortDateString()} from {startTime} to {endTime}.");
            }
            catch (Exception)
            {
                return new ServiceResult(false, "Could not create appointment.");
            }
        }

        public async Task<ServiceResult> DeleteAppointment(DateTime date, TimeSpan startTime)
        {
            DateTime appointmentDateTime = date.Add(startTime);
            
            if (appointmentDateTime < DateTime.Now)
            {
                return new ServiceResult(false, "Cannot delete past appointments.");
            }
            
            var appointment = await _appointmentRepository.FindByDateTime(date, startTime);
            if (appointment == null)
            {
                return new ServiceResult(false, $"Appointment not found on {date.ToShortDateString()} at {startTime}.");
            }

            try
            {
                await _appointmentRepository.Delete(appointment);
                return new ServiceResult(true, $"Appointment deleted on {date.ToShortDateString()} at {startTime}.");
            }
            catch (Exception)
            {
                // Consider logging the exception here
                return new ServiceResult(false, "Could not delete the appointment.");
            }
        }

        public async Task<ServiceResult> FindAppointment(DateTime date)
        {
            if (date.Date < DateTime.Today)
            {
                return new ServiceResult(false, "Cannot find appointments for past dates.");
            }

            try
            {
                TimeSpan startTime = date.Date == DateTime.Today ? DateTime.Now.TimeOfDay : new TimeSpan(9, 0, 0);
                if (date.Date == DateTime.Today && startTime.Minutes % 30 != 0)
                {
                    int minutesToAdd = 30 - startTime.Minutes % 30;
                    startTime = new TimeSpan(startTime.Hours, startTime.Minutes + minutesToAdd, 0);
                }

                var freeSlot = await _appointmentRepository.FindNextAvailableSlot(date, startTime);
                if (freeSlot != null)
                {
                    return new ServiceResult(true, $"Next available slot is on {date.ToShortDateString()} at {freeSlot.Value}.");
                }
                else
                {
                    return new ServiceResult(false, "No available slots for the day.");
                }
            }
            catch (Exception)
            {
                // Handle or log the exception as needed
                return new ServiceResult(false, "An error occurred while trying to find an appointment.");
            }
        }

        public async Task<ServiceResult> KeepTimeslot(TimeSpan time)
        {
            DateTime dateToCheck = DateTime.Today;
            TimeSpan endTime = time.Add(TimeSpan.FromMinutes(30));

            if (DateTime.Now.TimeOfDay > time && dateToCheck == DateTime.Today)
            {
                dateToCheck = DateTime.Today.AddDays(1);
            }

            while (true)
            {
                var validationResult = _timeSlotValidator.IsValidTimeSlot(dateToCheck, time, endTime);

                if (validationResult.IsValid && await _appointmentRepository.IsTimeSlotAvailable(dateToCheck, time, endTime))
                {
                    var appointment = new Appointment
                    {
                        AppointmentDate = dateToCheck,
                        StartTime = time,
                        EndTime = endTime
                    };

                    try
                    {
                        await _appointmentRepository.Add(appointment);
                        return new ServiceResult(true, $"Reserved timeslot at {time} on {dateToCheck.ToShortDateString()}.");
                    }
                    catch (Exception)
                    {
                        // Handle or log the exception as needed
                        return new ServiceResult(false, "An error occurred while trying to keep the timeslot.");
                    }
                }
                else if (!validationResult.IsValid)
                {
                    return new ServiceResult(false, validationResult.Message ?? "Invalid time slot.");
                }

                dateToCheck = dateToCheck.AddDays(1);

                if (dateToCheck > DateTime.Today.AddYears(1))
                {
                    return new ServiceResult(false, "Could not find an available slot within a year.");
                }
            }
        }
    }
}
