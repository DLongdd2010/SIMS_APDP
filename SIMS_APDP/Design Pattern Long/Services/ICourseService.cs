using SIMS_APDP.Models;

namespace SIMS_APDP.Services
{
    public interface ICourseService
    {
        IEnumerable<dynamic> GetCourses(string search = "", int page = 1, int pageSize = 10, string sort = "name_asc");
        (int Total, List<dynamic> Items) GetCoursesWithPagination(string search = "", int page = 1, int pageSize = 10, string sort = "name_asc");
        Course GetCourseById(int courseId);
        void AddCourse(Course course);
        void UpdateCourse(Course course);
        void DeleteCourse(int courseId);
        bool CourseExists(int courseId);
        IEnumerable<dynamic> GetDepartments();
        IEnumerable<dynamic> GetCoursesSimple();
        
        // Student enrollment methods
        int GetStudentCountInCourse(int courseId, string semester = "");
        (int Total, List<dynamic> Items) GetStudentsInCourse(int courseId, string semester = "", int page = 1, int pageSize = 10);
        void EnrollStudentInCourse(int userId, int courseId, string semester);
        void RemoveStudentFromCourse(int userId, int courseId, string semester);
        bool IsStudentEnrolled(int userId, int courseId, string semester);
        bool CanEnrollStudent(int courseId, string semester);
    }
}
