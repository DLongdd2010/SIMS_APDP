using Microsoft.AspNetCore.Mvc;

namespace SIMS_APDP.DesignPatternMinh.Factory
{
    public interface IDashboardFactory
    {
        IActionResult CreateDashboard(Controller controller, int userId);
    }
}