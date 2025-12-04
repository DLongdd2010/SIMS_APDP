using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIMS_APDP.Models
{
    [Table("Role")]
    public class Role
    {
        [Key]
        public int RoleId { get; set; }
        [Required, MaxLength(20)]
        public string RoleName { get; set; }
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
