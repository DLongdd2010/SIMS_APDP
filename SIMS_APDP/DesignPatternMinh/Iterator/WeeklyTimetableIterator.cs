using SIMS_APDP.Models;

namespace SIMS_APDP.DesignPatternMinh.Iterator
{
    public class WeeklyTimetableIterator : ITimetableIterator
    {
        private readonly List<Timetable> _timetables;
        private int _current = 0;
        private static readonly string[] Days = { "Thứ Hai", "Thứ Ba", "Thứ Tư", "Thứ Năm", "Thứ Sáu", "Thứ Bảy" };

        public WeeklyTimetableIterator(List<Timetable> timetables)
        {
            _timetables = timetables
                .OrderBy(t => Array.IndexOf(Days, t.DayOfWeek))
                .ThenBy(t => t.StartTime)
                .ToList();
        }

        public bool HasNext() => _current < _timetables.Count;

        public Timetable Next()
        {
            return _timetables[_current++];
        }
    }
}