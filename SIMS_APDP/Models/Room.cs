using System.ComponentModel.DataAnnotations;

namespace SIMS_APDP.Models
{
    public class Room
    {
        [Key]
        public int RoomId { get; set; }
        [Required]
        public string RoomName { get; set; }
    }
}
