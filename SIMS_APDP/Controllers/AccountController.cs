using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIMS_APDP.Data;
using SIMS_APDP.Services;
using System.Security.Claims;

namespace SIMS_APDP.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserService _userService;

        public AccountController(ApplicationDbContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
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
                TempData["Error"] = "Please fill in all fields.";
                return View("~/Views/Account/ChangePassword.cshtml");
            }

            if (newPassword != confirmPassword)
            {
                TempData["Error"] = "New password and confirmation do not match!";
                return View("~/Views/Account/ChangePassword.cshtml");
            }

            if (newPassword.Length < 6)
            {
                TempData["Error"] = "New password must be at least 6 characters!";
                return View("~/Views/Account/ChangePassword.cshtml");
            }

            // Lấy UserId từ claim
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
            {
                TempData["Error"] = "Account not identified!";
                return RedirectToAction("Logout", "Login");
            }

            // Tìm user trong DB
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
            {
                TempData["Error"] = "Account does not exist!";
                return RedirectToAction("Logout", "Login");
            }

            // Kiểm tra mật khẩu hiện tại (sử dụng IUserService để hash nhất quán)
            string currentHashed = _userService.HashPassword(currentPassword);

            if (user.Password != currentHashed)
            {
                TempData["Error"] = "Current password is incorrect!";
                return View("~/Views/Account/ChangePassword.cshtml");
            }

            // Hash mật khẩu mới (sử dụng IUserService để hash nhất quán)
            string newHashed = _userService.HashPassword(newPassword);

            // CẬP NHẬT THẬT VÀO DATABASE
            user.Password = newHashed;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Password changed successfully! Please log in again.";

            // TỐT NHẤT: Đăng xuất luôn để tránh dùng session cũ
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Index", "Login");
        }
    }
}