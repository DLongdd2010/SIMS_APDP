namespace SIMS_APDP.Validators
{
    public class CourseValidator : ICourseValidator
    {
        public (bool IsValid, string ErrorMessage) ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return (false, "Course name is required");

            return (true, "");
        }

        public (bool IsValid, string ErrorMessage) ValidateCredits(int credits)
        {
            if (credits < 1 || credits > 10)
                return (false, "Credits must be between 1 and 10");

            return (true, "");
        }
    }
}
