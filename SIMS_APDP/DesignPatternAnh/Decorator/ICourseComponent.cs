using SIMS_APDP.Models;

namespace SIMS_APDP.DesignPatternAnh.Decorator
{
    public interface ICourseComponent
    {
        Course GetCourse();
        int GetStudentCount();
        // Có thể thêm methods khác
    }
}