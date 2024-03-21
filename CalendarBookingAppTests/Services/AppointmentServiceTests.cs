using System;
using System.Threading.Tasks;
using CalendarBookingApp.Services;
using CalendarBookingApp.Repositories;
using CalendarBookingApp.Entities;
using CalendarBookingApp.Validators;
using CalendarBookingApp.Exceptions;
using Moq;
using Xunit;

namespace CalendarBookingAppTests
{
    public class AppointmentServiceTests
    {
        private readonly Mock<IAppointmentRepository> _appointmentRepositoryMock;
        private readonly Mock<ITimeSlotValidator> _timeSlotValidatorMock;
        private readonly AppointmentService _service;

        public AppointmentServiceTests()
        {
            _appointmentRepositoryMock = new Mock<IAppointmentRepository>();
            _timeSlotValidatorMock = new Mock<ITimeSlotValidator>();
            _service = new AppointmentService(_appointmentRepositoryMock.Object, _timeSlotValidatorMock.Object);
        }

        [Fact]
        public async Task AddAppointment_ValidAndAvailable_ReturnsSuccess()
        {
            // Arrange
            var date = DateTime.Today;
            var startTime = new TimeSpan(10, 0, 0); // 10 AM
            var endTime = startTime.Add(TimeSpan.FromMinutes(30));
            _timeSlotValidatorMock.Setup(v => v.IsValidTimeSlot(date, startTime, endTime))
                                .Returns(TimeSlotValidationResult.Valid()); // Use static method
            _appointmentRepositoryMock.Setup(r => r.IsTimeSlotAvailable(date, startTime, endTime))
                                    .ReturnsAsync(true);

            // Act
            var result = await _service.AddAppointment(date, startTime);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal($"Appointment added on {date.ToShortDateString()} from {startTime} to {endTime}.", result.Message);
        }

        [Fact]
        public async Task AddAppointment_NotAvailable_ReturnsFailure()
        {
            // Arrange
            var date = DateTime.Today;
            var startTime = new TimeSpan(10, 0, 0); // 10 AM
            var endTime = startTime.Add(TimeSpan.FromMinutes(30));
            _timeSlotValidatorMock.Setup(v => v.IsValidTimeSlot(date, startTime, endTime))
                .Returns(TimeSlotValidationResult.Valid());
            _appointmentRepositoryMock.Setup(r => r.IsTimeSlotAvailable(date, startTime, endTime))
                .ReturnsAsync(false); // Time slot is not available

            // Act
            var result = await _service.AddAppointment(date, startTime);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Time slot is not available.", result.Message);
        }

        [Fact]
        public async Task AddAppointment_InvalidTimeSlot_ReturnsFailure()
        {
            // Arrange
            var date = DateTime.Today;
            var startTime = new TimeSpan(10, 0, 0); // 10 AM
            var endTime = startTime.Add(TimeSpan.FromMinutes(30));
            var invalidMessage = "Invalid time slot.";
            _timeSlotValidatorMock.Setup(v => v.IsValidTimeSlot(date, startTime, endTime))
                                .Returns(TimeSlotValidationResult.Invalid(invalidMessage)); // Use static method

            // Act
            var result = await _service.AddAppointment(date, startTime);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(invalidMessage, result.Message);
        }

        [Fact]
        public async Task AddAppointment_RepositoryThrowsException_ReturnsFailure()
        {
            // Arrange
            var date = DateTime.Today;
            var startTime = new TimeSpan(10, 0, 0); // 10 AM
            var endTime = startTime.Add(TimeSpan.FromMinutes(30));
            _timeSlotValidatorMock.Setup(v => v.IsValidTimeSlot(date, startTime, endTime))
                                .Returns(TimeSlotValidationResult.Valid());
            _appointmentRepositoryMock.Setup(r => r.IsTimeSlotAvailable(date, startTime, endTime))
                                    .ReturnsAsync(true); // Time slot is initially available
            _appointmentRepositoryMock.Setup(r => r.Add(It.IsAny<Appointment>()))
                                    .ThrowsAsync(new DataAccessException("Database unavailable")); // Simulate a data access exception

            // Act
            var result = await _service.AddAppointment(date, startTime);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Could not create appointment.", result.Message);
        }

