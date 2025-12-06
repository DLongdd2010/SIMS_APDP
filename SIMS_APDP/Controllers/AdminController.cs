using Microsoft.AspNetCore.Mvc;
using SIMS_APDP.Data;
using SIMS_APDP.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace SIMS_APDP.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Admin
        public IActionResult Index()
        {
            return View();
        }

        // GET: /Admin/UserManager
        public IActionResult UserManager()
        {
            var users = _context.Users
                .Include(u => u.Role)
                .ToList();

            return View(users);
        }

        // POST: /Admin/AddUser
        [HttpPost]
        public IActionResult AddUser(User user, string confirmPassword)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("UserManager");
            }

            if (user.Password != confirmPassword)
            {
                ModelState.AddModelError("", "Password confirmation does not match!");
                return RedirectToAction("UserManager");
            }

            if (_context.Users.Any(u => u.Username == user.Username))
            {
                ModelState.AddModelError("", "Username already exists!");
                return RedirectToAction("UserManager");
            }

            if (_context.Users.Any(u => u.Email == user.Email))
            {
                ModelState.AddModelError("", "Email already registered!");
                return RedirectToAction("UserManager");
            }

            user.Password = HashPassword(user.Password);
            _context.Users.Add(user);
            _context.SaveChanges();

            return RedirectToAction("UserManager");
        }

        // POST: /Admin/UpdateUser
        [HttpPost]
        public IActionResult UpdateUser(User user)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("UserManager");
            }

            var existingUser = _context.Users.Find(user.UserId);
            if (existingUser == null)
            {
                return NotFound();
            }

            existingUser.FullName = user.FullName;
            existingUser.Email = user.Email;
            existingUser.PhoneNumber = user.PhoneNumber;
            existingUser.Gender = user.Gender;
            existingUser.DateOfBirth = user.DateOfBirth;
            existingUser.Address = user.Address;
            existingUser.RoleId = user.RoleId;

            _context.SaveChanges();
            return RedirectToAction("UserManager");
        }

        // POST: /Admin/DeleteUser
        [HttpPost]
        public IActionResult DeleteUser(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            _context.SaveChanges();

            return Ok();
        }

        // GET: /Admin/CourseManager
        public IActionResult CourseManager()
        {
            return View();
        }

        // GET: /Admin/TimeTable
        public IActionResult TimeTable()
        {
            return View();
        }

        // Helper method to normalize role name
        public static string NormalizeRoleName(string roleName)
        {
            if (string.IsNullOrEmpty(roleName))
                return "Unknown";

            return roleName switch
            {
                "Admin" => "Admin",
                "Administrator" => "Admin",
                "Teacher" => "Teacher",
                "Student" => "Student",
                _ => roleName
            };
        }

        // Helper method to hash password
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
}
