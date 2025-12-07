namespace SIMS_APDP.DesignPatternMinh.State
{
    public class InProgressState : ICourseState
    {
        public string GetStatus() => "Đang học";
        public string GetBadgeClass() => "bg-warning text-dark";
        public bool CanRetake() => false;
    }
}