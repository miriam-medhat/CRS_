using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRS.Models
{
    public class User
        
    {
        [Key]
      
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        [EmailAddress] //for email validation
        public string Email { get; set; }

        [Required]
         public string Password { get; set; }

        public RolesEnum Roles { get; set; }
        public ICollection<Reservation> Reservations { get; set; } //user has many reservations
    }
}
