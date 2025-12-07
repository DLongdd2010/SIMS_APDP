using Microsoft.AspNetCore.Mvc;
using SIMS_APDP.Controllers;

namespace SIMS_APDP.DesignPatternMinh.Factory
{
    public class StudentDashboardFactory : IDashboardFactory
    {
        public IActionResult CreateDashboard(Controller controller, int userId)
        {
            return controller.RedirectToAction("Index", "Student");
        }
    }
}