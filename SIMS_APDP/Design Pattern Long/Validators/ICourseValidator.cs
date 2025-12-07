namespace SIMS_APDP.Validators
{
    /// <summary>
    /// Course Validator Interface - validates course input
    /// </summary>
    public interface ICourseValidator
    {
        // Validate course name (required, not empty)
        (bool IsValid, string ErrorMessage) ValidateName(string name);

        // Validate course credits (1-10)
        (bool IsValid, string ErrorMessage) ValidateCredits(int credits);
    }
}
