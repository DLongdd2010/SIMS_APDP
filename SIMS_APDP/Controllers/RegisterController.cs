using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIMS_APDP.Data;
using SIMS_APDP.Models;
using System.Security.Cryptography;
using System.Text;

namespace SIMS_APDP.Controllers
{
    public class RegisterController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RegisterController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View(new User());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(User model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            bool usernameExists = await _context.Users
                .AnyAsync(u => u.Username == model.Username);

            if (usernameExists)
            {
                ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại!");
                return View(model);
            }

            bool emailExists = await _context.Users
                .AnyAsync(u => u.Email == model.Email);

            if (emailExists)
            {
                ModelState.AddModelError("Email", "Email này đã được đăng ký!");
                return View(model);
            }

            model.Password = BitConverter
                .ToString(SHA256.HashData(Encoding.UTF8.GetBytes(model.Password)))
                .Replace("-", "");

            model.RoleId = 3;

            _context.Users.Add(model);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đăng ký thành công! Bạn đã là Sinh viên SIMS.";
            return RedirectToAction("Index", "Login");
        }
    }
}
