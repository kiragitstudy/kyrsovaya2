using System.Text.Json.Serialization;

namespace ArtGallery.Models
{
    public class Artwork : BaseEntity
    {
        public string Title { get; set; }
        public string ArtistId { get; set; }
        public int Year { get; set; }
        public string Genre { get; set; }
        public string Description { get; set; }
        public decimal EstimatedValue { get; set; }
        public ArtworkStatus Status { get; set; }

        [JsonIgnore]
        public Artist? Artist { get; set; }
    }
}