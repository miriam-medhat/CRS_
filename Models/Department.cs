using System.ComponentModel.DataAnnotations;

namespace CRS.Models
{
    public class Department
    {
        [Key]
        public int DepartmentId { get; set; }

        [Required]
        public string Name { get; set; }

        public ICollection<Course> Courses { get; set; } //department has many courses

    }
}
