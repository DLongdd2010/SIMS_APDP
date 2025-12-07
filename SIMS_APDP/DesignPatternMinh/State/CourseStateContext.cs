namespace SIMS_APDP.DesignPatternMinh.State
{
    public class CourseStateContext
    {
        public ICourseState State { get; private set; }

        public CourseStateContext(decimal? grade)
        {
            State = grade switch
            {
                null => new InProgressState(),
                >= 4 => new PassedState(),
                _ => new FailedState()
            };
        }

        public string Status => State.GetStatus();
        public string Badge => State.GetBadgeClass();
        public bool CanRetake => State.CanRetake();
    }
}