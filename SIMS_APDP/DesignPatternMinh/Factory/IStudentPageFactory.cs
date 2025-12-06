// DesignPatternMinh/Factory/IStudentPageFactory.cs
namespace SIMS_APDP.DesignPatternMinh.Factory
{
    public interface IStudentPage
    {
        string ViewName { get; }
        string Title { get; }
    }

    public interface IStudentPageFactory
    {
        IStudentPage CreatePage(string pageType);
    }
}