using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIMS_APDP.Models
{
    public class Timetable
    {
        private static readonly string[] ValidDays = 
        { 
            "Thứ Hai", "Thứ Ba", "Thứ Tư", "Thứ Năm", "Thứ Sáu", "Thứ Bảy"
        };

        [Key]
        public int TimetableId { get; set; }
        
        [Required]
        public int CourseId { get; set; }
        
        [Required, MaxLength(50)]
        public string DayOfWeek { get; set; }
        
        [Required]
        public TimeSpan StartTime { get; set; }
        
        [Required]
        public TimeSpan EndTime { get; set; }
        
        [Required, MaxLength(10)]
        public string Semester { get; set; }
        
        public int? RoomId { get; set; }
        
        [ForeignKey("CourseId")]
        public virtual Course? Course { get; set; }
        
        [ForeignKey("RoomId")]
        public virtual Room? Room { get; set; }

        // Validation method
        public bool IsValidDay()
        {
            return ValidDays.Contains(DayOfWeek?.Trim());
        }
    }
}
