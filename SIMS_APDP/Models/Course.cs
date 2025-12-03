using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIMS_APDP.Models
{
    public class Course
    {
        [Key]
        public int CourseId { get; set; }
        [Required, MaxLength(255)]
        public string CourseName { get; set; }
        [Required]
        public string Description { get; set; }
        [Required, Range(1, 10)]
        public int Credits { get; set; }
        public int? TeacherId { get; set; }// Thêm TeacherId để gán teacher cho course
        [ForeignKey("TeacherId")]
        public virtual User? Teacher { get; set; }// Navigation property
    }
}
