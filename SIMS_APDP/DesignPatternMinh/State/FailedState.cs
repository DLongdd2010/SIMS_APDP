using SIMS_APDP.DesignPatternMinh.State;

public class FailedState : ICourseState
{
    public string GetStatus() => "Trượt - Có thể học lại";
    public string GetBadgeClass() => "bg-danger";
    public bool CanRetake() => true;
}