        [Fact]
        public async Task DeleteAppointment_ExistsAndFuture_ReturnsSuccess()
        {
            // Arrange
            var date = DateTime.Today.AddDays(1);
            var startTime = new TimeSpan(10, 0, 0);
            var appointment = new Appointment { AppointmentDate = date, StartTime = startTime, EndTime = startTime.Add(TimeSpan.FromMinutes(30)) };

            _appointmentRepositoryMock.Setup(r => r.FindByDateTime(date, startTime))
                .ReturnsAsync(appointment);

            // Act
            var result = await _service.DeleteAppointment(date, startTime);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal($"Appointment deleted on {date.ToShortDateString()} at {startTime}.", result.Message);
        }

        [Fact]
        public async Task DeleteAppointment_NotFound_ReturnsFailure()
        {
            // Arrange
            var date = DateTime.Today.AddDays(1);
            var startTime = new TimeSpan(10, 0, 0);

            _appointmentRepositoryMock.Setup(r => r.FindByDateTime(date, startTime))
                .ReturnsAsync((Appointment)null); // Appointment not found

            // Act
            var result = await _service.DeleteAppointment(date, startTime);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal($"Appointment not found on {date.ToShortDateString()} at {startTime}.", result.Message);
        }

        [Fact]
        public async Task DeleteAppointment_InThePast_ReturnsFailure()
        {
            // Arrange
            var date = DateTime.Today.AddDays(-1); // Yesterday
            var startTime = new TimeSpan(10, 0, 0);

            // Act
            var result = await _service.DeleteAppointment(date, startTime);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Cannot delete past appointments.", result.Message);
        }

        [Fact]
        public async Task DeleteAppointment_RepositoryThrowsException_ReturnsFailure()
        {
            // Arrange
            var date = DateTime.Today.AddDays(1);
            var startTime = new TimeSpan(10, 0, 0);
            var appointment = new Appointment { AppointmentDate = date, StartTime = startTime, EndTime = startTime.Add(TimeSpan.FromMinutes(30)) };

            _appointmentRepositoryMock.Setup(r => r.FindByDateTime(date, startTime))
                .ReturnsAsync(appointment);
            _appointmentRepositoryMock.Setup(r => r.Delete(appointment))
                .ThrowsAsync(new Exception()); // Simulate an exception during deletion

            // Act
            var result = await _service.DeleteAppointment(date, startTime);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Could not delete the appointment.", result.Message);
        }

        [Fact]
        public async Task FindAppointment_InPast_ReturnsFailure()
        {
            // Arrange
            var date = DateTime.Today.AddDays(-1); // Yesterday

            // Act
            var result = await _service.FindAppointment(date);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Cannot find appointments for past dates.", result.Message);
        }

        [Fact]
        public async Task FindAppointment_SlotAvailable_ReturnsSuccess()
        {
            // Arrange
            var date = DateTime.Today;
            var startTime = new TimeSpan(9, 0, 0);
            var nextAvailableSlot = startTime.Add(TimeSpan.FromHours(1)); // Assume next slot is an hour later

            _appointmentRepositoryMock.Setup(r => r.FindNextAvailableSlot(date, It.IsAny<TimeSpan>()))
                .ReturnsAsync(nextAvailableSlot);

            // Act
            var result = await _service.FindAppointment(date);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal($"Next available slot is on {date.ToShortDateString()} at {nextAvailableSlot}.", result.Message);
        }

        [Fact]
        public async Task FindAppointment_NoSlotAvailable_ReturnsFailure()
        {
            // Arrange
            var date = DateTime.Today;
            TimeSpan? noSlot = null; // No available slot

            _appointmentRepositoryMock.Setup(r => r.FindNextAvailableSlot(date, It.IsAny<TimeSpan>()))
                .ReturnsAsync(noSlot);

            // Act
            var result = await _service.FindAppointment(date);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("No available slots for the day.", result.Message);
        }

