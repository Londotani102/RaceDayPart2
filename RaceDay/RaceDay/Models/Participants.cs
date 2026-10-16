using System.Text.Json.Serialization;

namespace RaceDay.Models
{
    public class Participant
    {
        public int ParticipantId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string Surname { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;

        [JsonIgnore]
        public string PasswordHash { get; set; } = string.Empty;
    }
}