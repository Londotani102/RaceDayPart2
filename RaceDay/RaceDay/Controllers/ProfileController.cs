using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Data;
using RaceDay.Models;

namespace RaceDay.Controllers
{
    /// <summary>View and update the currently logged-in user's profile.</summary>
    [Route("api/profile")]
    [ApiController]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly RaceDayDbContext _context;
        public ProfileController(RaceDayDbContext context) => _context = context;

        private (string role, int id) CurrentUser()
        {
            var id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var role = User.FindFirstValue(ClaimTypes.Role)!;
            return (role, id);
        }

        /// <summary>Get the profile of the logged-in user.</summary>
        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            var (role, id) = CurrentUser();

            if (role == "Participant")
            {
                var p = await _context.Participants.FindAsync(id);
                if (p == null) return NotFound();
                return Ok(new
                {
                    p.ParticipantId,
                    p.FirstName,
                    p.Surname,
                    p.Email,
                    p.Phone,
                    Role = "Participant"
                });
            }
            else
            {
                var o = await _context.Organisers.FindAsync(id);
                if (o == null) return NotFound();
                return Ok(new
                {
                    o.OrganiserId,
                    o.FirstName,
                    o.Surname,
                    o.Email,
                    o.Phone,
                    Role = "Organiser"
                });
            }
        }

        /// <summary>Update the profile of the logged-in user.</summary>
        [HttpPut]
        public async Task<IActionResult> UpdateProfile([FromBody] ProfileUpdateRequest request)
        {
            var (role, id) = CurrentUser();

            if (role == "Participant")
            {
                var p = await _context.Participants.FindAsync(id);
                if (p == null) return NotFound();
                if (!string.IsNullOrWhiteSpace(request.FirstName)) p.FirstName = request.FirstName;
                if (!string.IsNullOrWhiteSpace(request.Surname)) p.Surname = request.Surname;
                if (!string.IsNullOrWhiteSpace(request.Phone)) p.Phone = request.Phone;
            }
            else
            {
                var o = await _context.Organisers.FindAsync(id);
                if (o == null) return NotFound();
                if (!string.IsNullOrWhiteSpace(request.FirstName)) o.FirstName = request.FirstName;
                if (!string.IsNullOrWhiteSpace(request.Surname)) o.Surname = request.Surname;
                if (!string.IsNullOrWhiteSpace(request.Phone)) o.Phone = request.Phone;
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Profile updated." });
        }
    }
}