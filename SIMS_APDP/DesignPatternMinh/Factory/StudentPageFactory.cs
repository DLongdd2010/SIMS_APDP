// DesignPatternMinh/Factory/StudentPageFactory.cs
namespace SIMS_APDP.DesignPatternMinh.Factory
{
    public class StudentPageFactory : IStudentPageFactory
    {
        public IStudentPage CreatePage(string pageType)
        {
            return pageType switch
            {
                "TableTime" => new TableTimePage(),
                "Transcript" => new TranscriptPage(),
                "Feedback" => new FeedbackPage(),
                _ => new DashboardPage()
            };
        }
    }

    // Các class con
    public class TableTimePage : IStudentPage { public string ViewName => "TableTime"; public string Title => "Thời khóa biểu"; }
    public class TranscriptPage : IStudentPage { public string ViewName => "Transcript"; public string Title => "Bảng điểm"; }
    public class FeedbackPage : IStudentPage { public string ViewName => "Feedback"; public string Title => "Phản hồi giảng viên"; }
    public class DashboardPage : IStudentPage { public string ViewName => "Index"; public string Title => "Trang chủ"; }
}