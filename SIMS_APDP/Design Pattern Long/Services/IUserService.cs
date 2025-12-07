using SIMS_APDP.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SIMS_APDP.Services
{
    public interface IUserService
    {
        IEnumerable<User> GetAllUsers();
        User GetUserById(int userId);
        void AddUser(User user);
        void UpdateUser(User user);
        void DeleteUser(int userId);
        bool UserExists(int userId);
        bool UsernameExists(string username);
        bool EmailExists(string email);
        string HashPassword(string password);
    }
}
