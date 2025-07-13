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
    public class ReservationsController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public ReservationsController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/Reservations
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReservationDTo>>> GetReservations()
        {
            var reservations = await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Course)
                .Select(r => new ReservationDTo
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    UserName = r.User.Name,
                    CourseId = r.CourseId,
                    CourseTitle = r.Course.Title,
                    Status = r.Status,
                    RequestDate = r.RequestDate
                })
                .ToListAsync();

            return Ok(reservations);
        }


        // GET: api/Reservations/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ReservationDTo>> GetReservation(int id)
        {
            var reservation = await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Course)
                .Where(r => r.Id == id)
                .Select(r => new ReservationDTo
                {
                    Id = r.Id,
                    UserId = r.UserId,
                    UserName = r.User.Name,         // Assumes User has a Name property
                    CourseId = r.CourseId,
                    CourseTitle = r.Course.Title,   // Assumes Course has a Title property
                    Status = r.Status ,  // Converts enum to readable string
                    RequestDate = r.RequestDate
                })
                .FirstOrDefaultAsync();

            if (reservation == null)
            {
                return NotFound();
            }

            return Ok(reservation);
        }


        // PUT: api/Reservations/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutReservation(int id, ReservationDTo updated)
        {
            var reservation = await _context.Reservations.FindAsync(id);

            if (reservation == null)
            {
                return NotFound();
            }

            reservation.CourseId = updated.CourseId;
            reservation.Status =updated.Status;
            reservation.RequestDate = updated.RequestDate;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ReservationExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok(new
            {
                Message = "Reservation updated successfully",
                reservation.Id,
                reservation.CourseId,
                reservation.Status,
                reservation.RequestDate
            });
        }

        // POST: api/Reservations
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<ReservationDTo>> ReserveCourse(ReservationDTo dto)
        {
            var course = await _context.Courses.FindAsync(dto.CourseId);

            if (course == null || course.CourseStates != CourseStates.available)
                return BadRequest("Course is not available.");

            if (course.Capacity <= 0)
                return BadRequest("Course is full.");

            var existing = await _context.Reservations
                .FirstOrDefaultAsync(r => r.UserId == dto.UserId && r.CourseId == dto.CourseId);

            if (existing != null)
                return BadRequest("You have already reserved this course.");

            course.Capacity--;

            var reservation = new Reservation
            {
                UserId = dto.UserId,
                CourseId = dto.CourseId,
                RequestDate = DateTime.Now,
                Status = ReservationStatus.Pending,
                
            };
            course.CourseStates = CourseStates.pending;


            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            return Ok();
        }



        // DELETE: api/Reservations/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReservation(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
            {
                return NotFound();
            }

            _context.Reservations.Remove(reservation);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ReservationExists(int id)
        {
            return _context.Reservations.Any(e => e.Id == id);
        }
    }
}
