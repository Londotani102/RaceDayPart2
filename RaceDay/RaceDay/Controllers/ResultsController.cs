using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Data;
using RaceDay.Models;

namespace RaceDay.Controllers
{
    /// <summary>Race results.</summary>
    [Route("api/results")]
    [ApiController]
    [Authorize]
    public class ResultsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;
        public ResultsController(RaceDayDbContext context) => _context = context;

        /// <summary>View all results. Organiser only.</summary>
        [HttpGet]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> GetResults() =>
            Ok(await _context.Results.ToListAsync());

        /// <summary>View the logged-in participant's own results.</summary>
        [HttpGet("me")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> GetMyResults()
        {
            var participantId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var results = await _context.Results
                .Include(r => r.EventEnrolment)
                .Where(r => r.EventEnrolment!.ParticipantId == participantId)
                .ToListAsync();

            return Ok(results);
        }

        /// <summary>View a specific result.</summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetResult(int id)
        {
            var role = User.FindFirstValue(ClaimTypes.Role)!;
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var result = await _context.Results
                .Include(r => r.EventEnrolment)
                .FirstOrDefaultAsync(r => r.ResultId == id);

            if (result == null) return NotFound();

            if (role == "Participant" && result.EventEnrolment!.ParticipantId != userId)
                return Forbid();

            return Ok(result);
        }

        /// <summary>Record a race result. Organiser only.</summary>
        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> CreateResult([FromBody] ResultCreateRequest request)
        {
            var enrolment = await _context.EventEnrolments
                .FindAsync(request.EventEnrollmentId);
            if (enrolment == null) return NotFound("Enrolment not found.");

            var newResult = new Result
            {
                EventEnrollmentId = request.EventEnrollmentId,
                FinishTime = request.FinishTime,
                Position = request.Position,
                RecordedAt = DateTime.UtcNow
            };

            _context.Results.Add(newResult);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetResult), new { id = newResult.ResultId }, newResult);
        }

        /// <summary>Update a race result. Organiser only.</summary>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> UpdateResult(int id, [FromBody] ResultCreateRequest request)
        {
            var existing = await _context.Results.FindAsync(id);
            if (existing == null) return NotFound();

            existing.FinishTime = request.FinishTime;
            existing.Position = request.Position;
            existing.RecordedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(existing);
        }
    }
}