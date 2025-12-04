using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIMS_APDP.Models
{
    public class Timetable
    {
        [Key]
        public int TimetableId { get; set; }
        [Required]
        public string DayOfWeek { get; set; }
        [Required]
        public TimeOnly StartTime { get; set; }
        [Required]
        public TimeOnly EndTime { get; set; }
        [Required]
        public string Semester { get; set; }
        public int CourseId { get; set; }
        public int? RoomId { get; set; }
        [ForeignKey("CourseId")]
        public Course? Course { get; set; }
        [ForeignKey("RoomId")]
        public Room? Room { get; set; }
    }
}
