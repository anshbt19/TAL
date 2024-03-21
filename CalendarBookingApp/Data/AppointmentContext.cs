using CalendarBookingApp.Entities;
using Microsoft.EntityFrameworkCore;

namespace CalendarBookingApp.Data
{
    #pragma warning restore format
    public class AppointmentContext : DbContext
    {
        public DbSet<Appointment>? Appointments { get; set; }

        public AppointmentContext(DbContextOptions<AppointmentContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Appointment>().ToTable("Appointments");
        }
    }
}
