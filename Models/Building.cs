using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DotNET.Models
{
    public class Building
    {
        [Key]
        public int BuildingId { get; set; }
        public string Name { get; set; }
      
    }
}
