using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIMS_APDP.Data;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SIMS_APDP.Controllers
{
    [Authorize] // bắt buộc phải login mới vào được
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Profile()
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                return RedirectToAction("Index", "Login");
            }

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                return RedirectToAction("Logout", "Login");
            }

            return View("~/Views/Account/Profile.cshtml", user);
        }

        // GET: /Account/ChangePassword
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View("~/Views/Account/ChangePassword.cshtml");
        }

        // POST: /Account/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(currentPassword) ||
                string.IsNullOrWhiteSpace(newPassword) ||
                string.IsNullOrWhiteSpace(confirmPassword))
            {
                TempData["Error"] = "Vui lòng điền đầy đủ các trường.";
                return View("~/Views/Account/ChangePassword.cshtml");
            }

            if (newPassword != confirmPassword)
            {
                TempData["Error"] = "Mật khẩu mới và xác nhận không khớp!";
                return View("~/Views/Account/ChangePassword.cshtml");
            }

            if (newPassword.Length < 6)
            {
                TempData["Error"] = "Mật khẩu mới phải có ít nhất 6 ký tự!";
                return View("~/Views/Account/ChangePassword.cshtml");
            }

            // Lấy UserId từ claim
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                TempData["Error"] = "Không xác định được tài khoản!";
                return RedirectToAction("Logout", "Login");
            }

            // Tìm user trong DB
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
            {
                TempData["Error"] = "Tài khoản không tồn tại!";
                return RedirectToAction("Logout", "Login");
            }

            // Kiểm tra mật khẩu hiện tại (so sánh hash)
            string currentHashed = BitConverter.ToString(SHA256.HashData(Encoding.UTF8.GetBytes(currentPassword)))
                                             .Replace("-", "");

            if (user.Password != currentHashed)
            {
                TempData["Error"] = "Mật khẩu hiện tại không đúng!";
                return View("~/Views/Account/ChangePassword.cshtml");
            }

            // Hash mật khẩu mới
            string newHashed = BitConverter.ToString(SHA256.HashData(Encoding.UTF8.GetBytes(newPassword)))
                                           .Replace("-", "");

            // CẬP NHẬT THẬT VÀO DATABASE
            user.Password = newHashed;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đổi mật khẩu thành công! Vui lòng đăng nhập lại.";

            // TỐT NHẤT: Đăng xuất luôn để tránh dùng session cũ
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Index", "Login");
        }
    }
}