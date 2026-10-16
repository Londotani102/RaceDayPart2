using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Data;
using RaceDay.Models;

namespace RaceDay.Controllers
{
    /// <summary>Manage race events.</summary>
    [Route("api/events")]
    [ApiController]
    [Authorize]
    public class EventsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;
        public EventsController(RaceDayDbContext context) => _context = context;

        /// <summary>View all events. Any authenticated user.</summary>
        [HttpGet]
        public async Task<IActionResult> GetEvents() =>
            Ok(await _context.Events.ToListAsync());

        /// <summary>View a single event. Any authenticated user.</summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetEvent(int id)
        {
            var ev = await _context.Events.FindAsync(id);
            return ev == null ? NotFound() : Ok(ev);
        }

        /// <summary>Create an event. Organiser only.</summary>
        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> CreateEvent([FromBody] EventCreateRequest request)
        {
            var organiserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var ev = new Event
            {
                EventName = request.EventName,
                description = request.description,
                EventDate = request.EventDate,
                Location = request.Location,
                EventType = request.EventType,
                RouteMapUrl = request.RouteMapUrl,
                RouteId = request.RouteId,
                OrganiserId = organiserId
            };

            _context.Events.Add(ev);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetEvent), new { id = ev.EventId }, ev);
        }

        /// <summary>Update an event. Only the owning Organiser.</summary>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> UpdateEvent(int id, [FromBody] EventCreateRequest request)
        {
            var organiserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var existing = await _context.Events.FindAsync(id);
            if (existing == null) return NotFound();
            if (existing.OrganiserId != organiserId) return Forbid();

            existing.EventName = request.EventName;
            existing.description = request.description;
            existing.EventDate = request.EventDate;
            existing.Location = request.Location;
            existing.EventType = request.EventType;
            existing.RouteId = request.RouteId;
            existing.RouteMapUrl = request.RouteMapUrl;

            await _context.SaveChangesAsync();
            return Ok(existing);
        }

        /// <summary>Delete an event. Only the owning Organiser.</summary>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var organiserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var ev = await _context.Events.FindAsync(id);
            if (ev == null) return NotFound();
            if (ev.OrganiserId != organiserId) return Forbid();

            _context.Events.Remove(ev);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}