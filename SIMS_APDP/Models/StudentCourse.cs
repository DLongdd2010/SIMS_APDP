namespace SIMS_APDP.Models
{
    public class StudentCourse
    {
        public int UserId { get; set; }
        public int CourseId { get; set; }
        public string Semester { get; set; }

        // Các cột khác
        public DateTime EnrolledDate { get; set; } = DateTime.Now;

        // Navigation properties
        public User User { get; set; }
        public Course Course { get; set; }
    }
}
