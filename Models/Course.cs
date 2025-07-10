using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CRS.Models
{
    public class Course
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

            public string Title { get; set; }
            public string Description { get; set; }
        public int DepartmentId { get; set; }

        [ForeignKey("DepartmentId")]
        public Department Department { get; set; }

        public int BuildingId { get; set; }
        [ForeignKey("BuildingId")]
        public DotNET.Models.Building Building { get; set; }
        public int RoomId { get; set; }


        [ForeignKey("RoomId")]
        public Room Room { get; set; }

        public DateTime Date { get; set; }

          
        }

    }

