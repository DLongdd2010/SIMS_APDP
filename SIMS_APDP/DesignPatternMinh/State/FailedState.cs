using SIMS_APDP.DesignPatternMinh.State;

public class FailedState : ICourseState
{
    public string GetStatus() => "fail";
    public string GetBadgeClass() => "bg-danger";
}