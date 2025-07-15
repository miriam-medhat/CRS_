using CRS.Data;
using CRS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CRS.Controllers
{
    public class DashboardUserController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public DashboardUserController(ApplicationDBContext context)
        {
            _context = context;
        }

        [HttpGet("courses")]
        public async Task<IActionResult> GetAvailableCourses()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr))
                return Unauthorized("Invalid token.");

            int userId = int.Parse(userIdStr);

            // 1. Get courseIds the user already reserved
            var reservedCourseIds = await _context.Reservations
                .Where(r => r.UserId == userId)
                .Select(r => r.CourseId)
                .ToListAsync();

            // 2. Get available courses NOT in reserved list
            var availableCourses = await _context.Courses
                .Where(c => c.CourseStates == CourseStates.Available && !reservedCourseIds.Contains(c.Id))
                .Include(c => c.Department)
                .Include(c => c.Building)
                  .Include(c => c.Room)
                .ToListAsync();

            // 3. Return simplified list
            var result = availableCourses.Select(c => new
            {
                c.Id,
                c.Title,
                c.Description,
                DepartmentName = c.Department.Name,
                BuildingName = c.Building.Name,
                RoomName = c.Room.Name,
                c.Date,
                c.Capacity
            });

            return Ok(result);
        }
    }
}