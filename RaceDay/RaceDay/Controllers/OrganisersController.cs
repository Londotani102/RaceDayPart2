
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Data;
using RaceDay.Models;

namespace RaceDay.Controllers
{
    [Route("api/organisers")]
    [ApiController]
    public class OrganisersController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public OrganisersController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: api/organisers
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Organiser>>>
            GetOrganisers()
        {
            return Ok(await _context.Organisers.ToListAsync());
        }

        // GET: api/organisers/1
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Organiser>>
            GetOrganiser(int id)
        {
            var organiser =
                await _context.Organisers.FindAsync(id);

            if (organiser == null)
                return NotFound();

            return Ok(organiser);
        }

        // POST: api/organisers
        [HttpPost]
        public async Task<ActionResult<Organiser>>
            CreateOrganiser([FromBody] Organiser organiser)
        {
            _context.Organisers.Add(organiser);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetOrganiser),
                new { id = organiser.OrganiserId },
                organiser);
        }

        // PUT: api/organisers/1
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateOrganiser(
            int id, [FromBody] Organiser organiser)
        {
            if (id != organiser.OrganiserId)
                return BadRequest("Organiser IDs do not match.");

            var existing =
                await _context.Organisers.FindAsync(id);

            if (existing == null)
                return NotFound();

            existing.FirstName = organiser.FirstName;
            existing.Surname = organiser.Surname;
            existing.Email = organiser.Email;
            existing.Phone = organiser.Phone;

            await _context.SaveChangesAsync();

            return Ok(existing);
        }

        // DELETE: api/organisers/1
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteOrganiser(int id)
        {
            var organiser =
                await _context.Organisers.FindAsync(id);

            if (organiser == null)
                return NotFound();

            _context.Organisers.Remove(organiser);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
