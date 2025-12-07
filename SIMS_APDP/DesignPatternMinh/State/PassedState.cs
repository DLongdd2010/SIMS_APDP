using SIMS_APDP.DesignPatternMinh.State;

public class PassedState : ICourseState
{
    public string GetStatus() => "Pass";
    public string GetBadgeClass() => "bg-success";

}