
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Data;
using RaceDay.Models;

namespace RaceDay.Controllers
{
    [Route("api/participants")]
    [ApiController]
    public class ParticipantsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public ParticipantsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: api/participants
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Participant>>>
            GetParticipants()
        {
            return Ok(await _context.Participants.ToListAsync());
        }

        // GET: api/participants/1
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Participant>>
            GetParticipant(int id)
        {
            var participant =
                await _context.Participants.FindAsync(id);

            if (participant == null)
                return NotFound();

            return Ok(participant);
        }

        // POST: api/participants
        [HttpPost]
        public async Task<ActionResult<Participant>>
            CreateParticipant([FromBody] Participant participant)
        {
            _context.Participants.Add(participant);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetParticipant),
                new { id = participant.ParticipantId },
                participant);
        }

        // PUT: api/participants/1
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateParticipant(
            int id, [FromBody] Participant participant)
        {
            if (id != participant.ParticipantId)
                return BadRequest("Participant IDs do not match.");

            var existing =
                await _context.Participants.FindAsync(id);

            if (existing == null)
                return NotFound();

            existing.FirstName = participant.FirstName;
            existing.Surname = participant.Surname;
            existing.Email = participant.Email;
            existing.Phone = participant.Phone;

            await _context.SaveChangesAsync();

            return Ok(existing);
        }

        // DELETE: api/participants/1
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteParticipant(int id)
        {
            var participant =
                await _context.Participants.FindAsync(id);

            if (participant == null)
                return NotFound();

            _context.Participants.Remove(participant);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
