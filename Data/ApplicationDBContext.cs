using CRS.Models;
using DotNET.Models;
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
        public DbSet<Building> Buildings { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<User> User {  get; set; }

        // ✅ Fluent API config goes in OnModelCreating
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Reservation>()
                .Property(r => r.Status)
                .HasConversion<string>();  // Store enum as string
        
        
            // Prevent cascade delete to avoid "multiple cascade paths"
            modelBuilder.Entity<Course>()
                .HasOne(c => c.Building)
                .WithMany()
                .HasForeignKey(c => c.BuildingId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Course>()
                .HasOne(c => c.Room)
                .WithMany(r => r.Courses)
                .HasForeignKey(c => c.RoomId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Room>()
                .HasOne(r => r.Building)
                .WithMany()
                .HasForeignKey(r => r.BuildingId)
                .OnDelete(DeleteBehavior.Restrict);
        }


    }
}
