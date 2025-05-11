using System.Text.Json.Serialization;

namespace ArtGallery.Models
{
    public class Artist : BaseEntity
    {
        public string FullName { get; set; }
        public string Country { get; set; }
        public string LifeYears { get; set; }
        public string Style { get; set; }
        public List<string> ArtworkIds { get; set; } = new List<string>();

        [JsonIgnore]
        public List<Artwork> Artworks { get; set; } = new List<Artwork>();
    }
}