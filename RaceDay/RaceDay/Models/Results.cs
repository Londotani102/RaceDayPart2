using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Models
{
    public class Result
    {
        [Key]
        public int ResultId { get; set; }

        public int EventEnrollmentId { get; set; }
        [ForeignKey("EventEnrollmentId")]
        public EventEnrolment? EventEnrolment { get; set; }

        public TimeSpan FinishTime { get; set; }
        public int Position { get; set; }
        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
    }
}