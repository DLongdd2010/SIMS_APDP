using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIMS_APDP.Models
{
    public class GradesProfile
    {
        [Key]
        public int GradesID { get; set; }

        // FK parts for composite relation to StudentCourse
        public int UserId { get; set; }
        public int CourseId { get; set; }

        [Required, MaxLength(10)]
        public string Semester { get; set; }

        public decimal? Grade { get; set; }

        [MaxLength(20)]
        public string Ranking { get; set; }

        // Who updated
        public int? UpdatedBy { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        // Composite FK configured in Fluent API
        public StudentCourse StudentCourse { get; set; }

        [ForeignKey("UpdatedBy")]
        public User UpdatedByUser { get; set; }
    }
}
