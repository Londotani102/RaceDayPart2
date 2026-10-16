using System.ComponentModel.DataAnnotations;

namespace RaceDay.Models
{
    public class Routes
    {
        [Key]
        public int RouteId { get; set; }

        public string RouteName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double Distance { get; set; }
        public string StartLocation { get; set; } = string.Empty;
        public string EndLocation { get; set; } = string.Empty;
        public string RouteMapUrl { get; set; } = string.Empty;
    }
}