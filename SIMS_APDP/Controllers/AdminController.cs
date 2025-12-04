using Microsoft.AspNetCore.Mvc;

namespace SIMS_APDP.Controllers
{
    public class AdminController : Controller
    {
        // GET: /Admin
        public IActionResult Index()
        {
            return View();
        }
    }
}
