using System.ComponentModel.DataAnnotations;

namespace SIMS_APDP.Models
{
    public class StudentCourse
    {
        public int UserId { get; set; }
        public int CourseId { get; set; }
        [Required, MaxLength(10)]
        public string Semester { get; set; } = "";

        public DateTime EnrolledDate { get; set; } = DateTime.Now;

        // Navigation properties
        public User? User { get; set; }
        public Course? Course { get; set; }
    }
}
