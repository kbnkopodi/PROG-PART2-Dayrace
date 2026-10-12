using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Data;
using RaceDay.Models;
using System.Security.Claims;

namespace RaceDay.Controllers
{
    [ApiController]
    [Route("api/users/me")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly RaceDayContext _context;

        public UsersController(RaceDayContext context)
        {
            _context = context;
        }

        private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        /// <summary>Returns the logged-in user's own profile. Both roles.</summary>
        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            var user = await _context.Users.FindAsync(CurrentUserId);
            if (user == null) return NotFound("User not found.");

            return Ok(new { user.UserId, user.FullName, user.Email, user.Role });
        }

        /// <summary>Updates the logged-in user's own profile. Both roles.</summary>
        [HttpPut]
        public async Task<IActionResult> UpdateProfile(ProfileDto dto)
        {
            var user = await _context.Users.FindAsync(CurrentUserId);
            if (user == null) return NotFound("User not found.");

            bool emailTaken = await _context.Users
                .AnyAsync(u => u.Email == dto.Email && u.UserId != CurrentUserId);
            if (emailTaken)
                return BadRequest("That email is already in use.");

            user.FullName = dto.FullName;
            user.Email = dto.Email;

            await _context.SaveChangesAsync();
            return Ok(new { user.UserId, user.FullName, user.Email, user.Role });
        }
    }
}