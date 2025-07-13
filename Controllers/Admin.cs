using CRS.Data;
using CRS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CRS.Controllers
{
    public class Admin : Controller
    {
        private readonly ApplicationDBContext _context;
        public async Task<IActionResult> AdminAction()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var user = await _context.User.FindAsync(userId);
            if (user == null || user.Roles != RolesEnum.Admin)
                return Forbid(); // or return Unauthorized();

            return Ok("Admin Action Success");
        }

    }

}
