using Microsoft.AspNetCore.Mvc;

namespace SIMS_APDP.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
<<<<<<< HEAD
=======

        // POST: /Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string Email, string Password, bool RememberMe = false)
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                TempData["Error"] = "Vui lòng nhập email và mật khẩu.";
                return View();
            }

            //// Hash password using same approach as registration
            string hashed = BitConverter
                .ToString(SHA256.HashData(Encoding.UTF8.GetBytes(Password)))
                .Replace("-", "");

            // Find user by email
            var user = await _context.Users.Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == Email);

            if (user == null || user.Password != hashed)
            {
                TempData["Error"] = "Email hoặc mật khẩu không đúng.";
                return View();
            }

            // Create claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username ?? user.Email),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim("RoleId", user.RoleId.ToString())
            };

            if (user.Role != null)
            {
                claims.Add(new Claim(ClaimTypes.Role, user.Role.RoleName));
            }

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            var authProperties = new AuthenticationProperties()
            {
                IsPersistent = RememberMe,                                   
                ExpiresUtc = RememberMe ? DateTimeOffset.UtcNow.AddDays(7) : null,
                IssuedUtc = DateTimeOffset.UtcNow
            };

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

        [HttpGet("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Login");
        }
>>>>>>> 774f573 (minh up role student)
    }
}
