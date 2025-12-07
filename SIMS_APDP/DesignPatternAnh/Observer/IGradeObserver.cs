namespace SIMS_APDP.DesignPatternAnh.Observer
{
    public interface IGradeObserver
    {
        void Update(int userId, decimal newGrade); // Notify khi grade thay đổi
    }
}