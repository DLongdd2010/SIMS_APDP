using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIMS_APDP.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }
        [Required]
        public string Username { get; set; }
        [Required, EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string FullName { get; set; }
        [Required]
        public string Address { get; set; }
        [Required]
        public string? Gender { get; set; }
        [Required, Phone]
        public string? PhoneNumber { get; set; }
        [Required]
        public DateTime DateOfBirth { get; set; } = DateTime.UtcNow;
        [ForeignKey("RoleId")]
        public Role Role { get; set; }
    }
}
