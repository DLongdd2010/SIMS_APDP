using SIMS_APDP.Data;
using SIMS_APDP.Models;
using Microsoft.EntityFrameworkCore;

namespace SIMS_APDP.Services
{
    public class TimetableService : ITimetableService
    {
        private readonly ApplicationDbContext _context;
        private static readonly string[] ValidDays = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };

        public TimetableService(ApplicationDbContext context)
        {
            _context = context;
        }

        public (int Total, List<dynamic> Items) GetTimetables(int? course = null, string semester = "", string day = "", int page = 1, int pageSize = 10)
        {
            var query = _context.Timetables
                .Include(t => t.Course)
                .Include(t => t.Room)
                .AsQueryable();

            if (course.HasValue && course.Value > 0)
                query = query.Where(t => t.CourseId == course.Value);

            if (!string.IsNullOrWhiteSpace(semester))
                query = query.Where(t => t.Semester == semester);

            if (!string.IsNullOrWhiteSpace(day))
                query = query.Where(t => t.DayOfWeek == day);

            var total = query.Count();

            var items = query
                .OrderBy(t => t.DayOfWeek)
                .ThenBy(t => t.StartTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList()
                .Select(t => new
                {
                    id = t.TimetableId,
                    courseName = t.Course != null ? t.Course.CourseName : "N/A",
                    courseId = t.CourseId,
                    dayOfWeek = t.DayOfWeek,
                    startTime = t.StartTime.ToString(@"hh\:mm"),
                    endTime = t.EndTime.ToString(@"hh\:mm"),
                    roomName = t.Room != null ? t.Room.RoomName : "N/A",
                    roomId = t.RoomId,
                    semester = t.Semester
                })
                .Cast<dynamic>()
                .ToList();

            return (total, items);
        }

        public dynamic GetTimetableById(int timetableId)
        {
            var timetable = _context.Timetables.Find(timetableId);
            if (timetable == null)
                return null;

            return new
            {
                id = timetable.TimetableId,
                courseId = timetable.CourseId,
                dayOfWeek = timetable.DayOfWeek,
                startTime = timetable.StartTime.ToString(@"hh\:mm"),
                endTime = timetable.EndTime.ToString(@"hh\:mm"),
                roomId = timetable.RoomId,
                semester = timetable.Semester
            };
        }

        public void AddTimetable(Timetable timetable)
        {
            _context.Timetables.Add(timetable);
            _context.SaveChanges();
        }

        public void UpdateTimetable(Timetable timetable)
        {
            _context.Timetables.Update(timetable);
            _context.SaveChanges();
        }

        public void DeleteTimetable(int timetableId)
        {
            var timetable = _context.Timetables.Find(timetableId);
            if (timetable != null)
            {
                _context.Timetables.Remove(timetable);
                _context.SaveChanges();
            }
        }

        public bool TimetableExists(int timetableId)
        {
            return _context.Timetables.Any(t => t.TimetableId == timetableId);
        }

        public IEnumerable<dynamic> GetRooms()
        {
            return _context.Rooms
                .Select(r => new { id = r.RoomId, name = r.RoomName })
                .Cast<dynamic>()
                .ToList();
        }

        public IEnumerable<string> GetValidDays()
        {
            return ValidDays;
        }
    }
}
