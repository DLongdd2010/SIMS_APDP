using SIMS_APDP.Data;
using SIMS_APDP.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace SIMS_APDP.Services
{
    /// <summary>
    /// User Service Implementation - handles all user operations with database
    /// </summary>
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;

        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Get all users with their roles
        public IEnumerable<User> GetAllUsers()
        {
            return _context.Users.Include(u => u.Role).ToList();
        }

        // Get single user by ID with role included
        public User GetUserById(int userId)
        {
            return _context.Users.Include(u => u.Role).FirstOrDefault(u => u.UserId == userId);
        }

        // Add new user to database
        public void AddUser(User user)
        {
            _context.Users.Add(user);
            _context.SaveChanges();
        }

        // Update existing user
        public void UpdateUser(User user)
        {
            _context.Users.Update(user);
            _context.SaveChanges();
        }

        // Delete user by ID
        public void DeleteUser(int userId)
        {
            var user = _context.Users.Find(userId);
            if (user != null)
            {
                _context.Users.Remove(user);
                _context.SaveChanges();
            }
        }

        // Check if user exists
        public bool UserExists(int userId)
        {
            return _context.Users.Any(u => u.UserId == userId);
        }

        // Check if username is already used
        public bool UsernameExists(string username)
        {
            return _context.Users.Any(u => u.Username == username);
        }

        // Check if email is already registered
        public bool EmailExists(string email)
        {
            return _context.Users.Any(u => u.Email == email);
        }

        // Hash password using SHA256
        public string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
}
