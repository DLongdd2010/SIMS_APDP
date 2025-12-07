namespace SIMS_APDP.DesignPatternMinh.State
{
    public interface ICourseState
    {
        string GetStatus();      // Đang học, Đạt, Trượt, Có thể học lại
        string GetBadgeClass();  // bg-warning, bg-success, bg-danger
        bool CanRetake();
    }
}