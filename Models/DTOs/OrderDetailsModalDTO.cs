namespace DSJsBookStore.Models.DTOs
{
    public class OrderDetailsModalDTO
    {
        public string? DivId { get; set; }
        public IEnumerable<OrderDetails>? OrderDetails { get; set; }
    }
}

