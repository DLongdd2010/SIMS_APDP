using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIMS_APDP.Data;
using SIMS_APDP.Models;
using SIMS_APDP.Services;
using System.Security.Claims;

namespace SIMS_APDP.Controllers
{
    public class LoginController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserService _userService;

        public LoginController(ApplicationDbContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        // GET: /Login
        public IActionResult Index()
        {
            return View();
        }

        // POST: /Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string Email, string Password, bool RememberMe = false)
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                TempData["Error"] = "Vui lòng nhập email và mật khẩu.";
                return RedirectToAction("Index");
            }

            // Hash password using UserService (consistent hashing method)
            string hashed = _userService.HashPassword(Password);

            // Find user by email
            var user = await _context.Users.Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == Email);

            if (user == null || user.Password != hashed)
            {
                TempData["Error"] = "Email hoặc mật khẩu không đúng.";
                return RedirectToAction("Index");
            }

            // Create claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username ?? user.Email),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim("UserId", user.UserId.ToString()),
                new Claim("RoleId", user.RoleId.ToString())
            };

            if (user.Role != null)
            {
                claims.Add(new Claim(ClaimTypes.Role, user.Role.RoleName));
            }

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            var authProperties = new AuthenticationProperties();

            if (RememberMe)
            {
                authProperties.IsPersistent = true;
                // Set expiration explicitly when persistent (match cookie options)
                authProperties.ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7);
            }
            else
            {
                // Non-persistent: cookie will be a session cookie and removed when browser closes
                authProperties.IsPersistent = false;
            }

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);

            // Redirect based on RoleId
            switch (user.RoleId)
            {
                case 1:
                    return RedirectToAction("Index", "Admin");
                case 2:
                    return RedirectToAction("Index", "Teacher");
                case 3:
                default:
                    return RedirectToAction("Index", "Student");
            }
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Login");
        }
    }
}
