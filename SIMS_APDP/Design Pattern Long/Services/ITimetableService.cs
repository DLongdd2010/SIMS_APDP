using SIMS_APDP.Models;

namespace SIMS_APDP.Services
{
    public interface ITimetableService
    {
        (int Total, List<dynamic> Items) GetTimetables(int? course = null, string semester = "", string day = "", int page = 1, int pageSize = 10);
        dynamic GetTimetableById(int timetableId);
        void AddTimetable(Timetable timetable);
        void UpdateTimetable(Timetable timetable);
        void DeleteTimetable(int timetableId);
        bool TimetableExists(int timetableId);
        IEnumerable<dynamic> GetRooms();
        IEnumerable<string> GetValidDays();
    }
}
