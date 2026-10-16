using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RaceDay.Data;
using RaceDay.DTOs;
using RaceDay.Models;

namespace RaceDay.Controllers
{
    /// <summary>Registration and login for both Participants and Organisers.</summary>
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly RaceDayDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(RaceDayDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        /// <summary>Register a new Participant or Organiser account.</summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var role = request.Role.Trim();

            if (!string.Equals(role, "Participant", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(role, "Organiser", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Role must be 'Participant' or 'Organiser'.");
            }

            var email = request.Email.Trim().ToLowerInvariant();

            bool emailTaken =
                await _context.Participants.AnyAsync(p => p.Email.ToLower() == email) ||
                await _context.Organisers.AnyAsync(o => o.Email.ToLower() == email);

            if (emailTaken)
                return BadRequest("An account with this email already exists.");

            if (string.Equals(role, "Participant", StringComparison.OrdinalIgnoreCase))
            {
                var participant = new Participant
                {
                    FirstName = request.FirstName.Trim(),
                    Surname = request.Surname.Trim(),
                    Email = email,
                    Phone = request.Phone?.Trim() ?? string.Empty
                };

                var hasher = new PasswordHasher<Participant>();
                participant.PasswordHash = hasher.HashPassword(participant, request.Password);

                _context.Participants.Add(participant);
            }
            else
            {
                var organiser = new Organiser
                {
                    FirstName = request.FirstName.Trim(),
                    Surname = request.Surname.Trim(),
                    Email = email,
                    Phone = request.Phone?.Trim() ?? string.Empty
                };

                var hasher = new PasswordHasher<Organiser>();
                organiser.PasswordHash = hasher.HashPassword(organiser, request.Password);

                _context.Organisers.Add(organiser);
            }

            await _context.SaveChangesAsync();

            return StatusCode(201, new
            {
                message = $"{role} account created successfully."
            });
        }

        /// <summary>Authenticate and receive a JWT token.</summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var email = request.Email.Trim().ToLowerInvariant();

            var participant = await _context.Participants
                .FirstOrDefaultAsync(p => p.Email.ToLower() == email);
            var organiser = await _context.Organisers
                .FirstOrDefaultAsync(o => o.Email.ToLower() == email);

            string role, userId, firstName, surname;
            bool passwordValid;

            if (participant != null)
            {
                var hasher = new PasswordHasher<Participant>();
                passwordValid = !string.IsNullOrWhiteSpace(participant.PasswordHash) &&
                    hasher.VerifyHashedPassword(
                        participant, participant.PasswordHash, request.Password)
                    != PasswordVerificationResult.Failed;

                role = "Participant";
                userId = participant.ParticipantId.ToString();
                firstName = participant.FirstName;
                surname = participant.Surname;
            }
            else if (organiser != null)
            {
                var hasher = new PasswordHasher<Organiser>();
                passwordValid = !string.IsNullOrWhiteSpace(organiser.PasswordHash) &&
                    hasher.VerifyHashedPassword(
                        organiser, organiser.PasswordHash, request.Password)
                    != PasswordVerificationResult.Failed;

                role = "Organiser";
                userId = organiser.OrganiserId.ToString();
                firstName = organiser.FirstName;
                surname = organiser.Surname;
            }
            else
            {
                return Unauthorized(new { message = "Invalid email or password." });
            }

            if (!passwordValid)
                return Unauthorized(new { message = "Invalid email or password." });

            var keyValue = _configuration["Jwt:Key"]!;
            var issuer = _configuration["Jwt:Issuer"]!;
            var audience = _configuration["Jwt:Audience"]!;

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, email),
                new Claim(ClaimTypes.Role, role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyValue));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: credentials);

            return Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                user = new { userId, firstName, surname, email, role }
            });
        }
    }
}