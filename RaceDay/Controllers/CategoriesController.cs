using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Data;
using RaceDay.Models;
using System.Security.Claims;

namespace RaceDay.Controllers
{
    [ApiController]
    [Route("api")]
    [Authorize]
    public class CategoriesController : ControllerBase
    {
        private readonly RaceDayContext _context;

        public CategoriesController(RaceDayContext context)
        {
            _context = context;
        }

        private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        /// <summary>Lists all categories for an event. Both roles.</summary>
        [HttpGet("events/{eventId}/categories")]
        public async Task<IActionResult> GetForEvent(string eventId)
        {
            bool eventExists = await _context.Events.AnyAsync(e => e.EventId == eventId);
            if (!eventExists) return NotFound("Event not found.");

            var categories = await _context.Categories
                .Where(c => c.EventId == eventId)
                .Select(c => new { c.CategoryId, c.EventId, c.CategoryName, c.Distance, c.EntryFee })
                .ToListAsync();

            return Ok(categories);
        }

        /// <summary>Gets one category by id. Both roles.</summary>
        [HttpGet("categories/{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var c = await _context.Categories.FindAsync(id);
            if (c == null) return NotFound("Category not found.");

            return Ok(new { c.CategoryId, c.EventId, c.CategoryName, c.Distance, c.EntryFee });
        }

        /// <summary>Creates a category for an event. Organiser only, own events only.</summary>
        [HttpPost("events/{eventId}/categories")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> Create(string eventId, CategoryDto dto)
        {
            var ev = await _context.Events.FindAsync(eventId);
            if (ev == null) return NotFound("Event not found.");
            if (ev.UserId != CurrentUserId) return Forbid();

            var category = new Category
            {
                EventId = eventId,
                CategoryName = dto.CategoryName,
                Distance = dto.Distance,
                EntryFee = dto.EntryFee
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = category.CategoryId },
                new { category.CategoryId, category.EventId, category.CategoryName, category.Distance, category.EntryFee });
        }

        /// <summary>Updates a category. Organiser only, own events only.</summary>
        [HttpPut("categories/{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> Update(string id, CategoryDto dto)
        {
            var category = await _context.Categories
                .Include(c => c.Event)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null) return NotFound("Category not found.");
            if (category.Event!.UserId != CurrentUserId) return Forbid();

            category.CategoryName = dto.CategoryName;
            category.Distance = dto.Distance;
            category.EntryFee = dto.EntryFee;

            await _context.SaveChangesAsync();
            return Ok(new { category.CategoryId, category.EventId, category.CategoryName, category.Distance, category.EntryFee });
        }

        /// <summary>Deletes a category. Organiser only, own events only.</summary>
        [HttpDelete("categories/{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> Delete(string id)
        {
            var category = await _context.Categories
                .Include(c => c.Event)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null) return NotFound("Category not found.");
            if (category.Event!.UserId != CurrentUserId) return Forbid();

            bool hasEntries = await _context.Entries.AnyAsync(e => e.CategoryId == id);
            if (hasEntries)
                return BadRequest("Cannot delete a category that already has enrolments.");

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}