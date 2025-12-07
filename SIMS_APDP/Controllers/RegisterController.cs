using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIMS_APDP.Data;
using SIMS_APDP.Models;
using SIMS_APDP.Services;

namespace SIMS_APDP.Controllers
{
    public class RegisterController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserService _userService;

        public RegisterController(ApplicationDbContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
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
            // Remove validation for auto-assigned fields
            ModelState.Remove("RoleId");
            ModelState.Remove("Role");
            ModelState.Remove("StudentCourses");

            if (!ModelState.IsValid)
            {
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        Console.WriteLine($"VALIDATION ERROR: {error.ErrorMessage} / {error.Exception?.Message}");
                    }
                }
                return View(model);
            }

            // Check if username already exists
            if (_userService.UsernameExists(model.Username))
            {
                ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại!");
                return View(model);
            }

            // Check if email already exists
            if (_userService.EmailExists(model.Email))
            {
                ModelState.AddModelError("Email", "Email này đã được đăng ký!");
                return View(model);
            }

            // Hash password using UserService (consistent hashing method)
            model.Password = _userService.HashPassword(model.Password);

            // Assign student role
            model.RoleId = 3;

            // Save to database
            _context.Users.Add(model);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đăng ký thành công! Bạn đã là Sinh viên SIMS.";

            return RedirectToAction("Index", "Login");
        }
    }
}
