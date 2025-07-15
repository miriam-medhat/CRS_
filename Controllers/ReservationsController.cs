using CRS.Data;
using CRS.Dtos;
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
        bool alreadyReserved = await _context.Reservations.AnyAsync(
            r => r.UserId == userId && r.CourseId == course.Id && r.Status != ReservationStatus.Rejected
        );
        if (alreadyReserved)
            return BadRequest("You already reserved this course.");

        // Check course capacity
        int reservedCount = await _context.Reservations.CountAsync(r => r.CourseId == course.Id && (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Accepted));
        if (reservedCount >= course.Capacity)
            return BadRequest("Course is full. No more reservations allowed.");

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
            Status = r.Status.ToString(),
            r.RequestDate,
            CourseTitle = r.Course.Title
        });

        return Ok(result);
    }



    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<AdminReservationDto>>> GetAllReservations()
    {
        var reservations = await _context.Reservations
        .Include(r => r.User)
        .Include(r => r.Course)
        .Select(r => new AdminReservationDto
        {
            Id = r.Id,
            UserName = r.User.Name,
            CourseTitle = r.Course.Title,
            Status = r.Status.ToString()
        })
        .ToListAsync();

        return Ok(reservations);
    }


    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] ReservationStatusDto dto)
    {
        var reservation = await _context.Reservations.FindAsync(id);
        if (reservation == null) return NotFound();

        if (!Enum.TryParse<ReservationStatus>(dto.Status, true, out var newStatus))
            return BadRequest($"Invalid status value: {dto.Status}");

        // Only decrease capacity if status is being set to Accepted and wasn't already Accepted
        if (newStatus == ReservationStatus.Accepted && reservation.Status != ReservationStatus.Accepted)
        {
            var course = await _context.Courses.FindAsync(reservation.CourseId);
            if (course == null)
                return BadRequest("Course not found for this reservation.");

            if (course.Capacity <= 0)
                return BadRequest("No available capacity for this course.");

            course.Capacity -= 1;
            _context.Courses.Update(course); // <-- Ensure EF tracks the change
        }
        // If status is being set to Rejected and was previously Accepted, increase capacity
        else if (newStatus == ReservationStatus.Rejected && reservation.Status == ReservationStatus.Accepted)
        {
            var course = await _context.Courses.FindAsync(reservation.CourseId);
            if (course != null)
            {
                course.Capacity += 1;
                _context.Courses.Update(course);
            }
        }

        reservation.Status = newStatus;
        await _context.SaveChangesAsync();
        return NoContent();
    }


    [HttpDelete("withdraw/{id}")]
    public async Task<IActionResult> WithdrawReservation(int id)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr))
            return Unauthorized("Invalid token.");

        int userId = int.Parse(userIdStr);

        var reservation = await _context.Reservations.FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);
        if (reservation == null)
            return NotFound("Reservation not found or you do not have permission to withdraw it.");

        _context.Reservations.Remove(reservation);
        await _context.SaveChangesAsync();

        return Ok("Reservation withdrawn successfully.");
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("admin-withdraw/{id}")]
    public async Task<IActionResult> AdminWithdrawReservation(int id)
    {
        var reservation = await _context.Reservations.FirstOrDefaultAsync(r => r.Id == id);
        if (reservation == null)
            return NotFound("Reservation not found.");

        _context.Reservations.Remove(reservation);
        await _context.SaveChangesAsync();

        return Ok("Reservation withdrawn by admin successfully.");
    }

    [HttpDelete("withdraw/by-title/{courseTitle}")]
    public async Task<IActionResult> WithdrawReservationByTitle(string courseTitle)
    {
        var userIdStr = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr))
            return Unauthorized("Invalid token.");

        int userId = int.Parse(userIdStr);

        var course = await _context.Courses.FirstOrDefaultAsync(c => c.Title.ToLower() == courseTitle.ToLower());
        if (course == null)
            return NotFound("Course not found.");

        var reservation = await _context.Reservations.FirstOrDefaultAsync(r => r.UserId == userId && r.CourseId == course.Id);
        if (reservation == null)
            return NotFound("No reservation found for this course.");

        _context.Reservations.Remove(reservation);
        await _context.SaveChangesAsync();

        return Ok("Withdrawn from course successfully by title.");
    }





}
