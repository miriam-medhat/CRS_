using System.ComponentModel.DataAnnotations;

namespace CRS.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        [EmailAddress] //for email validation
        public string Email { get; set; }

        public ICollection<Reservation> Reservations { get; set; } //user has many reservations
    }
}
