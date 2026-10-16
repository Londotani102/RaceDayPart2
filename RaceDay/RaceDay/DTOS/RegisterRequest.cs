using System.ComponentModel.DataAnnotations;

namespace RaceDay.DTOs
{
    public class RegisterRequest
    {
        [Required] public string FirstName { get; set; } = string.Empty;
        [Required] public string Surname { get; set; } = string.Empty;
        [Required][EmailAddress] public string Email { get; set; } = string.Empty;
        [Required][MinLength(6)] public string Password { get; set; } = string.Empty;

        [Required]
        [RegularExpression("Organiser|Participant",
            ErrorMessage = "Role must be 'Organiser' or 'Participant'.")]
        public string Role { get; set; } = "Participant";

        public string? Phone { get; set; }
    }
}