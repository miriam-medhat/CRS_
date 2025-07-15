using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CRS.Data;
using CRS.Models;
using Microsoft.AspNetCore.Authorization;

namespace CRS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomsController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public RoomsController(ApplicationDBContext context)
        {
            _context = context;
        }

        //GET: api/Rooms
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Room>>> GetRooms()
        {
            return await _context.Rooms.ToListAsync();
        }

        //// GET: api/Rooms/5
        //[HttpGet("{id}")]
        //public async Task<ActionResult<Room>> GetRoom(int id)
        //{
        //    var room = await _context.Rooms.FindAsync(id);

        //    if (room == null)
        //    {
        //        return NotFound();
        //    }

        //    return room;
        //}
        /// <summary>
        /// /////////////////////////////////////////////
        /// </summary>

        /// <returns></returns>
        //// For a single room (GetRoom)
        //[HttpGet("{id}")]
        [HttpGet("{id}")]
        public async Task<ActionResult<Room>> GetRoom(int id)
        {
            var room = await _context.Rooms
                .Include(r => r.Building)  // Load Building
                .Include(r => r.Courses)   // Load Courses
                .FirstOrDefaultAsync(r => r.RoomId == id);

            if (room == null)
            {
                return NotFound();
            }

            return room;
        }

        // For all rooms (GetRooms)
        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<Room>>> GetRooms()
        //{
        //    return await _context.Rooms
        //        .Include(r => r.Building)
        //        .Include(r => r.Courses)
        //        .ToListAsync();
        //}

        // PUT: api/Rooms/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRoom(int id, Room room)
        {
            if (id != room.RoomId)
            {
                return BadRequest();
            }

            // Ensure only buildingId is used
            room.Building = null;

            _context.Entry(room).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RoomExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            catch (DbUpdateException ex)
            {
                return BadRequest("Database update failed: " + (ex.InnerException?.Message ?? ex.Message));
            }

            return NoContent();
        }

        // POST: api/Rooms
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<Room>> PostRoom(Room room)
        {
            // Ensure only buildingId is used
            room.Building = null;

            _context.Rooms.Add(room);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return BadRequest("Database update failed: " + (ex.InnerException?.Message ?? ex.Message));
            }

            return CreatedAtAction("GetRoom", new { id = room.RoomId }, room);
        }

        // DELETE: api/Rooms/5
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoom(int id)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null)
            {
                return NotFound();
            }

            // Check if any courses reference this room
            bool isRoomInUse = await _context.Courses.AnyAsync(c => c.RoomId == id);
            if (isRoomInUse)
            {
                return BadRequest("Cannot delete room: it is assigned to one or more courses. Please reassign or delete those courses first.");
            }

            _context.Rooms.Remove(room);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return BadRequest("Database update failed: " + (ex.InnerException?.Message ?? ex.Message));
            }

            return NoContent();
        }

        private bool RoomExists(int id)
        {
            return _context.Rooms.Any(e => e.RoomId == id);
        }
    }
}
