using CRS.Data;
using CRS.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using CRS.Dtos;



namespace CRS.Controllers
{
    public class Authentication : ControllerBase

    {
        private readonly ApplicationDBContext _context;
        private readonly IConfiguration _config;

        public Authentication(ApplicationDBContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto userDto)
        {
            if (await _context.User.AnyAsync(u => u.Email == userDto.Email))
                return BadRequest("Email already exists.");

            var user = new User
            {
                Name = userDto.Name,
                Email = userDto.Email,
                Roles = RolesEnum.User,
                Password = new PasswordHasher<User>().HashPassword(null, userDto.Password)
            };

            _context.User.Add(user);
            await _context.SaveChangesAsync();

            return Ok("User registered successfully.");
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginData)
        {
            if (loginData == null || string.IsNullOrWhiteSpace(loginData.Email) || string.IsNullOrWhiteSpace(loginData.Password))
                return BadRequest("Email and password are required.");

            var user = await _context.User.SingleOrDefaultAsync(u => u.Email == loginData.Email);
            if (user == null)
                return Unauthorized("Invalid email.");

            var hasher = new PasswordHasher<User>();
            var result = hasher.VerifyHashedPassword(user, user.Password, loginData.Password);
            if (result != PasswordVerificationResult.Success)
                return Unauthorized("Invalid password.");

            var token = GenerateJwtToken(user);
            return Ok(new { token });
        }



        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Role, user.Roles.ToString()) // enum to string
    };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}
