using SIMS_APDP.DesignPatternMinh.State;

public class PassedState : ICourseState
{
    public string GetStatus() => "Đạt";
    public string GetBadgeClass() => "bg-success";
    public bool CanRetake() => false;
}