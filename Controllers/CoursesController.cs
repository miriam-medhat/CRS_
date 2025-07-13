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
                    DepartmentId=c.DepartmentId,
                    RoomId=c.RoomId,
                    RoomName = c.Room.Name,
                    BuildingName = c.Building.Name,
                    Date = c.Date,
                    CourseStates=c.CourseStates
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
                    CourseStates=c.CourseStates
                })

                .FirstOrDefaultAsync();

            if (course == null)
            {
                return NotFound();
            }

            return Ok(course);
        }

      
        // POST: api/Courses
        [HttpPost]
        public async Task<ActionResult<CoursesDto>> PostCourse(CoursesDto dto)
        {
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





        // put : api/Courses
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCourse(int id, CoursesDto dto)
        {
            if (id != dto.Id)
            {
                return BadRequest("ID mismatch.");
            }

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
