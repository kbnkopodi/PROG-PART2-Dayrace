using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Data;
using RaceDay.Models;
using System.Security.Claims;

namespace RaceDay.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // every endpoint needs a logged-in user
    public class EventsController : ControllerBase
    {
        private readonly RaceDayContext _context;

        public EventsController(RaceDayContext context)
        {
            _context = context;
        }

        private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        private static readonly string[] ValidTypes = { "Run", "Walk", "Cycle" };

        /// <summary>Lists all events. Both roles.</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var events = await _context.Events
                .Select(e => new
                {
                    e.EventId,
                    e.EventName,
                    e.Description,
                    e.EventType,
                    e.EventDate,
                    e.Location,
                    e.Distance,
                    e.UserId
                })
                .ToListAsync();

            return Ok(events);
        }

        /// <summary>Gets one event by id. Both roles.</summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null) return NotFound("Event not found.");

            return Ok(new
            {
                ev.EventId,
                ev.EventName,
                ev.Description,
                ev.EventType,
                ev.EventDate,
                ev.Location,
                ev.Distance,
                ev.UserId
            });
        }

        /// <summary>Creates an event. Organiser only.</summary>
        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> Create(EventDto dto)
        {
            if (!ValidTypes.Contains(dto.EventType))
                return BadRequest("EventType must be Run, Walk or Cycle.");

            var ev = new Event
            {
                UserId = CurrentUserId,
                EventName = dto.EventName,
                Description = dto.Description,
                EventType = dto.EventType,
                EventDate = dto.EventDate,
                Location = dto.Location,
                Distance = dto.Distance
            };

            _context.Events.Add(ev);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = ev.EventId }, new
            {
                ev.EventId,
                ev.EventName,
                ev.Description,
                ev.EventType,
                ev.EventDate,
                ev.Location,
                ev.Distance
            });
        }

        /// <summary>Updates an event. Organiser only, own events only.</summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> Update(string id, EventDto dto)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null) return NotFound("Event not found.");
            if (ev.UserId != CurrentUserId) return Forbid();

            if (!ValidTypes.Contains(dto.EventType))
                return BadRequest("EventType must be Run, Walk or Cycle.");

            ev.EventName = dto.EventName;
            ev.Description = dto.Description;
            ev.EventType = dto.EventType;
            ev.EventDate = dto.EventDate;
            ev.Location = dto.Location;
            ev.Distance = dto.Distance;

            await _context.SaveChangesAsync();
            return Ok(new
            {
                ev.EventId,
                ev.EventName,
                ev.Description,
                ev.EventType,
                ev.EventDate,
                ev.Location,
                ev.Distance
            });
        }

        /// <summary>Deletes an event. Organiser only, own events only.</summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> Delete(string id)
        {
            var ev = await _context.Events.FindAsync(id);
            if (ev == null) return NotFound("Event not found.");
            if (ev.UserId != CurrentUserId) return Forbid();

            bool hasCategories = await _context.Categories.AnyAsync(c => c.EventId == id);
            if (hasCategories)
                return BadRequest("Delete this event's categories first.");

            _context.Events.Remove(ev);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

}
