using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIMS_APDP.Models
{
    [Table("User")]
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        public int RoleId { get; set; }

        [Required, StringLength(50)]
        public string Username { get; set; }

        [Required, EmailAddress, StringLength(100)]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [Required, StringLength(100)]        
        public string FullName { get; set; }

        [Required, StringLength(255)]        
        public string Address { get; set; }

        public string? Gender { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }

        [Required]                           
        public DateTime DateOfBirth { get; set; }

        // Navigation (KHÔNG được NotMapped)
        [ForeignKey("RoleId")]
        public Role? Role { get; set; }

        public ICollection<StudentCourse> StudentCourses { get; set; } = new List<StudentCourse>();
    }
}
