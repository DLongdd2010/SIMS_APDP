using Microsoft.EntityFrameworkCore;
using SIMS_APDP.Models;

namespace SIMS_APDP.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Course> Courses { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<GradesProfile> GradesProfiles { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Timetable> Timetables { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<StudentCourse> StudentCourses { get; set; }

        // Configure model / relationships / constraints
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Seed roles
            modelBuilder.Entity<Role>().HasData(
                new Role { RoleId = 1, RoleName = "Admin" },
                new Role { RoleId = 2, RoleName = "Teacher" },
                new Role { RoleId = 3, RoleName = "Student" }
            );

            // User <-> Role (explicit FK RoleId)
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict); // prevent role deletion if users exist

            // Course <-> Teacher (Course.TeacherId optional)
            modelBuilder.Entity<Course>()
                .HasOne(c => c.Teacher)
                .WithMany()
                .HasForeignKey(c => c.TeacherId)
                .OnDelete(DeleteBehavior.SetNull); // keep course if teacher removed

            // StudentCourse composite key and relations
            modelBuilder.Entity<StudentCourse>()
                .HasKey(sc => new { sc.UserId, sc.CourseId, sc.Semester });

            modelBuilder.Entity<StudentCourse>()
                .HasOne(sc => sc.User)
                .WithMany()
                .HasForeignKey(sc => sc.UserId)
                .OnDelete(DeleteBehavior.Restrict); // prevent deleting user if enrollments exist

            modelBuilder.Entity<StudentCourse>()
                .HasOne(sc => sc.Course)
                .WithMany()
                .HasForeignKey(sc => sc.CourseId)
                .OnDelete(DeleteBehavior.Restrict); // prevent deleting course if enrollments exist

            // GradesProfile: composite FK to StudentCourse (UserId, CourseId, Semester)
            modelBuilder.Entity<GradesProfile>()
                .HasOne(g => g.StudentCourse)
                .WithMany()
                .HasForeignKey(g => new { g.UserId, g.CourseId, g.Semester })
                .OnDelete(DeleteBehavior.Restrict); // keep grades intact; prevent deleting studentcourse while grades exist

            // GradesProfile: UpdatedBy -> User (optional)
            modelBuilder.Entity<GradesProfile>()
                .HasOne(g => g.UpdatedByUser)
                .WithMany()
                .HasForeignKey(g => g.UpdatedBy)
                .OnDelete(DeleteBehavior.SetNull);

            // Timetable: CourseId and RoomId as explicit FKs
            modelBuilder.Entity<Timetable>()
                .HasOne(t => t.Course)
                .WithMany()
                .HasForeignKey(t => t.CourseId)
                .OnDelete(DeleteBehavior.SetNull); // if course removed, keep timetable but nullify course

            modelBuilder.Entity<Timetable>()
                .HasOne(t => t.Room)
                .WithMany()
                .HasForeignKey(t => t.RoomId)
                .OnDelete(DeleteBehavior.Restrict); // don't allow room deletion when timetables reference it

            // Feedback relations
            modelBuilder.Entity<Feedback>()
                .HasOne(f => f.User)
                .WithMany()
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Restrict); // keep feedbacks when user deletion is protected

            modelBuilder.Entity<Feedback>()
                .HasOne(f => f.Course)
                .WithMany()
                .HasForeignKey(f => f.CourseId)
                .OnDelete(DeleteBehavior.SetNull);

            base.OnModelCreating(modelBuilder);
        }
    }   
}
