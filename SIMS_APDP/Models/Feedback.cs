using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIMS_APDP.Models
{
    public class Feedback
    {
        [Key]
        public int FeedbackId { get; set; }

        // Khóa ngoại đến User
        public int UserId { get; set; }

        // Khóa ngoại đến Course (có thể null)
        public int? CourseId { get; set; }

        [Required]
        public string Message { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("UserId")]
        public User? User { get; set; }

        [ForeignKey("CourseId")]
        public Course? Course { get; set; }
    }
}
