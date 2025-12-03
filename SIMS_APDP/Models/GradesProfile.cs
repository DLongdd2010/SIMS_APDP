using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIMS_APDP.Models
{
    public class GradesProfile
    {
        [Key]
        public int GradesID { get; set; }

        // Khóa ngoại đến StudentCourse (UserID + CourseID + Semester)
        public int UserId { get; set; }
        public int CourseId { get; set; }
        [Required, MaxLength(10)]
        public string Semester { get; set; }

        public decimal? Grade { get; set; } // Có thể null
        [MaxLength(20)]
        public string Ranking { get; set; }

        // Ai cập nhật điểm
        public int? UpdatedBy { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("UserId,CourseId,Semester")]
        public StudentCourse StudentCourse { get; set; }

        [ForeignKey("UpdatedBy")]
        public User UpdatedByUser { get; set; }
    }
}
