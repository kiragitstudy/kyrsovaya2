using System.Text.Json.Serialization;

namespace ArtGallery.Models
{
    //Выставка
    public class Exhibition : BaseEntity
    {
        public string Title { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Location { get; set; }
        public List<string> ArtworkIds { get; set; } = new List<string>();
        public decimal TicketPrice { get; set; }

        [JsonIgnore]
        public List<Artwork> Artworks { get; set; } = new List<Artwork>();
    }
}