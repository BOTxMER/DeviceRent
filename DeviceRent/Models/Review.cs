using System.Text.Json.Serialization;

namespace DeviceRent.Models
{
    public class Review
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string Date { get; set; } = string.Empty;

        public int OrderId { get; set; }
        [JsonIgnore]
        public Order Order { get; set; } = null!;
    }
}
