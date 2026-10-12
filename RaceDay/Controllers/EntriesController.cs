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
    public class EntriesController : ControllerBase
    {
        private readonly RaceDayContext _context;

        public EntriesController(RaceDayContext context)
        {
            _context = context;
        }

        private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        /// <summary>Enrols the logged-in Participant into an event category. Participant only.</summary>
        [HttpPost("entries")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> Enter(EntryDto dto)
        {
            var category = await _context.Categories
                .Include(c => c.Event)
                .FirstOrDefaultAsync(c => c.CategoryId == dto.CategoryId);

            if (category == null) return NotFound("Category not found.");

            // Block entering two categories of the same event
            bool alreadyEntered = await _context.Entries
                .AnyAsync(e => e.UserId == CurrentUserId
                            && e.Category!.EventId == category.EventId
                            && e.Status != "Cancelled");

            if (alreadyEntered)
                return BadRequest("You have already entered this event.");

            var entry = new Entry
            {
                UserId = CurrentUserId,
                CategoryId = dto.CategoryId,
                EntryDate = DateTime.UtcNow,
                Status = "Registered"
            };

            _context.Entries.Add(entry);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMyEntries), new
            {
                entry.EntryId,
                entry.CategoryId,
                EventId = category.EventId,
                EventName = category.Event!.EventName,
                CategoryName = category.CategoryName,
                entry.EntryDate,
                entry.Status
            });
        }

        /// <summary>Lists the logged-in Participant's own enrolments. Participant only.</summary>
        [HttpGet("users/me/entries")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> GetMyEntries()
        {
            var entries = await _context.Entries
                .Where(e => e.UserId == CurrentUserId)
                .Select(e => new
                {
                    e.EntryId,
                    e.CategoryId,
                    CategoryName = e.Category!.CategoryName,
                    EventId = e.Category.EventId,
                    EventName = e.Category.Event!.EventName,
                    e.EntryDate,
                    e.Status
                })
                .ToListAsync();

            return Ok(entries);
        }

        /// <summary>Cancels the Participant's own enrolment. Participant only.</summary>
        [HttpPut("entries/{id}/cancel")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> Cancel(string id)
        {
            var entry = await _context.Entries.FindAsync(id);
            if (entry == null) return NotFound("Entry not found.");
            if (entry.UserId != CurrentUserId) return Forbid();

            entry.Status = "Cancelled";
            await _context.SaveChangesAsync();
            return Ok(new { entry.EntryId, entry.Status });
        }

        /// <summary>Lists all enrolments for an event. Organiser only, own events only.</summary>
        [HttpGet("events/{eventId}/entries")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> GetForEvent(string eventId)
        {
            var ev = await _context.Events.FindAsync(eventId);
            if (ev == null) return NotFound("Event not found.");
            if (ev.UserId != CurrentUserId) return Forbid();

            var entries = await _context.Entries
                .Where(e => e.Category!.EventId == eventId)
                .Select(e => new
                {
                    e.EntryId,
                    e.UserId,
                    ParticipantName = e.Participant!.FullName,
                    e.CategoryId,
                    CategoryName = e.Category!.CategoryName,
                    e.EntryDate,
                    e.Status
                })
                .ToListAsync();

            return Ok(entries);
        }
    }
}