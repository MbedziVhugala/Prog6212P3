using Microsoft.EntityFrameworkCore;
using Prog6212.Models;

namespace Prog6212.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<LecturerClaim> LecturerClaims { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Id)
                      .ValueGeneratedOnAdd(); // ADD THIS LINE - Let database generate ID

                entity.Property(u => u.FullName).IsRequired().HasMaxLength(100);
                entity.Property(u => u.Email).IsRequired().HasMaxLength(100);
                entity.Property(u => u.Password).IsRequired().HasMaxLength(100);
                entity.Property(u => u.Role).IsRequired().HasMaxLength(20);
                entity.Property(u => u.HourlyRate).HasColumnType("decimal(18,2)");
                entity.Property(u => u.CreatedDate).HasDefaultValueSql("GETDATE()");
            });

            // Configure LecturerClaim entity
            modelBuilder.Entity<LecturerClaim>(entity =>
            {
                entity.HasKey(lc => lc.Id);
                entity.Property(lc => lc.Id)
                      .ValueGeneratedOnAdd(); // ADD THIS LINE - Let database generate ID

                entity.Property(lc => lc.HoursWorked).HasColumnType("decimal(18,2)");
                entity.Property(lc => lc.HourlyRate).HasColumnType("decimal(18,2)");
                entity.Property(lc => lc.Status).HasMaxLength(20).HasDefaultValue("Pending");
                entity.Property(lc => lc.SubmissionDate).HasDefaultValueSql("GETDATE()");
                entity.Property(lc => lc.AdditionalNotes).HasMaxLength(1000);

                // IGNORE the computed property - THIS IS CRITICAL
                entity.Ignore(lc => lc.TotalAmount);

                // Relationships
                entity.HasOne(lc => lc.User)
                      .WithMany()
                      .HasForeignKey(lc => lc.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(lc => lc.Approver)
                      .WithMany()
                      .HasForeignKey(lc => lc.ApprovedBy)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}