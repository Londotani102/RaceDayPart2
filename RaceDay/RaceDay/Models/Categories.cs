using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceDay.Models
{
    public class Categories
    {
        [Key]
        public int CategoryId { get; set; }

        public string CategoryName { get; set; } = string.Empty;
        public int MinAge { get; set; }
        public int MaxAge { get; set; }
        public double Distance { get; set; }

        public int EventId { get; set; }
        [ForeignKey("EventId")]
        public Event? Event { get; set; }
    }
}