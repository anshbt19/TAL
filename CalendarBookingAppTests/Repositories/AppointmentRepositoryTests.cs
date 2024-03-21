using System;
using System.Threading.Tasks;
using CalendarBookingApp.Data;
using CalendarBookingApp.Entities;
using CalendarBookingApp.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CalendarBookingAppTests
{
    public class AppointmentRepositoryTests
    {
        [Fact]
        public async Task Add_AppointmentIsAdded()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<AppointmentContext>()
                .UseInMemoryDatabase(databaseName: "AddAppointmentTest") // Unique name for the in-memory database
                .Options;

            using var context = new AppointmentContext(options);
            var repository = new AppointmentRepository(context);
            var appointment = new Appointment
            {
                AppointmentDate = DateTime.Now,
                StartTime = TimeSpan.FromHours(10),
                EndTime = TimeSpan.FromHours(11),
            };

            // Act
            await repository.Add(appointment);

            // Assert
            var addedAppointment = await context.Appointments.FirstOrDefaultAsync(a => a == appointment);
            Assert.NotNull(addedAppointment);
            Assert.Equal(appointment.AppointmentDate, addedAppointment.AppointmentDate);
            Assert.Equal(appointment.StartTime, addedAppointment.StartTime);
            Assert.Equal(appointment.EndTime, addedAppointment.EndTime);
        }

        [Fact]
        public async Task Delete_AppointmentIsDeleted()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<AppointmentContext>()
                .UseInMemoryDatabase(databaseName: "DeleteAppointmentTest") 
                .Options;

            var context = new AppointmentContext(options);
            var repository = new AppointmentRepository(context);

            var appointment = new Appointment
            {
                AppointmentDate = DateTime.Now,
                StartTime = TimeSpan.FromHours(10),
                EndTime = TimeSpan.FromHours(11),
            };

            context.Appointments.Add(appointment);
            await context.SaveChangesAsync();

            // Act
            await repository.Delete(appointment);
            var deletedAppointment = await context.Appointments.FirstOrDefaultAsync(a => a == appointment);

            // Assert
            Assert.Null(deletedAppointment);
        }

        [Fact]
        public async Task FindByDateTime_ExistingAppointment_ReturnsAppointment()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<AppointmentContext>()
                .UseInMemoryDatabase(databaseName: "FindByDateTimeTest")
                .Options;

            var context = new AppointmentContext(options);
            var repository = new AppointmentRepository(context);

            var targetDate = DateTime.Today;
            var startTime = TimeSpan.FromHours(10);
            var endTime = TimeSpan.FromHours(11);

            var appointment = new Appointment
            {
                AppointmentDate = targetDate,
                StartTime = startTime,
                EndTime = endTime,
            };

            context.Appointments.Add(appointment);
            await context.SaveChangesAsync();

            // Act
            var result = await repository.FindByDateTime(targetDate, startTime);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(appointment.AppointmentDate, result.AppointmentDate);
            Assert.Equal(appointment.StartTime, result.StartTime);
            Assert.Equal(appointment.EndTime, result.EndTime);
        }

        [Fact]
        public async Task FindByDateTime_NonExistingAppointment_ReturnsNull()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<AppointmentContext>()
                .UseInMemoryDatabase(databaseName: "FindByDateTimeNonExistingTest")
                .Options;

            var context = new AppointmentContext(options);
            var repository = new AppointmentRepository(context);

            var targetDate = DateTime.Today;
            var startTime = TimeSpan.FromHours(10);

            // No appointment is added to the context to simulate non-existence

            // Act
            var result = await repository.FindByDateTime(targetDate, startTime);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task IsTimeSlotAvailable_SlotIsAvailable_ReturnsTrue()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<AppointmentContext>()
                .UseInMemoryDatabase(databaseName: "TimeSlotAvailableTest")
                .Options;

            var context = new AppointmentContext(options);
            var repository = new AppointmentRepository(context);

            var date = DateTime.Today;
            // Existing appointment from 10 AM to 11 AM
            var existingAppointment = new Appointment
            {
                AppointmentDate = date,
                StartTime = TimeSpan.FromHours(10),
                EndTime = TimeSpan.FromHours(11),
            };

            context.Appointments.Add(existingAppointment);
            await context.SaveChangesAsync();

            // Checking for availability from 11 AM to 12 PM
            var startTime = TimeSpan.FromHours(11);
            var endTime = TimeSpan.FromHours(12);

            // Act
            var isAvailable = await repository.IsTimeSlotAvailable(date, startTime, endTime);

            // Assert
            Assert.True(isAvailable);
        }

        [Fact]
        public async Task IsTimeSlotAvailable_SlotIsNotAvailable_ReturnsFalse()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<AppointmentContext>()
                .UseInMemoryDatabase(databaseName: "TimeSlotNotAvailableTest")
                .Options;

            var context = new AppointmentContext(options);
            var repository = new AppointmentRepository(context);

            var date = DateTime.Today;
            // Existing appointment from 10 AM to 11 AM
            var existingAppointment = new Appointment
            {
                AppointmentDate = date,
                StartTime = TimeSpan.FromHours(10),
                EndTime = TimeSpan.FromHours(11),
            };

            context.Appointments.Add(existingAppointment);
            await context.SaveChangesAsync();

            // Checking for availability from 10:30 AM to 11:30 AM which overlaps with the existing appointment
            var startTime = TimeSpan.FromHours(10).Add(TimeSpan.FromMinutes(30));
            var endTime = TimeSpan.FromHours(11).Add(TimeSpan.FromMinutes(30));

            // Act
            var isAvailable = await repository.IsTimeSlotAvailable(date, startTime, endTime);

            // Assert
            Assert.False(isAvailable);
        }

        [Fact]
        public async Task FindNextAvailableSlot_NoSlotAvailable_ReturnsNull()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<AppointmentContext>()
                .UseInMemoryDatabase(databaseName: "FindNextAvailableSlotTest3")
                .Options;

            var context = new AppointmentContext(options);
            var repository = new AppointmentRepository(context);

            var date = DateTime.Today;
            // Appointment taking up the entire workday
            var allDayAppointment = new Appointment
            {
                AppointmentDate = date,
                StartTime = TimeSpan.FromHours(9),
                EndTime = TimeSpan.FromHours(17),
            };

            context.Appointments.Add(allDayAppointment);
            await context.SaveChangesAsync();

            var startTime = TimeSpan.FromHours(9);

            // Act
            var nextAvailableSlot = await repository.FindNextAvailableSlot(date, startTime);

            // Assert
            Assert.Null(nextAvailableSlot); // No slot should be available
        }
    }
}
