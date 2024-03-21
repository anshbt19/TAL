using CalendarBookingApp.Services;
using CalendarBookingApp.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using Microsoft.Extensions.Configuration;
using CalendarBookingApp.Validators;
using CalendarBookingApp.Data;
using CalendarBookingApp.Utilities;

namespace CalendarBookingApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Build configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Database connection string 'DefaultConnection' is not configured.");
            }

            // Set up Dependency Injection
            var serviceProvider = new ServiceCollection()
                .AddDbContext<AppointmentContext>(options => 
                    options.UseSqlServer(connectionString))
                .AddScoped<IAppointmentRepository, AppointmentRepository>()
                .AddScoped<AppointmentService>()
                .AddScoped<TimeSlotValidator>()
                .BuildServiceProvider();

            if (args.Length == 0)
            {
                Console.WriteLine("No command provided.");
                return;
            }

            var command = args[0].ToUpper();
            var appointmentService = serviceProvider.GetService<AppointmentService>();

            // Checking the appointmentService instance
            if (appointmentService == null)
            {
                Console.WriteLine("Appointment service is not available.");
                return;
            }

            try
            {
                switch (command)
                {
                    case "ADD":
                        await AddAppointment(args, appointmentService);
                        break;
                    case "DELETE":
                        await DeleteAppointment(args, appointmentService);
                        break;
                    case "FIND":
                        await FindAppointment(args, appointmentService);
                        break;
                    case "KEEP":
                        await KeepTimeslot(args, appointmentService);
                        break;
                    default:
                        Console.WriteLine("Invalid command.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
        }

        private static async Task AddAppointment(string[] args, AppointmentService service)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("Incorrect number of arguments for ADD command. Expecting date (dd/MM) and time (HH:mm).");
                return;
            }

            if (!DateTimeUtils.TryParseDateAndTime(args[1], args[2], out var date, out var time))
            {
                // The error message will be handled by the TryParseDateAndTime method
                return;
            }

            var result = await service.AddAppointment(date, time);
            Console.WriteLine(result.Message);
        }

        private static async Task DeleteAppointment(string[] args, AppointmentService service)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("Incorrect number of arguments for DELETE command. Expecting date (dd/MM) and time (HH:mm).");
                return;
            }

            if (!DateTimeUtils.TryParseDateAndTime(args[1], args[2], out var date, out var time))
            {
                return;
            }

            var result = await service.DeleteAppointment(date, time);
            Console.WriteLine(result.Message);
        }

        private static async Task FindAppointment(string[] args, AppointmentService service)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Incorrect number of arguments for FIND command. Expecting date (dd/MM).");
                return;
            }

            if (!DateTimeUtils.TryParseDate(args[1], out var date))
            {
                Console.WriteLine("Invalid date format for FIND command. Please use 'dd/MM' format.");
                return;
            }

            var result = await service.FindAppointment(date);
            Console.WriteLine(result.Message);
        }

        private static async Task KeepTimeslot(string[] args, AppointmentService service)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Incorrect number of arguments for KEEP command. Expecting time (HH:mm).");
                return;
            }

            if (!DateTimeUtils.TryParseTime(args[1], out var time))
            {
                Console.WriteLine("Invalid time format for KEEP command. Please use 'HH:mm' format.");
                return;
            }

            var result = await service.KeepTimeslot(time);
            Console.WriteLine(result.Message);
        }
    }
}
