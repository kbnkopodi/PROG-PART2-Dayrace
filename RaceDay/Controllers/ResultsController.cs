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
    public class ResultsController : ControllerBase
    {
        private readonly RaceDayContext _context;

        public ResultsController(RaceDayContext context)
        {
            _context = context;
        }

        private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        private static readonly string[] ValidStatuses = { "Finished", "DNF" };

        /// <summary>Captures a result for an entry. Organiser only, own events only.</summary>
        [HttpPost("results")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> Create(ResultDto dto)
        {
            if (!ValidStatuses.Contains(dto.Status))
                return BadRequest("Status must be Finished or DNF.");

            var entry = await _context.Entries
                .Include(e => e.Category)
                    .ThenInclude(c => c!.Event)
                .FirstOrDefaultAsync(e => e.EntryId == dto.EntryId);

            if (entry == null) return NotFound("Entry not found.");
            if (entry.Category!.Event!.UserId != CurrentUserId) return Forbid();

            bool resultExists = await _context.Results.AnyAsync(r => r.EntryId == dto.EntryId);
            if (resultExists)
                return BadRequest("A result already exists for this entry. Use PUT to update it.");

            var result = new Result
            {
                EntryId = dto.EntryId,
                FinishTime = dto.FinishTime,
                OverallPosition = dto.OverallPosition,
                Status = dto.Status
            };

            _context.Results.Add(result);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMyResults), new
            {
                result.ResultId,
                result.EntryId,
                result.FinishTime,
                result.OverallPosition,
                result.Status
            });
        }

        /// <summary>Updates a result. Organiser only, own events only.</summary>
        [HttpPut("results/{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> Update(string id, ResultDto dto)
        {
            if (!ValidStatuses.Contains(dto.Status))
                return BadRequest("Status must be Finished or DNF.");

            var result = await _context.Results
                .Include(r => r.Entry)
                    .ThenInclude(e => e!.Category)
                        .ThenInclude(c => c!.Event)
                .FirstOrDefaultAsync(r => r.ResultId == id);

            if (result == null) return NotFound("Result not found.");
            if (result.Entry!.Category!.Event!.UserId != CurrentUserId) return Forbid();

            result.FinishTime = dto.FinishTime;
            result.OverallPosition = dto.OverallPosition;
            result.Status = dto.Status;

            await _context.SaveChangesAsync();
            return Ok(new { result.ResultId, result.EntryId, result.FinishTime, result.OverallPosition, result.Status });
        }

        /// <summary>Lists all results for an event. Organiser only, own events only.</summary>
        [HttpGet("events/{eventId}/results")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> GetForEvent(string eventId)
        {
            var ev = await _context.Events.FindAsync(eventId);
            if (ev == null) return NotFound("Event not found.");
            if (ev.UserId != CurrentUserId) return Forbid();

            var results = await _context.Results
                .Where(r => r.Entry!.Category!.EventId == eventId)
                .OrderBy(r => r.OverallPosition)
                .Select(r => new
                {
                    r.ResultId,
                    r.EntryId,
                    ParticipantName = r.Entry!.Participant!.FullName,
                    CategoryName = r.Entry.Category!.CategoryName,
                    r.FinishTime,
                    r.OverallPosition,
                    r.Status
                })
                .ToListAsync();

            return Ok(results);
        }

        /// <summary>Lists the logged-in Participant's own results. Participant only.</summary>
        [HttpGet("users/me/results")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> GetMyResults()
        {
            var results = await _context.Results
                .Where(r => r.Entry!.UserId == CurrentUserId)
                .Select(r => new
                {
                    r.ResultId,
                    r.EntryId,
                    EventName = r.Entry!.Category!.Event!.EventName,
                    EventDate = r.Entry.Category.Event.EventDate,
                    CategoryName = r.Entry.Category.CategoryName,
                    r.FinishTime,
                    r.OverallPosition,
                    r.Status
                })
                .ToListAsync();

            return Ok(results);
        }
    }
}