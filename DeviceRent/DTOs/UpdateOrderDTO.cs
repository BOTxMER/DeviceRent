namespace DeviceRent.DTOs
{
    public class UpdateOrderDTO
    {
        public int Id { get; set; }
        public string? Status { get; set; }
        public string? Diller { get; set; }
        public string? Comment { get; set; }
    }
}
