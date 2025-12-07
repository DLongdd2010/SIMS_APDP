namespace SIMS_APDP.Validators
{
    public interface ICourseValidator
    {
        (bool IsValid, string ErrorMessage) ValidateName(string name);
        (bool IsValid, string ErrorMessage) ValidateCredits(int credits);
    }
}
