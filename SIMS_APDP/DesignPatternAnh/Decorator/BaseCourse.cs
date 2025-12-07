using SIMS_APDP.Models;

namespace SIMS_APDP.DesignPatternAnh.Decorator
{
    public class BaseCourse : ICourseComponent
    {
        private readonly Course _course;

        public BaseCourse(Course course) => _course = course;

        public Course GetCourse() => _course;
        public int GetStudentCount() => 0; // Default
    }
}