using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Models
{
    public class Event
    {
        [Key]
        public int EventId { get; set; }

        public string EventName { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string description { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public string RouteMapUrl { get; set; } = string.Empty;

        public int OrganiserId { get; set; }
        [ForeignKey("OrganiserId")]
        public Organiser? Organiser { get; set; }

        public int RouteId { get; set; }
        [ForeignKey("RouteId")]
        public Routes? Route { get; set; }
    }
}