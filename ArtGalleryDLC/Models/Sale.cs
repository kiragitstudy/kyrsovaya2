using System.Text.Json.Serialization;

namespace ArtGalleryDLC.Models
{
    //Продажа
    public class Sale : BaseEntity
    {
        public string ArtworkId { get; set; }
        public string BuyerId { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }

        [JsonIgnore]
        public Artwork Artwork { get; set; }
        [JsonIgnore]
        public Visitor Buyer { get; set; }
    }
}