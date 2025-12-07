namespace SIMS_APDP.Validators
{
    /// <summary>
    /// Course Validator Implementation - validates course data before saving
    /// </summary>
    public class CourseValidator : ICourseValidator
    {
        // Validate course name is not empty
        public (bool IsValid, string ErrorMessage) ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return (false, "Course name is required");

            return (true, "");
        }

        // Validate course credits is between 1-10
        public (bool IsValid, string ErrorMessage) ValidateCredits(int credits)
        {
            if (credits < 1 || credits > 10)
                return (false, "Credits must be between 1 and 10");

            return (true, "");
        }
    }
}
