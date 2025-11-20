using Microsoft.EntityFrameworkCore;
using ContractMonthlyClaimSystem.Models;

namespace ContractMonthlyClaimSystem.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Claim> Claims { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<Review> Reviews { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed Roles (optional for testing)
            modelBuilder.Entity<UserRole>().HasData(
                new UserRole { RoleID = 1, RoleName = "Lecturer" },
                new UserRole { RoleID = 2, RoleName = "ProgrammeCoordinator" },
                new UserRole { RoleID = 3, RoleName = "AcademicManager" },
                new UserRole { RoleID = 4, RoleName = "HR" }
            );

            modelBuilder.Entity<User>().HasData(
                new User { UserID = 1, FullName = "Samkelisiwe Hlatshwayo", RoleID = 1, UserEmail = "sh@gmail.com", Password = "Password@sh1", ContactNumber = "0987654321", HourlyRate = 300m},
                new User { UserID = 2,  FullName = "Anele Mkhabela", RoleID = 2, UserEmail = "am@gmail.com", Password = "Password@am2", ContactNumber = "1234567890", HourlyRate = 350m },
                new User { UserID = 3, FullName = "Sonto Mthalane", RoleID = 3, UserEmail = "sm@gmail.com", Password = "Password@sm31", ContactNumber = "1357908642", HourlyRate = 400m }
                );
            // 🔹 Claim → User (Lecturer)
            modelBuilder.Entity<Claim>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserID)
                .OnDelete(DeleteBehavior.Restrict); // ✅ prevent cascade loops

            // 🔹 Review → Claim
            modelBuilder.Entity<Review>()
                .HasOne(r => r.Claim)
                .WithMany()
                .HasForeignKey(r => r.ClaimID)
                .OnDelete(DeleteBehavior.Cascade); // safe to cascade here

            // 🔹 Review → User (Reviewer)
            modelBuilder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserID)
                .OnDelete(DeleteBehavior.Restrict); // <— this is the key line

            // 🔹 Document → Claim
            modelBuilder.Entity<Document>()
                .HasOne(d => d.Claim)
                .WithMany(c => c.Documents)
                .HasForeignKey(d => d.ClaimID)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
