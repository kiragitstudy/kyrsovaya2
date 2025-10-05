using System.Text.Json.Serialization;

namespace ArtGallery.Models
{
    public class Ticket : BaseEntity
    {
        public string? ExhibitionId { get; set; }
        public string? VisitorId { get; set; }
        public DateTime VisitDate { get; set; }
        public decimal Price { get; set; }
        public TicketStatus Status { get; set; }

        [JsonIgnore]
        public Exhibition? Exhibition { get; set; }
        [JsonIgnore]
        public Visitor? Visitor { get; set; }
    }
}
