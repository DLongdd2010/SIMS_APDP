namespace SIMS_APDP.DesignPatternAnh.Strategy
{
    public class AverageGradeStrategy : IGradeStrategy
    {
        public decimal CalculateGrade(decimal[] scores)
        {
            return scores.Average();
        }

        public string GetRanking(decimal grade)
        {
            if (grade >= 9) return "Xuất Sắc";
            if (grade >= 7) return "Giỏi";
            return "Trung Bình";
        }
    }
}