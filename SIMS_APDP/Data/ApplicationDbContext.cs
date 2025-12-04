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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ======================
            // ROLE TABLE
            // ======================
            modelBuilder.Entity<Role>()
                .ToTable("Role");

            // ======================
            // USER TABLE
            // ======================
            modelBuilder.Entity<User>()
                .ToTable("User");

            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            // ======================
            // COURSE TABLE
            // ======================
            modelBuilder.Entity<Course>()
                .ToTable("Course");

            modelBuilder.Entity<Course>()
                .HasOne(c => c.Teacher)
                .WithMany()
                .HasForeignKey(c => c.TeacherId)
                .OnDelete(DeleteBehavior.SetNull);

            // ======================
            // ROOM TABLE
            // ======================
            modelBuilder.Entity<Room>()
                .ToTable("Room");

            // ======================
            // TIMETABLE TABLE
            // ======================
            modelBuilder.Entity<Timetable>()
                .ToTable("Timetable");

            modelBuilder.Entity<Timetable>()
                .HasOne(t => t.Course)
                .WithMany()
                .HasForeignKey(t => t.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Timetable>()
                .HasOne(t => t.Room)
                .WithMany()
                .HasForeignKey(t => t.RoomId)
                .OnDelete(DeleteBehavior.Restrict);

            // ======================
            // STUDENT COURSE TABLE
            // ======================
            modelBuilder.Entity<StudentCourse>()
                .ToTable("Student_Course");

            modelBuilder.Entity<StudentCourse>()
                .HasKey(sc => new { sc.UserId, sc.CourseId, sc.Semester });

            modelBuilder.Entity<StudentCourse>()
                .HasOne(sc => sc.User)
                .WithMany()
                .HasForeignKey(sc => sc.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StudentCourse>()
                .HasOne(sc => sc.Course)
                .WithMany()
                .HasForeignKey(sc => sc.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            // ======================
            // GRADES PROFILE TABLE
            // ======================
            modelBuilder.Entity<GradesProfile>()
                .ToTable("GradesProfile");

            modelBuilder.Entity<GradesProfile>()
                .HasOne(g => g.StudentCourse)
                .WithMany()
                .HasForeignKey(g => new { g.UserId, g.CourseId, g.Semester })
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<GradesProfile>()
                .HasOne(g => g.UpdatedByUser)
                .WithMany()
                .HasForeignKey(g => g.UpdatedBy)
                .OnDelete(DeleteBehavior.SetNull);

            // ======================
            // FEEDBACK TABLE
            // ======================
            modelBuilder.Entity<Feedback>()
                .ToTable("Feedback");

            modelBuilder.Entity<Feedback>()
                .HasOne(f => f.User)
                .WithMany()
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Feedback>()
                .HasOne(f => f.Course)
                .WithMany()
                .HasForeignKey(f => f.CourseId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
