using System.ComponentModel.DataAnnotations.Schema;

namespace DeviceRent.Models
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime StartDate { get; set; }
        public string DeviceModel { get; set; } = string.Empty;
        public int Hour { get; set; }
        public string Client { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? EndDate { get; set; }
        public string? Diller { get; set; } = "Не назначен";
        public string DeliveryAddress { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public List<string> Comments { get; set; } = new();
        public Review? Review { get; set; }
        public int TotalAmount { get; set; }
    }
}
