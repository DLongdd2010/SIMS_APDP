namespace SIMS_APDP.Validators
{
    public interface ITimetableValidator
    {
        (bool IsValid, string ErrorMessage) ValidateDayOfWeek(string dayOfWeek);
        (bool IsValid, string ErrorMessage) ValidateTime(string startTime, string endTime);
        (bool IsValid, string ErrorMessage) ValidateCourseId(int courseId);
        (bool IsValid, string ErrorMessage) ValidateRoomId(int? roomId);
        (bool IsValid, string ErrorMessage) ValidateSemester(string semester);
        (bool IsValid, string ErrorMessage) ValidateConflict(int? roomId, string day, TimeSpan start, TimeSpan end, string semester, int? dataToExclude);
    }
}
