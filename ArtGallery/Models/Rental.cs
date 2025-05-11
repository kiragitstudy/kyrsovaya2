using System.Text.Json.Serialization;

namespace ArtGallery.Models
{
    /// <summary>
    /// Аренда
    /// </summary>
    public class Rental : BaseEntity
    {
        public string ArtworkId { get; set; }
        public string RenterId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal Cost { get; set; }

        [JsonIgnore]
        public Artwork Artwork { get; set; }
        [JsonIgnore]
        public Visitor Renter { get; set; }
    }
}