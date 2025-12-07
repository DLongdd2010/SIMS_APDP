using SIMS_APDP.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SIMS_APDP.Services
{
    /// <summary>
    /// User Service Interface - qu?n lý Users (CRUD, validation, security)
    /// </summary>
    public interface IUserService
    {
        // Read operations
        IEnumerable<User> GetAllUsers();
        User GetUserById(int userId);

        // Write operations
        void AddUser(User user);
        void UpdateUser(User user);
        void DeleteUser(int userId);

        // Check operations
        bool UserExists(int userId);
        bool UsernameExists(string username);
        bool EmailExists(string email);

        // Security operations
        string HashPassword(string password);
    }
}
