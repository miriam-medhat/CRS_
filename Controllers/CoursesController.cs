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
                    RoomName = c.Room.Name,
                    BuildingName = c.Building.Name,
                    Date = c.Date
                })
                .ToListAsync();

            return Ok(courses);
        }


      

        // PUT: api/Courses/5
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
                    DepartmentName = c.Department.Name,
                    RoomName = c.Room.Name,
                    BuildingName = c.Building.Name,
                    Date = c.Date
                })
                .FirstOrDefaultAsync();

            if (course == null)
            {
                return NotFound();
            }

            return Ok(course);
        }


        // POST: api/Courses
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Course>> PostCourse(Course course)
        {
            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCourse), new { id = course.Id }, new CoursesDto
            {
                Id = course.Id,
                Title = course.Title,
                Description = course.Description,
                DepartmentName = (await _context.Departments.FindAsync(course.DepartmentId))?.Name,
                RoomName = (await _context.Rooms.FindAsync(course.RoomId))?.Name,
                BuildingName = (await _context.Buildings.FindAsync(course.BuildingId))?.Name,
                Date = course.Date
            });
            ;
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

        private bool CourseExists(int id)
        {
            return _context.Courses.Any(e => e.Id == id);
        }
    }
}
