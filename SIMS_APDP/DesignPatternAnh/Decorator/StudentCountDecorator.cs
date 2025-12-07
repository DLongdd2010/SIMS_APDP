using SIMS_APDP.Models;

namespace SIMS_APDP.DesignPatternAnh.Decorator
{
    public class StudentCountDecorator : ICourseComponent
    {
        private readonly ICourseComponent _component;
        private readonly int _studentCount;

        public StudentCountDecorator(ICourseComponent component, int studentCount)
        {
            _component = component;
            _studentCount = studentCount;
        }

        public Course GetCourse() => _component.GetCourse();
        public int GetStudentCount() => _studentCount;
    }
}