using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CRS.Data;
using CRS.Models;
using CRS.Dtos;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using System.Security.Claims;

namespace CRS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CoursesController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public CoursesController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/Courses
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CoursesDto>>> GetCourses()
        {
            var courses = await _context.Courses
                .Include(c => c.Department)
                .Include(c => c.Building)
                .Include(c => c.Room)
                .Select(c => new CoursesDto
                {
                    Id = c.Id,
                    Title = c.Title,
                    Description = c.Description,
                    DepartmentName = c.Department.Name,
                    DepartmentId = c.DepartmentId,
                    RoomId = c.RoomId,
                    RoomName = c.Room.Name,
                    BuildingName = c.Building.Name,
                    BuildingId = c.BuildingId,
                    Capacity = c.Capacity,
                    Date = c.Date,
                    CourseStates = c.CourseStates
                })
                .ToListAsync();

            return Ok(courses);
        }




        // Get: api/Courses/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpGet("{id}")]
        public async Task<ActionResult<CoursesDto>> GetCourse(int id)
        {
            var course = await _context.Courses
                .Include(c => c.Department)
                .Include(c => c.Building)
                .Include(c => c.Room)
                .Where(c => c.Id == id)
                .Select(c => new CoursesDto
                {
                    Id = c.Id,
                    Title = c.Title,
                    Description = c.Description,

                    DepartmentId = c.DepartmentId,
                    DepartmentName = c.Department.Name,

                    RoomId = c.RoomId,
                    RoomName = c.Room.Name,

                    BuildingId = c.BuildingId,
                    BuildingName = c.Building.Name,

                    Capacity = c.Capacity,
                    Date = c.Date,
                    CourseStates = c.CourseStates
                })

                .FirstOrDefaultAsync();

            if (course == null)
            {
                return NotFound();
            }

            return Ok(course);
        }


        // POST: api/Courses

        [Authorize(Roles = "Admin")] // only Admin
        [HttpPost]
        public async Task<ActionResult<CoursesDto>> PostCourse(CoursesDto dto)
        {
            // Validate foreign keys
            if (!await _context.Departments.AnyAsync(d => d.DepartmentId == dto.DepartmentId))
                return BadRequest("Invalid DepartmentId");
            if (!await _context.Rooms.AnyAsync(r => r.RoomId == dto.RoomId))
                return BadRequest("Invalid RoomId");
            if (!await _context.Buildings.AnyAsync(b => b.BuildingId == dto.BuildingId))
                return BadRequest("Invalid BuildingId");

            var course = new Course
            {
                Title = dto.Title,
                Description = dto.Description,
                DepartmentId = dto.DepartmentId,
                RoomId = dto.RoomId,
                BuildingId = dto.BuildingId,
                Capacity = dto.Capacity,
                Date = dto.Date
            };

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            dto.Id = course.Id;
            dto.DepartmentName = (await _context.Departments.FindAsync(course.DepartmentId))?.Name;
            dto.RoomName = (await _context.Rooms.FindAsync(course.RoomId))?.Name;
            dto.BuildingName = (await _context.Buildings.FindAsync(course.BuildingId))?.Name;

            return CreatedAtAction(nameof(GetCourse), new { id = course.Id }, dto);
        }

        // DELETE: api/Courses/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        [Authorize]
        [HttpDelete("withdraw/{courseId}")]
        public async Task<IActionResult> WithdrawFromCourse(int courseId)
        {
            var userIdStr = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr))
                return Unauthorized("Invalid token.");

            int userId = int.Parse(userIdStr);

            var reservation = await _context.Reservations.FirstOrDefaultAsync(r => r.UserId == userId && r.CourseId == courseId);
            if (reservation == null)
                return NotFound("No reservation found for this course.");

            _context.Reservations.Remove(reservation);
            await _context.SaveChangesAsync();

            return Ok("Withdrawn from course successfully.");
        }

        [Authorize]
        [HttpDelete("withdraw/by-title/{courseTitle}")]
        public async Task<IActionResult> WithdrawFromCourseByTitle(string courseTitle)
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





        // put : api/Courses

        [Authorize(Roles = "Admin")] // only Admin
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCourse(int id, CoursesDto dto)
        {
            if (id != dto.Id)
            {
                return BadRequest("ID mismatch.");
            }

            // Validate foreign keys
            if (!await _context.Departments.AnyAsync(d => d.DepartmentId == dto.DepartmentId))
                return BadRequest("Invalid DepartmentId");
            if (!await _context.Rooms.AnyAsync(r => r.RoomId == dto.RoomId))
                return BadRequest("Invalid RoomId");
            if (!await _context.Buildings.AnyAsync(b => b.BuildingId == dto.BuildingId))
                return BadRequest("Invalid BuildingId");

            var course = await _context.Courses.FindAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            // Update only the fields that are allowed to change
            course.Title = dto.Title;
            course.Description = dto.Description;
            course.DepartmentId = dto.DepartmentId;
            course.RoomId = dto.RoomId;
            course.BuildingId = dto.BuildingId;
            course.Capacity = dto.Capacity;
            course.Date = dto.Date;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Courses.Any(c => c.Id == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok(dto);
        }

        private bool CourseExists(int id)
        {
            return _context.Courses.Any(e => e.Id == id);
        }
    }
}
