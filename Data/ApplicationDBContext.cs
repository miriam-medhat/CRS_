using CRS.Models;
using Microsoft.EntityFrameworkCore;

namespace CRS.Data
{
    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options)
            : base(options)
        {
        }

        // ✅ DbSets go here
        public DbSet<Course> Courses { get; set; }
        public DbSet<Reservation> Reservations { get; set; }

        // ✅ Fluent API config goes in OnModelCreating
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Reservation>()
                .Property(r => r.Status)
                .HasConversion<string>();  // Store enum as string
        }
    }
}