        [Fact]
        public async Task FindAppointment_RepositoryThrowsException_ReturnsFailure()
        {
            // Arrange
            var date = DateTime.Today;

            _appointmentRepositoryMock.Setup(r => r.FindNextAvailableSlot(date, It.IsAny<TimeSpan>()))
                .ThrowsAsync(new Exception()); // Simulate an exception

            // Act
            var result = await _service.FindAppointment(date);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("An error occurred while trying to find an appointment.", result.Message);
        }

        [Fact]
        public async Task KeepTimeslot_ValidAndAvailable_ReturnsSuccess()
        {
            // Arrange
            var time = new TimeSpan(10, 0, 0); // 10 AM
            var endTime = time.Add(TimeSpan.FromMinutes(30));
            var dateToCheck = DateTime.Now.TimeOfDay > time ? DateTime.Today.AddDays(1) : DateTime.Today;

            _timeSlotValidatorMock.Setup(v => v.IsValidTimeSlot(dateToCheck, time, endTime))
                .Returns(TimeSlotValidationResult.Valid());
            _appointmentRepositoryMock.Setup(r => r.IsTimeSlotAvailable(dateToCheck, time, endTime))
                .ReturnsAsync(true);

            // Act
            var result = await _service.KeepTimeslot(time);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal($"Reserved timeslot at {time} on {dateToCheck.ToShortDateString()}.", result.Message);
        }

        [Fact]
        public async Task KeepTimeslot_InvalidTimeslot_ReturnsFailure()
        {
            // Arrange
            var time = new TimeSpan(10, 0, 0); // 10 AM
            var endTime = time.Add(TimeSpan.FromMinutes(30));
            var dateToCheck = DateTime.Now.TimeOfDay > time ? DateTime.Today.AddDays(1) : DateTime.Today;
            var invalidMessage = "Invalid time slot.";

            _timeSlotValidatorMock.Setup(v => v.IsValidTimeSlot(dateToCheck, time, endTime))
                .Returns(TimeSlotValidationResult.Invalid(invalidMessage));

            // Act
            var result = await _service.KeepTimeslot(time);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(invalidMessage, result.Message);
        }

        [Fact]
        public async Task KeepTimeslot_TimeslotNotAvailable_ReturnsFailure()
        {
            // This test simulates the scenario where no available slot is found within a year
            // Arrange
            var time = new TimeSpan(10, 0, 0); // 10 AM
            var endTime = time.Add(TimeSpan.FromMinutes(30));
            _timeSlotValidatorMock.Setup(v => v.IsValidTimeSlot(It.IsAny<DateTime>(), time, endTime))
                .Returns(TimeSlotValidationResult.Valid());
            _appointmentRepositoryMock.Setup(r => r.IsTimeSlotAvailable(It.IsAny<DateTime>(), time, endTime))
                .ReturnsAsync(false);

            // Act
            var result = await _service.KeepTimeslot(time);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Could not find an available slot within a year.", result.Message);
        }

        [Fact]
        public async Task KeepTimeslot_RepositoryThrowsException_ReturnsFailure()
        {
            // Arrange
            var time = new TimeSpan(10, 0, 0); // 10 AM
            var endTime = time.Add(TimeSpan.FromMinutes(30));
            var dateToCheck = DateTime.Now.TimeOfDay > time ? DateTime.Today.AddDays(1) : DateTime.Today;

            _timeSlotValidatorMock.Setup(v => v.IsValidTimeSlot(dateToCheck, time, endTime))
                .Returns(TimeSlotValidationResult.Valid());
            _appointmentRepositoryMock.Setup(r => r.IsTimeSlotAvailable(dateToCheck, time, endTime))
                .ReturnsAsync(true);
            _appointmentRepositoryMock.Setup(r => r.Add(It.IsAny<Appointment>()))
                .ThrowsAsync(new Exception()); // Simulate an exception during add

            // Act
            var result = await _service.KeepTimeslot(time);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("An error occurred while trying to keep the timeslot.", result.Message);
        }
    }
}
