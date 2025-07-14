using CRS.Data;
using CRS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReservationController : ControllerBase
{
    private readonly ApplicationDBContext _context;

    public ReservationController(ApplicationDBContext context)
    {
        _context = context;
    }

    [HttpPost("reserve")]
    public async Task<IActionResult> ReserveCourse([FromBody] ReservationDTO dto)
    {
        if (string.IsNullOrWhiteSpace(dto.CourseTitle))
            return BadRequest("Course title is required.");

        //  Get logged-in user ID from JWT
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr))
            return Unauthorized("Invalid token.");

        int userId = int.Parse(userIdStr);

        // Find course by title
        var course = await _context.Courses
      .FirstOrDefaultAsync(c => c.Title.ToLower() == dto.CourseTitle.ToLower());

        if (course == null)
            return NotFound("Course not found.");

        //  Check if already reserved
        bool alreadyReserved = await _context.Reservations.AnyAsync(r => r.UserId == userId && r.CourseId == course.Id);
        if (alreadyReserved)
            return BadRequest("You already reserved this course.");

        //  Create reservation
        var reservation = new Reservation
        {
            UserId = userId,
            CourseId = course.Id,
            Status = ReservationStatus.Pending,
            RequestDate = DateTime.UtcNow
        };

        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();

        return Ok("Reservation successful.");
    }


    [Authorize]
    [HttpGet("my-reservations")]
    public async Task<IActionResult> GetMyReservations()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr))
            return Unauthorized("Invalid token.");

        int userId = int.Parse(userIdStr);

        var reservations = await _context.Reservations
            .Where(r => r.UserId == userId &&
                       (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Accepted))
            .Include(r => r.Course)
            .ToListAsync();

        var result = reservations.Select(r => new
        {
            r.Id,
            r.Status,
            r.RequestDate,
            CourseTitle = r.Course.Title
        });

        return Ok(result);
    }

}
