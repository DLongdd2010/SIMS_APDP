using Microsoft.AspNetCore.Mvc;

namespace SIMS_APDP.Controllers
{
    public class StudentController : Controller
    {
        // GET: /Student
        public IActionResult Index()
        {
            return View();
        }
    }
}
