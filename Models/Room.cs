using DotNET.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography.Xml;

namespace CRS.Models
{
    public class Room
    {
        [Key]
        public int RoomId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public int BuildingId { get; set; }

        //reference navigation
        [ForeignKey("BuildingId")]
        public Building Building { get; set; } //room belongs to one building

        //collection navigation
        public ICollection<Course> Courses { get; set; } //room has many courses
    }
}
