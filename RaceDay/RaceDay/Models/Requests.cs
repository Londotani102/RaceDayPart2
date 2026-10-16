using System.ComponentModel.DataAnnotations;

namespace RaceDay.Models
{
    // ---------------- EVENT ----------------
    public class EventCreateRequest
    {
        [Required]
        public string EventName { get; set; } = string.Empty;

        public string description { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string Location { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public string RouteMapUrl { get; set; } = string.Empty;
        public int RouteId { get; set; }
    }

    // ---------------- CATEGORY ----------------
    public class CategoryCreateRequest
    {
        [Required]
        public string CategoryName { get; set; } = string.Empty;

        public int MinAge { get; set; }
        public int MaxAge { get; set; }
        public double Distance { get; set; }
        public int EventId { get; set; }
    }

    // ---------------- ROUTE ----------------
    public class RouteCreateRequest
    {
        public string RouteName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double Distance { get; set; }
        public string StartLocation { get; set; } = string.Empty;
        public string EndLocation { get; set; } = string.Empty;
        public string RouteMapUrl { get; set; } = string.Empty;
    }

    // ---------------- ENROLMENT ----------------
    public class EnrolmentCreateRequest
    {
        public int EventId { get; set; }
        public int CategoryId { get; set; }
    }

    // ---------------- RESULT ----------------
    public class ResultCreateRequest
    {
        public int EventEnrollmentId { get; set; }
        public TimeSpan FinishTime { get; set; }
        public int Position { get; set; }
    }

    // ---------------- PROFILE ----------------
    public class ProfileUpdateRequest
    {
        public string? FirstName { get; set; }
        public string? Surname { get; set; }
        public string? Phone { get; set; }
    }
}