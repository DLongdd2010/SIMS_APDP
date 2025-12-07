using Microsoft.AspNetCore.Mvc;

namespace SIMS_APDP.DesignPatternMinh.Factory
{
    public static class DashboardFactory
    {
        // Role IDs: 2 = Teacher, 3 = Student (Based on Database/Seeding)
        public static IDashboardFactory GetFactory(int roleId) => roleId switch
        {
            3 => new StudentDashboardFactory(),
            2 => new TeacherDashboardFactory(),
            _ => new StudentDashboardFactory() // Default to Student if unknown
        };
    }
}