namespace SIMS_APDP.DesignPatternMinh.State
{
    public class InProgressState : ICourseState
    {
        public string GetStatus() => "Studying";
        public string GetBadgeClass() => "bg-warning text-dark";

    }
}