using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Models
{
    public class EventEnrolment
    {
        [Key]
        public int EventEnrolmentId { get; set; }

        public int CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public Categories? Category { get; set; }

        public DateTime EnrolmentDate { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Confirmed";

        public int ParticipantId { get; set; }
        [ForeignKey("ParticipantId")]
        public Participant? Participant { get; set; }

        public int EventId { get; set; }
        [ForeignKey("EventId")]
        public Event? Event { get; set; }

        public Result? Result { get; set; }
    }
}