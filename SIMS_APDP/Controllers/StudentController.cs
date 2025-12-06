using Microsoft.AspNetCore.Mvc;

namespace SIMS_APDP.Controllers
{
    public class StudentController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
