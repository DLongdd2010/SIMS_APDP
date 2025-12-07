namespace SIMS_APDP.Validators
{
    /// <summary>
    /// Timetable Validator Interface - validates timetable input
    /// </summary>
    public interface ITimetableValidator
    {
        // Validate day of week (Monday - Saturday)
        (bool IsValid, string ErrorMessage) ValidateDayOfWeek(string dayOfWeek);

        // Validate start/end times (format and logic)
        (bool IsValid, string ErrorMessage) ValidateTime(string startTime, string endTime);

        // Validate course exists
        (bool IsValid, string ErrorMessage) ValidateCourseId(int courseId);

        // Validate room exists (optional)
        (bool IsValid, string ErrorMessage) ValidateRoomId(int? roomId);

        // Validate semester is provided
        (bool IsValid, string ErrorMessage) ValidateSemester(string semester);

        // Validate no room scheduling conflict
        (bool IsValid, string ErrorMessage) ValidateConflict(int? roomId, string day, TimeSpan start, TimeSpan end, string semester, int? dataToExclude);
    }
}
