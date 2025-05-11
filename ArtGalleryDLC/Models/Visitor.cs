using System.Text.Json.Serialization;

namespace ArtGalleryDLC.Models
{
    public class Visitor : BaseEntity
    {
        public string FullName { get; set; }
        public string ContactInfo { get; set; }
        public List<string> VisitHistory { get; set; } = new List<string>();
        public List<string> PurchaseIds { get; set; } = new List<string>();

        [JsonIgnore]
        public List<Sale> Purchases { get; set; } = new List<Sale>();
    }
}