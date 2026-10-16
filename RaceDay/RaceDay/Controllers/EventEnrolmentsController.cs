using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Data;
using RaceDay.Models;

namespace RaceDay.Controllers
{
    /// <summary>Event enrolments.</summary>
    [Route("api/enrolments")]
    [ApiController]
    [Authorize]
    public class EventEnrolmentsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;
        public EventEnrolmentsController(RaceDayDbContext context) => _context = context;

        /// <summary>View the logged-in participant's own enrolments.</summary>
        [HttpGet]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> GetMyEnrolments()
        {
            var participantId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var enrolments = await _context.EventEnrolments
                .Where(e => e.ParticipantId == participantId)
                .ToListAsync();

            return Ok(enrolments);
        }

        /// <summary>View a specific enrolment (participant must own it; organiser may view any).</summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetEventEnrolment(int id)
        {
            var role = User.FindFirstValue(ClaimTypes.Role)!;
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var enrolment = await _context.EventEnrolments.FindAsync(id);
            if (enrolment == null) return NotFound();

            if (role == "Participant" && enrolment.ParticipantId != userId)
                return Forbid();

            return Ok(enrolment);
        }

        /// <summary>Enrol in an event. Participant only.</summary>
        [HttpPost]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> CreateEventEnrolment([FromBody] EnrolmentCreateRequest request)
        {
            var participantId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var ev = await _context.Events.FindAsync(request.EventId);
            if (ev == null) return NotFound("Event not found.");

            var cat = await _context.Categories.FindAsync(request.CategoryId);
            if (cat == null) return NotFound("Category not found.");

            if (cat.EventId != request.EventId)
                return BadRequest("Category does not belong to the specified event.");

            var alreadyEnrolled = await _context.EventEnrolments
                .AnyAsync(e => e.EventId == request.EventId && e.ParticipantId == participantId);

            if (alreadyEnrolled)
                return Conflict("You are already enrolled in this event.");

            var newEnrolment = new EventEnrolment
            {
                EventId = request.EventId,
                CategoryId = request.CategoryId,
                ParticipantId = participantId,
                EnrolmentDate = DateTime.UtcNow,
                Status = "Confirmed"
            };

            _context.EventEnrolments.Add(newEnrolment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetEventEnrolment),
                new { id = newEnrolment.EventEnrolmentId },
                newEnrolment);
        }

        /// <summary>Cancel an enrolment. Participant may only cancel their own.</summary>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> DeleteEventEnrolment(int id)
        {
            var participantId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var enrolment = await _context.EventEnrolments.FindAsync(id);
            if (enrolment == null) return NotFound();
            if (enrolment.ParticipantId != participantId) return Forbid();

            _context.EventEnrolments.Remove(enrolment);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>View all enrolments for an event. Organiser only.</summary>
        [HttpGet("/api/events/{eventId:int}/enrolments")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> GetEnrolmentsForEvent(int eventId)
        {
            var eventExists = await _context.Events.AnyAsync(e => e.EventId == eventId);
            if (!eventExists) return NotFound("Event not found.");

            var enrolments = await _context.EventEnrolments
                .Where(e => e.EventId == eventId)
                .ToListAsync();

            return Ok(enrolments);
        }
    }
}