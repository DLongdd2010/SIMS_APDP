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

        // GET: /Register
        public IActionResult Index()
        {
            return View(new User());
        }

        // POST: /Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(User model)
        {
            // --- BƯỚC 1: KIỂM TRA MODEL TỪ HTML ---
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // --- BƯỚC 2: KIỂM TRA USERNAME ĐÃ TỒN TẠI ---
            bool usernameExists = await _context.Users
                .AnyAsync(u => u.Username == model.Username);

            if (usernameExists)
            {
                ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại!");
                return View(model);
            }

            // --- BƯỚC 3: KIỂM TRA EMAIL ĐÃ TỒN TẠI ---
            bool emailExists = await _context.Users
                .AnyAsync(u => u.Email == model.Email);

            if (emailExists)
            {
                ModelState.AddModelError("Email", "Email này đã được đăng ký!");
                return View(model);
            }

            // --- BƯỚC 4: MÃ HÓA MẬT KHẨU ---
            model.Password = BitConverter
                .ToString(SHA256.HashData(Encoding.UTF8.GetBytes(model.Password)))
                .Replace("-", "");

            // --- BƯỚC 5: GÁN ROLE SINH VIÊN ---
            model.RoleId = 3;

            // --- BƯỚC 6: LƯU DB ---
            _context.Users.Add(model);
            await _context.SaveChangesAsync();

            // --- BƯỚC 7: BÁO THÀNH CÔNG ---
            TempData["Success"] = "Đăng ký thành công! Bạn đã là Sinh viên SIMS.";

            // --- BƯỚC 8: CHUYỂN TRANG ---
            return RedirectToAction("Index", "Login");
        }
    }
}
