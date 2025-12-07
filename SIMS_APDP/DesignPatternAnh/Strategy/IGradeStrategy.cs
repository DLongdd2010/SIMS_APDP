namespace SIMS_APDP.DesignPatternAnh.Strategy
{
    public interface IGradeStrategy
    {
        decimal CalculateGrade(decimal[] scores); // Ví dụ: tính trung bình hoặc khác
        string GetRanking(decimal grade); // Xếp hạng dựa trên điểm
    }
}