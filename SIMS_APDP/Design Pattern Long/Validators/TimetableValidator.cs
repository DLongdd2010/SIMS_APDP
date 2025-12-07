using SIMS_APDP.Data;
using SIMS_APDP.Services;

namespace SIMS_APDP.Validators
{
    public class TimetableValidator : ITimetableValidator
    {
        private readonly ApplicationDbContext _context;
        private readonly ITimetableService _timetableService;
        private readonly ICourseService _courseService;

        public TimetableValidator(ApplicationDbContext context, ITimetableService timetableService, ICourseService courseService)
        {
            _context = context;
            _timetableService = timetableService;
            _courseService = courseService;
        }

        public (bool IsValid, string ErrorMessage) ValidateDayOfWeek(string dayOfWeek)
        {
            if (string.IsNullOrWhiteSpace(dayOfWeek))
                return (false, "Day is required");

            var validDays = _timetableService.GetValidDays().ToList();
            if (!validDays.Contains(dayOfWeek.Trim()))
                return (false, $"Invalid day: '{dayOfWeek}'. Valid days: {string.Join(", ", validDays)}");

            return (true, "");
        }

        public (bool IsValid, string ErrorMessage) ValidateTime(string startTime, string endTime)
        {
            if (string.IsNullOrWhiteSpace(startTime) || string.IsNullOrWhiteSpace(endTime))
                return (false, "Time is required");

            if (!TimeSpan.TryParse(startTime, out var start))
                return (false, "Invalid start time format (use HH:mm)");

            if (!TimeSpan.TryParse(endTime, out var end))
                return (false, "Invalid end time format (use HH:mm)");

            if (start >= end)
                return (false, "Start time must be before end time");

            return (true, "");
        }

        public (bool IsValid, string ErrorMessage) ValidateCourseId(int courseId)
        {
            if (courseId <= 0)
                return (false, "Course is required");

            if (!_courseService.CourseExists(courseId))
                return (false, "Course does not exist");

            return (true, "");
        }

        public (bool IsValid, string ErrorMessage) ValidateRoomId(int? roomId)
        {
            if (!roomId.HasValue || roomId.Value <= 0)
                return (true, "");

            var roomExists = _context.Rooms.Any(r => r.RoomId == roomId.Value);
            if (!roomExists)
                return (false, "Room does not exist");

            return (true, "");
        }

        public (bool IsValid, string ErrorMessage) ValidateSemester(string semester)
        {
            if (string.IsNullOrWhiteSpace(semester))
                return (false, "Semester is required");

            return (true, "");
        }

        public (bool IsValid, string ErrorMessage) ValidateConflict(int? roomId, string day, TimeSpan start, TimeSpan end, string semester, int? excludeId)
        {
            if (!roomId.HasValue || roomId.Value <= 0)
                return (true, "");

            var conflict = _context.Timetables
                .Any(t => t.RoomId == roomId.Value 
                        && t.DayOfWeek == day 
                        && t.Semester == semester 
                        && (!excludeId.HasValue || t.TimetableId != excludeId.Value)
                        && t.StartTime < end 
                        && t.EndTime > start);

            if (conflict)
                return (false, "Room is already occupied at this time.");

            return (true, "");
        }
    }
}
