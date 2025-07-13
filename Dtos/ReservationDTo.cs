using CRS.Models;

namespace CRS.Dtos
{
    public class ReservationDTo
    {
      
            public int Id { get; set; }

            public int UserId { get; set; }
            public string UserName { get; set; }  // Assuming User has a Name property

            public int CourseId { get; set; }
            public string CourseTitle { get; set; }  // Assuming Course has a Title

            public ReservationStatus Status{ get; set; }  // Use string if enum for readability

            public DateTime RequestDate { get; set; }
        }

    }
