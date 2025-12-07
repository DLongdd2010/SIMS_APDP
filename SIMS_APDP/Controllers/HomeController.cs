using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SIMS_APDP.Models;
using System.Diagnostics;

namespace SIMS_APDP.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        //Test database connection
        private readonly IConfiguration _configuration;

        public HomeController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("test-db")]
        public async Task<IActionResult> TestDb()
        {
            string connString = _configuration.GetConnectionString("DefaultConnection");

            try
            {
                using (var conn = new SqlConnection(connString))
                {
                    await conn.OpenAsync();
                    return Ok("Kết nối SQL thành công!");
                }
            }
            catch (Exception ex)
            {
                return BadRequest("Lỗi kết nối SQL: " + ex.Message);
            }
        }
    }
}
