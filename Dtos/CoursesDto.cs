using CRS.Models;

public class CoursesDto
{
    public int Id { get; set; }  // optional on POST

    public string Title { get; set; }
    public string Description { get; set; }

    // IDs needed for input
    public int DepartmentId { get; set; }
    public int RoomId { get; set; }
    public int BuildingId { get; set; }

    // Names are only for output
    public string? DepartmentName { get; set; }
    public string? RoomName { get; set; }
    public string? BuildingName { get; set; }

    public int Capacity { get; set; }
    public DateTime Date { get; set; }
    public CourseStates CourseStates { get; set; }
}
